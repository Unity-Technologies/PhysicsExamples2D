using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Maps between the screen and the plane physics runs on, for one example's camera.
/// The manipulator drives pan, zoom and picking through this, so the same drag and explode behavior works whether the example frames its scene with an orthographic or a perspective camera.
/// </summary>
/// <remarks>
/// Use <see cref="For"/> to get the one matching a camera.
/// The plane itself comes from the physics world rather than from a per example setting, so an example that switches the world to XZ is handled without extra configuration.
/// </remarks>
public abstract class CameraProjection
{
    /// <summary>
    /// Returns the projection matching the specified camera, orthographic or perspective.
    /// </summary>
    public static CameraProjection For(Camera camera)
    {
        return camera.orthographic ? new Orthographic(camera) : new Perspective(camera);
    }

    /// <summary>
    /// Converts a screen position into the point on the physics plane underneath it.
    /// </summary>
    public abstract Vector2 ScreenToPlanePoint(Vector2 screenPosition);

    /// <summary>
    /// Frames the specified height of the physics plane, in meters, divided by the zoom factor.
    /// </summary>
    public abstract void SetFraming(float cameraSize, float zoom);

    /// <summary>
    /// Moves the camera by the specified distance measured on the physics plane, in meters.
    /// </summary>
    public abstract void PanBy(Vector2 planeDelta);

    /// <summary>
    /// Moves the camera to look at the specified point on the physics plane, keeping its current distance and orientation.
    /// </summary>
    public abstract void MoveTo(Vector2 planePosition);

    /// <summary>
    /// The camera this projection drives.
    /// </summary>
    public Camera camera { get; }

    // Converts a point on the physics plane into world space, using whichever plane the default world currently runs on.
    protected static Vector3 PlaneToWorld(Vector2 planePosition) => PhysicsMath.ToPosition3D(planePosition, Vector3.zero, PhysicsWorld.defaultWorld);

    // Converts a world position onto the physics plane.
    protected static Vector2 WorldToPlane(Vector3 worldPosition) => PhysicsMath.ToPosition2D(worldPosition, PhysicsWorld.defaultWorld);

    protected CameraProjection(Camera camera) => this.camera = camera;

    // The orthographic case, where the camera looks straight down at the physics plane and screen to world needs no ray at all.
    private sealed class Orthographic : CameraProjection
    {
        public override Vector2 ScreenToPlanePoint(Vector2 screenPosition) => camera.ScreenToWorldPoint(screenPosition);

        public override void SetFraming(float cameraSize, float zoom) => camera.orthographicSize = cameraSize / math.max(zoom, 0.00001f);

        public override void PanBy(Vector2 planeDelta) => camera.transform.position -= (Vector3)planeDelta;

        public override void MoveTo(Vector2 planePosition)
        {
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(planePosition.x, planePosition.y, cameraTransform.position.z);
        }

        public Orthographic(Camera camera) : base(camera) { }
    }

    // The perspective case, where the camera can sit at any angle to the physics plane.
    // Screen positions become a ray that has to be intersected with the plane, and zooming moves the camera along its own view direction rather than resizing a box.
    private sealed class Perspective : CameraProjection
    {
        public override Vector2 ScreenToPlanePoint(Vector2 screenPosition)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            var plane = physicsPlane;

            // A ray parallel to the plane, or pointing away from it, has no useful answer, so hold the last position rather than returning a wild one.
            if (!plane.Raycast(ray, out var distance))
                return WorldToPlane(camera.transform.position);

            return WorldToPlane(ray.GetPoint(distance));
        }

        public override void SetFraming(float cameraSize, float zoom)
        {
            // Match what an orthographic camera of this size would show, by backing the camera off far enough for its field of view to span the same height at the plane.
            var halfHeight = cameraSize / math.max(zoom, 0.00001f);
            var distance = halfHeight / math.tan(math.radians(camera.fieldOfView * 0.5f));

            var cameraTransform = camera.transform;
            var lookAt = LookAtPoint();

            cameraTransform.position = lookAt - cameraTransform.forward * distance;
        }

        public override void PanBy(Vector2 planeDelta)
        {
            // A distance on the plane is only a world distance once it has been mapped through the plane's own axes.
            var worldDelta = PlaneToWorld(planeDelta) - PlaneToWorld(Vector2.zero);

            camera.transform.position -= worldDelta;
        }

        public override void MoveTo(Vector2 planePosition)
        {
            var cameraTransform = camera.transform;
            var distance = Vector3.Distance(cameraTransform.position, LookAtPoint());

            cameraTransform.position = PlaneToWorld(planePosition) - cameraTransform.forward * distance;
        }

        // The point on the physics plane the camera is currently aimed at.
        // Falls back to the plane's origin when the camera is aimed away from the plane entirely, which only happens while an example is being set up.
        private Vector3 LookAtPoint()
        {
            var cameraTransform = camera.transform;
            var ray = new Ray(cameraTransform.position, cameraTransform.forward);

            return physicsPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : PlaneToWorld(Vector2.zero);
        }

        // The physics plane as a geometric plane in world space, derived from the world's own axes so a custom plane works the same as XY or XZ.
        private static Plane physicsPlane
        {
            get
            {
                var origin = PlaneToWorld(Vector2.zero);
                var right = PlaneToWorld(Vector2.right) - origin;
                var up = PlaneToWorld(Vector2.up) - origin;

                return new Plane(Vector3.Cross(right, up).normalized, origin);
            }
        }

        public Perspective(Camera camera) : base(camera) { }
    }
}
