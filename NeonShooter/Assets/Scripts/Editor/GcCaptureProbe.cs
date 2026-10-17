// Compiled out unless the GC_PROBE scripting define is set (Project Settings > Player > Scripting Define Symbols).
#if GC_PROBE
using System.Collections.Generic;
using System.Text;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// One-click profiler captures for the shooter.
/// GC mode force-fires the gun and reports the watched GC rows; Timing mode records with editor profiling on
/// while the user fires manually with the real mouse, then reports the top rows by self time.
/// Run via Tools > GC Probe.
/// </summary>
/// <remarks>
/// Kept inert by default so nothing probe-related exists in the domain while profiling the demonstrator's own performance.
/// </remarks>
// NoAutoStaticsCleanup: capture state lives in SessionState and the fire accumulator is reset explicitly per capture.
[NoAutoStaticsCleanup]
[InitializeOnLoad]
public static class GcCaptureProbe
{
    const float Duration = 10f;
    const float FireRate = 30f;
    const string PendingKey = "GcProbe.pending";
    const string AnalyzeKey = "GcProbe.analyze";
    const string TimingKey = "GcProbe.timing";

    // Profiler rows to aggregate; matched by substring against the hierarchy item name.
    // Once a row matches, its children are not searched, so nested matches never double count.
    static readonly string[] WatchNames =
    {
        "PhysicsPoseProvider.OnEnable",
        "PhysicsPoseProvider.OnDisable",
        "PhysicsWorldProvider.OnEnable",
        "PhysicsWorldProvider.OnDisable",
        "GetBatchTransformObjects",
        "PhysicsBody.SetBatchTransform",
        "Enemy.OnDisable",
        "Bullet.OnDisable",
        "GUI.Repaint",
    };

    static GcCaptureProbe()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        // A domain reload happens on entering play, so re-arm the in-play driver from the session flag.
        if (EditorApplication.isPlayingOrWillChangePlaymode && SessionState.GetBool(PendingKey, false))
            EditorApplication.update += DriveCapture;
    }

    [MenuItem("Tools/GC Probe/Capture GC (auto fire)")]
    static void StartCapture() => StartCapture(timing: false);

    [MenuItem("Tools/GC Probe/Capture Timing (fire manually)")]
    static void StartTimingCapture() => StartCapture(timing: true);

    static void StartCapture(bool timing)
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[GcProbe] Already in play mode; exit play first.");
            return;
        }

        SessionState.SetBool(PendingKey, true);
        SessionState.SetBool(AnalyzeKey, false);
        SessionState.SetBool(TimingKey, timing);
        s_FireAccumulator = 0f;

        ProfilerDriver.ClearAllFrames();

        // Timing mode profiles the editor too, so the EditorLoop breaks down instead of showing as one opaque row.
        ProfilerDriver.profileEditor = timing;
        ProfilerDriver.enabled = true;

        Debug.Log(timing
            ? $"[GcProbe] Capturing {Duration:F0}s of play. FIRE MANUALLY with the mouse for the whole window."
            : $"[GcProbe] Capturing {Duration:F0}s of play with forced fire at {FireRate:F0}/s...");
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        // Arm the in-play driver here too, covering projects with domain reload disabled where the static ctor does not re-run on play entry.
        if (change == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
        {
            EditorApplication.update -= DriveCapture;
            EditorApplication.update += DriveCapture;
        }

        if (change == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(AnalyzeKey, false))
        {
            SessionState.SetBool(AnalyzeKey, false);
            ProfilerDriver.enabled = false;
            ProfilerDriver.profileEditor = false;

            if (SessionState.GetBool(TimingKey, false))
                AnalyzeTiming();
            else
                Analyze();
        }
    }

    // Runs every editor tick while the capture window is open: force-fires the gun, then ends the capture.
    static void DriveCapture()
    {
        if (!EditorApplication.isPlaying)
            return;

        var elapsed = Time.timeSinceLevelLoad;

        if (elapsed >= Duration)
        {
            EditorApplication.update -= DriveCapture;
            SessionState.SetBool(PendingKey, false);
            SessionState.SetBool(AnalyzeKey, true);
            EditorApplication.ExitPlaymode();
            return;
        }

        // Timing mode leaves the input to the user so real mouse-driven costs are captured.
        if (SessionState.GetBool(TimingKey, false))
            return;

        // Fire directly (SendMessage reaches the private Fire method) so the capture needs no held mouse button.
        var gun = Object.FindAnyObjectByType<GunController>();
        if (gun == null)
            return;

        s_FireAccumulator += Time.deltaTime * FireRate;

        while (s_FireAccumulator >= 1f)
        {
            s_FireAccumulator -= 1f;
            gun.SendMessage("Fire", SendMessageOptions.DontRequireReceiver);
        }
    }

    // Walk every captured frame's main-thread hierarchy and aggregate the watched rows.
    static void Analyze()
    {
        var first = ProfilerDriver.firstFrameIndex;
        var last = ProfilerDriver.lastFrameIndex;

        if (first < 0 || last < first)
        {
            Debug.LogWarning("[GcProbe] No profiler frames captured.");
            return;
        }

        var totals = new Dictionary<string, RowTotal>();
        var frameCount = 0;

        for (var frame = first; frame <= last; ++frame)
        {
            using var view = ProfilerDriver.GetHierarchyFrameDataView(frame, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnGcMemory, false);
            if (view == null || !view.valid)
                continue;

            ++frameCount;
            Accumulate(view, view.GetRootItemID(), totals);
        }

        var report = new StringBuilder();
        report.AppendLine($"[GcProbe] {frameCount} frames analysed ({first}..{last}). Watched rows, worst first by total GC:");
        report.AppendLine($"{"Row",-42} {"Total GC",12} {"Calls",8} {"GC/call",10} {"Frames hit",10} {"Worst frame",12}");

        foreach (var pair in Sorted(totals))
        {
            var t = pair.Value;
            var perCall = t.calls > 0 ? t.gcBytes / t.calls : 0;
            report.AppendLine($"{pair.Key,-42} {FormatBytes(t.gcBytes),12} {t.calls,8:F0} {FormatBytes(perCall),10} {t.framesHit,10} {FormatBytes(t.worstFrameBytes),12}");
        }

        Debug.Log(report.ToString());
    }

    // Aggregate self time per row name across every captured frame and report the biggest costs.
    // Frames are also split into a slow and a fast half by frame time, so a cost that only exists while firing shows a lopsided split.
    static void AnalyzeTiming()
    {
        var first = ProfilerDriver.firstFrameIndex;
        var last = ProfilerDriver.lastFrameIndex;

        if (first < 0 || last < first)
        {
            Debug.LogWarning("[GcProbe] No profiler frames captured.");
            return;
        }

        // First pass: frame times, to find the median split.
        var frameTimes = new List<float>();

        for (var frame = first; frame <= last; ++frame)
        {
            using var view = ProfilerDriver.GetHierarchyFrameDataView(frame, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnSelfTime, false);
            frameTimes.Add(view != null && view.valid ? view.frameTimeMs : 0f);
        }

        var sorted = new List<float>(frameTimes);
        sorted.Sort();
        var median = sorted[sorted.Count / 2];

        // Second pass: self time per row name, accumulated separately for slow-half and fast-half frames.
        var slow = new Dictionary<string, float>();
        var fast = new Dictionary<string, float>();
        var slowFrames = 0;
        var fastFrames = 0;

        for (var frame = first; frame <= last; ++frame)
        {
            using var view = ProfilerDriver.GetHierarchyFrameDataView(frame, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnSelfTime, false);
            if (view == null || !view.valid)
                continue;

            var isSlow = view.frameTimeMs > median;
            if (isSlow)
                ++slowFrames;
            else
                ++fastFrames;

            AccumulateSelfTime(view, view.GetRootItemID(), isSlow ? slow : fast);
        }

        // Rank by how much worse a row is per slow frame than per fast frame: that difference is the firing overhead.
        var names = new HashSet<string>(slow.Keys);
        names.UnionWith(fast.Keys);

        var ranked = new List<(string name, float slowPerFrame, float fastPerFrame)>();

        foreach (var name in names)
        {
            var s = slowFrames > 0 && slow.TryGetValue(name, out var st) ? st / slowFrames : 0f;
            var f = fastFrames > 0 && fast.TryGetValue(name, out var ft) ? ft / fastFrames : 0f;
            ranked.Add((name, s, f));
        }

        ranked.Sort((a, b) => (b.slowPerFrame - b.fastPerFrame).CompareTo(a.slowPerFrame - a.fastPerFrame));

        var report = new StringBuilder();
        report.AppendLine($"[GcProbe] Timing: {frameTimes.Count} frames, median {median:F2} ms. Slow half {slowFrames} frames avg {Average(frameTimes, median, above: true):F2} ms, fast half {fastFrames} frames avg {Average(frameTimes, median, above: false):F2} ms.");
        report.AppendLine("Top rows by (slow-frame ms - fast-frame ms), i.e. where the slow frames spend their extra time:");
        report.AppendLine($"{"Row",-60} {"Slow ms/f",10} {"Fast ms/f",10} {"Delta",8}");

        for (var i = 0; i < ranked.Count && i < 25; ++i)
        {
            var (name, s, f) = ranked[i];
            report.AppendLine($"{Truncate(name, 60),-60} {s,10:F3} {f,10:F3} {s - f,8:F3}");
        }

        Debug.Log(report.ToString());
    }

    // Sum every item's SELF time into its name bucket; recurse the whole tree (self times never double count).
    static void AccumulateSelfTime(HierarchyFrameDataView view, int itemId, Dictionary<string, float> totals)
    {
        var localChildren = new List<int>();
        view.GetItemChildren(itemId, localChildren);

        foreach (var child in localChildren)
        {
            var self = view.GetItemColumnDataAsSingle(child, HierarchyFrameDataView.columnSelfTime);
            if (self > 0f)
            {
                var name = view.GetItemName(child);
                totals.TryGetValue(name, out var total);
                totals[name] = total + self;
            }

            AccumulateSelfTime(view, child, totals);
        }
    }

    // Averages the values on one side of the threshold: those strictly above it when above is true, otherwise those at or below it.
    static float Average(List<float> values, float threshold, bool above)
    {
        var sum = 0f;
        var n = 0;

        foreach (var v in values)
        {
            if (above ? v > threshold : v <= threshold)
            {
                sum += v;
                ++n;
            }
        }

        return n > 0 ? sum / n : 0f;
    }

    static string Truncate(string value, int max) => value.Length <= max ? value : value.Substring(0, max - 1) + "…";

    // Depth-first walk; a matched item is recorded with its inclusive values and not descended into.
    static void Accumulate(HierarchyFrameDataView view, int itemId, Dictionary<string, RowTotal> totals)
    {
        var localChildren = new List<int>();
        view.GetItemChildren(itemId, localChildren);

        foreach (var child in localChildren)
        {
            var name = view.GetItemName(child);
            var matched = MatchWatchName(name);

            if (matched != null)
            {
                var gc = view.GetItemColumnDataAsSingle(child, HierarchyFrameDataView.columnGcMemory);
                var calls = view.GetItemColumnDataAsSingle(child, HierarchyFrameDataView.columnCalls);

                if (!totals.TryGetValue(matched, out var total))
                    total = new RowTotal();

                total.gcBytes += gc;
                total.calls += calls;
                if (gc > 0)
                {
                    ++total.framesHit;
                    total.worstFrameBytes = Mathf.Max(total.worstFrameBytes, gc);
                }

                totals[matched] = total;
                continue;
            }

            Accumulate(view, child, totals);
        }
    }

    static string MatchWatchName(string name)
    {
        foreach (var watch in WatchNames)
        {
            if (name.Contains(watch))
                return watch;
        }

        return null;
    }

    static IEnumerable<KeyValuePair<string, RowTotal>> Sorted(Dictionary<string, RowTotal> totals)
    {
        var list = new List<KeyValuePair<string, RowTotal>>(totals);
        list.Sort((a, b) => b.Value.gcBytes.CompareTo(a.Value.gcBytes));
        return list;
    }

    static string FormatBytes(float bytes)
    {
        if (bytes >= 1024f * 1024f)
            return $"{bytes / (1024f * 1024f):F2} MB";
        if (bytes >= 1024f)
            return $"{bytes / 1024f:F1} KB";
        return $"{bytes:F0} B";
    }

    struct RowTotal
    {
        public float gcBytes;
        public float calls;
        public int framesHit;
        public float worstFrameBytes;
    }

    static float s_FireAccumulator;
}
#endif
