using Unity.Collections;
using UnityEngine;

namespace Unity.U2D.Physics.Examples
{
    /// <summary>
    /// Adds the shapes of a cogged gear to the body of its owning pose: a round hub with cogs set evenly around it.
    /// Gears drive each other by their cogs touching rather than by any constraint between them, so two gears turn each other only where their cogs actually reach.
    /// </summary>
    /// <remarks>
    /// This is the gear's shapes and nothing else. Put it on a <see cref="PhysicsPose"/> with a dynamic body so the gear can turn, and hold that body on its axle with a <see cref="PhysicsConstraintHinge"/> anchored at the gear's center.
    /// Everything <see cref="PhysicsArea"/> offers applies to the gear as a whole, including the shape definition and its shared assets, so one surface material and one contact filter cover the hub and every cog alike.
    /// </remarks>
    [AddComponentMenu("Physics 2D (Core)/Example Area Gear", 42)]
    [Icon("Packages/com.unity.2d.physics.examples/Editor/Icons/ExampleAreaGear.png")]
    [PhysicsCustomProperties]
    public sealed class ExampleAreaGear : PhysicsArea
    {
        /// <summary>
        /// The radius of the hub the cogs are set around, in world units.
        /// </summary>
        /// <remarks>
        /// Measured to the base of the cogs, so the gear reaches this plus <see cref="cogHeight"/> in total.
        /// Values below 0.01 are raised to it, because a hub with no radius is not a shape.
        /// Changing this does not update the running shapes until you call <see cref="PhysicsArea.ApplyGeometry"/>.
        /// </remarks>
        public float radius
        {
            get => m_Radius;
            set => m_Radius = Mathf.Max(value, MinimumSize);
        }

        /// <summary>
        /// How many cogs are set evenly around the hub.
        /// </summary>
        /// <remarks>
        /// Values below one are raised to it. A single cog is valid and produces a lopsided gear, which is why there is no higher floor.
        /// Changing this does not update the running shapes until you call <see cref="PhysicsArea.ApplyGeometry"/>.
        /// </remarks>
        public int cogCount
        {
            get => m_CogCount;
            set => m_CogCount = Mathf.Max(value, MinimumCogCount);
        }

        /// <summary>
        /// How wide each cog is across the face of the gear, in world units.
        /// </summary>
        /// <remarks>
        /// Values below 0.01 are raised to it, because a cog with no width is not a shape. Widen the cogs past the spacing the hub allows and neighboring cogs overlap into one solid rim.
        /// Changing this does not update the running shapes until you call <see cref="PhysicsArea.ApplyGeometry"/>.
        /// </remarks>
        public float cogWidth
        {
            get => m_CogWidth;
            set => m_CogWidth = Mathf.Max(value, MinimumSize);
        }

        /// <summary>
        /// How far each cog stands out from the hub, in world units.
        /// </summary>
        /// <remarks>
        /// Values below 0.01 are raised to it, because a cog with no height is not a shape. This is what decides how far apart two gears can sit and still reach each other.
        /// Changing this does not update the running shapes until you call <see cref="PhysicsArea.ApplyGeometry"/>.
        /// </remarks>
        public float cogHeight
        {
            get => m_CogHeight;
            set => m_CogHeight = Mathf.Max(value, MinimumSize);
        }

        /// <summary>
        /// How far the corners of each cog are rounded off, in world units.
        /// </summary>
        /// <remarks>
        /// Values below zero are raised to it. Rounded corners let cogs slide past each other instead of catching, so a gear with square cogs meshes far more roughly.
        /// Changing this does not update the running shapes until you call <see cref="PhysicsArea.ApplyGeometry"/>.
        /// </remarks>
        public float cogRadius
        {
            get => m_CogRadius;
            set => m_CogRadius = Mathf.Max(value, 0f);
        }

        #region Overrides

        /// <summary>
        /// The title the Inspector gives the section holding the gear's own properties.
        /// </summary>
        /// <remarks>
        /// Names the shape being authored, the same way the other areas title that section after the primitive they build.
        /// </remarks>
        protected override string areaSectionTitle => "Gear";

        /// <summary>
        /// Builds the gear as one circle for the hub and one rounded box per cog, set evenly around it.
        /// </summary>
        /// <remarks>
        /// The gear is authored around this area's origin, so both the hub and the cogs are placed by <paramref name="relative"/> before they are created.
        /// </remarks>
        /// <param name="body">Body to create the shapes on.</param>
        /// <param name="relative">Matrix mapping geometry authored in this area's space into the body's local space.</param>
        /// <param name="applyScaleRadius">Whether the scale carried by <paramref name="relative"/> also scales the hub radius and the rounding on the cog corners.</param>
        /// <param name="shapeDefinition">Shape definition to create every shape with.</param>
        /// <param name="shapes">List to append the created shapes to.</param>
        protected override void GenerateShapes(PhysicsBody body, Matrix4x4 relative, bool applyScaleRadius, PhysicsShapeDefinition shapeDefinition, NativeList<PhysicsShape> shapes)
        {
            var hub = new CircleGeometry { radius = m_Radius }.Transform(relative, applyScaleRadius);
            if (hub.isValid)
            {
                var hubShape = body.CreateShape(hub, shapeDefinition);
                if (hubShape.isValid)
                    shapes.Add(hubShape);
            }

            // A count this low can only arrive from a serialized value written before the property clamped it, so the gear is left as a bare hub.
            if (m_CogCount < MinimumCogCount)
                return;

            // Each cog stands out from the hub by its own height, so its center sits half a height beyond the hub radius.
            // They are placed by turning that one offset around the hub rather than by trigonometry per cog, which keeps every cog the same distance out however many there are.
            var cogs = new NativeArray<PolygonGeometry>(m_CogCount, Allocator.Temp);
            var step = PhysicsRotate.FromRadians(PhysicsMath.TAU / m_CogCount);
            var offset = new Vector2(m_Radius + m_CogHeight * 0.5f, 0f);
            var rotation = PhysicsRotate.identity;

            for (var i = 0; i < m_CogCount; ++i)
            {
                cogs[i] = PolygonGeometry.CreateBox(
                    size: new Vector2(m_CogHeight, m_CogWidth),
                    radius: m_CogRadius,
                    transform: new PhysicsTransform(rotation.RotateVector(offset), rotation));

                rotation = step.MultiplyRotation(rotation);
            }

            PolygonGeometry.Transform(cogs.AsSpan(), relative, applyScaleRadius);

            // One batch for the whole ring: the world is locked once rather than once per cog.
            var cogShapes = body.CreateShapeBatch(cogs.AsReadOnlySpan(), shapeDefinition);
            if (cogShapes.IsCreated)
            {
                shapes.AddRange(cogShapes);
                cogShapes.Dispose();
            }

            cogs.Dispose();
        }

        #endregion

        #region Internal

        // The fewest cogs a gear can have. One is lopsided but valid, so nothing higher is enforced.
        private const int MinimumCogCount = 1;

        // The smallest any dimension can be. Zero is degenerate here: a circle with no radius and a box with no extent are not shapes at all.
        // A constant rather than a derived value, so the Min attribute and the property clamps agree and nothing is silently corrected after the user types it.
        private const float MinimumSize = 0.01f;

        [Tooltip("The radius of the hub the cogs are set around, in world units.")]
        [Min(MinimumSize)]
        [SerializeField] private float m_Radius = 1f;

        [Tooltip("How many cogs are set evenly around the hub.")]
        [Min(MinimumCogCount)]
        [SerializeField] private int m_CogCount = 12;

        [Tooltip("How wide each cog is across the face of the gear, in world units.")]
        [Min(MinimumSize)]
        [SerializeField] private float m_CogWidth = 0.25f;

        [Tooltip("How far each cog stands out from the hub, in world units.")]
        [Min(MinimumSize)]
        [SerializeField] private float m_CogHeight = 0.5f;

        [Tooltip("How far the corners of each cog are rounded off, in world units.")]
        [Min(0f)]
        [SerializeField] private float m_CogRadius = 0.03f;

        #endregion
    }
}
