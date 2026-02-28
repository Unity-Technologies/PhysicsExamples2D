using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, sourceClassName: "IWorldSceneTransformChanged")]
    public interface ITestWorldTransformChanged
    {
        void TransformChanged();
    }
}