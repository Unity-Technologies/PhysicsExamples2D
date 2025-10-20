using UnityEngine;

public class TurnOffUrpDebugManager : MonoBehaviour
{
    private void Start()
    {
        // We don't want this appearing all the time.
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }
}
