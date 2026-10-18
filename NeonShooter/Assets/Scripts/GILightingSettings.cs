using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// Puts the scene's lighting override on the options menu.
/// The override lives in the lighting package, which cannot see the game's settings registry, so this component on the same object registers its values.
/// </summary>
[RequireComponent(typeof(GILightingOverride))]
public sealed class GILightingSettings : MonoBehaviour
{
    void Awake()
    {
        var lighting = GetComponent<GILightingOverride>();
        var background = lighting.background;

        GameSettings.RegisterToggle(this, GameSettings.LightingTab, "Cascade Lighting", () => lighting.lightingEnabled, v => lighting.lightingEnabled = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.LightingTab, "Cascades", 1, GILightingOverride.MaxCascadeCount, () => lighting.cascadeCount, v => lighting.cascadeCount = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Render Scale", GILightingOverride.MinRenderScale, 1f, () => lighting.renderScale, v => lighting.renderScale = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Step Stride", 0.05f, 1f, () => lighting.stepStride, v => lighting.stepStride = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Exposure", -8f, 8f, () => lighting.exposure, v => lighting.exposure = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Grid Cells", 2f, 60f, () => background.cellsAcross, v => background.cellsAcross = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Grid Line Thickness", 0f, 0.2f, () => background.lineThickness, v => background.lineThickness = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Grid Cells Variation", 0f, 30f, () => background.cellsVariation, v => background.cellsVariation = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Grid Tilt Interval", 0f, 30f, () => background.tiltInterval, v => background.tiltInterval = v);
        GameSettings.RegisterSlider(this, GameSettings.LightingTab, "Grid Perspective", 0.3f, 6f, () => background.perspective, v => background.perspective = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);
}
