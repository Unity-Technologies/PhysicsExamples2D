using UnityEngine;

public class Joint2D_BreakAction : MonoBehaviour
{
    void OnJointBreak2D(Joint2D joint)
    {
        string message;

        switch (joint.breakAction)
        {
            case JointBreakAction2D.CallbackOnly:
                message = "- Callback Only\n- More Callbacks possible";
                break;

            case JointBreakAction2D.Disable:
                message = "- Callback\n- Joint Disabled";
                break;

            case JointBreakAction2D.Destroy:
                message = "- Callback\n - Joint Destroyed";
                break;

            // NOTE: These will never be called!
            case JointBreakAction2D.Ignore:
            default:
                message = string.Empty;
                break;
        }

        GetComponentInChildren<TextMesh>().text = message;
    }
}
