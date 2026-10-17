using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace Unity.U2D.RadianceCascades
{
    /// <summary>
    /// Draws every registered sprite through <see cref="Graphics.RenderSpriteInstanced"/>: one instanced draw per distinct sprite.
    /// Bullets and enemies register here instead of carrying SpriteRenderer components, so their prefabs hold no renderers at all.
    /// </summary>
    /// <remarks>
    /// Instances are grouped by sprite and their matrices are rebuilt every frame from each owner's Transform plus its live color and scale.
    /// The draw material is a private copy of the assigned sprite material with GPU instancing enabled.
    /// </remarks>
    [AutoStaticsCleanup]
    public sealed partial class SpriteRenderSystem : MonoBehaviour
    {
        /// <summary>
        /// The scene's sprite render system, or null when none is active.
        /// </summary>
        public static SpriteRenderSystem instance { get; private set; }

        /// <summary>
        /// One registered sprite to draw: the owner keeps this handle and writes its color and scale freely.
        /// Position and rotation are read from the transform every frame, so movement needs no notification.
        /// </summary>
        public sealed class SpriteInstance
        {
            /// <summary>
            /// The tint applied to this instance; HDR values drive the bloom exactly as a SpriteRenderer color would.
            /// </summary>
            public Color color = Color.white;

            /// <summary>
            /// The world scale of the sprite, replacing what the old renderer child's local scale provided.
            /// </summary>
            public Vector2 scale = Vector2.one;

            internal Transform transform;
            internal Group group;
            internal int index;
        }

        /// <summary>
        /// Registers a sprite to draw at the transform's position and rotation, and returns the handle used to recolor, rescale, or remove it.
        /// </summary>
        /// <param name="spriteTransform">The transform the sprite's position and rotation are read from every frame.</param>
        /// <param name="sprite">The sprite to draw. Instances sharing a sprite are drawn together.</param>
        /// <param name="scale">The world scale to draw at.</param>
        /// <param name="color">The starting tint, which the returned handle can change freely.</param>
        public SpriteInstance Add(Transform spriteTransform, Sprite sprite, Vector2 scale, Color color)
        {
            if (!m_Groups.TryGetValue(sprite, out var group))
            {
                // The white base color leaves the tint entirely to the per-instance colors.
                group = new Group
                {
                    spriteParams = new SpriteParams(sprite, Color.white, SpriteMaskInteraction.None)
                };
                m_Groups.Add(sprite, group);
            }

            var spriteInstance = new SpriteInstance
            {
                color = color,
                scale = scale,
                transform = spriteTransform,
                group = group,
                index = group.instances.Count
            };
            group.instances.Add(spriteInstance);

            return spriteInstance;
        }

        /// <summary>
        /// Removes a registered sprite; the group's last entry is swapped into its slot.
        /// </summary>
        public void Remove(SpriteInstance spriteInstance)
        {
            var group = spriteInstance.group;

            if (group == null)
                return;

            var lastIndex = group.instances.Count - 1;
            var last = group.instances[lastIndex];
            group.instances[spriteInstance.index] = last;
            group.instances.RemoveAt(lastIndex);
            last.index = spriteInstance.index;

            spriteInstance.group = null;
        }

        void OnEnable()
        {
            // The assigned material is shared with the scene's remaining SpriteRenderers, so instancing is enabled on a private copy.
            m_Material = new Material(m_SourceMaterial) { enableInstancing = true };

            m_RenderParams = new RenderParams(m_Material)
            {
                worldBounds = new Bounds(Vector3.zero, new Vector3(200f, 200f, 10f))
            };

            instance = this;
        }

        void OnDisable()
        {
            if (instance == this)
                instance = null;

            m_Groups.Clear();
            Destroy(m_Material);
        }

        // Submits every group after all movement for the frame has landed in the Transforms.
        void LateUpdate()
        {
            foreach (var group in m_Groups.Values)
            {
                var count = group.instances.Count;

                if (count == 0)
                    continue;

                if (group.buffer == null || group.buffer.Length < count)
                {
                    var capacity = Mathf.NextPowerOfTwo(count);

                    group.buffer = new InstanceData[capacity];
                    group.transforms = new Matrix4x4[capacity];
                    group.colors = new Vector4[capacity];
                }

                for (var i = 0; i < count; ++i)
                {
                    var spriteInstance = group.instances[i];

                    var objectToWorld = Matrix4x4.TRS(
                        spriteInstance.transform.position,
                        spriteInstance.transform.rotation,
                        new Vector3(spriteInstance.scale.x, spriteInstance.scale.y, 1f));

                    group.buffer[i] = new InstanceData
                    {
                        objectToWorld = objectToWorld,
                        spriteColor = spriteInstance.color
                    };

                    group.transforms[i] = objectToWorld;
                    group.colors[i] = spriteInstance.color;
                }

                for (var offset = 0; offset < count; offset += MaxInstancesPerDraw)
                {
                    var length = Mathf.Min(MaxInstancesPerDraw, count - offset);

                    Graphics.RenderSpriteInstanced(m_RenderParams, group.spriteParams, 0, new ReadOnlySpan<InstanceData>(group.buffer, offset, length));
                }
            }
        }

        /// <summary>
        /// One instanced draw's worth of sprites: the sprite they share, their packed transforms and tints, and how many are in use.
        /// </summary>
        /// <remarks>
        /// The arrays belong to the system and are rebuilt every frame, so a caller reads them during the frame and never keeps them.
        /// </remarks>
        public readonly struct Batch
        {
            /// <summary>
            /// The sprite every instance in this batch draws.
            /// </summary>
            public readonly Sprite sprite;

            /// <summary>
            /// The world transform of each instance, valid up to <see cref="count"/>.
            /// </summary>
            public readonly Matrix4x4[] transforms;

            /// <summary>
            /// The tint of each instance, valid up to <see cref="count"/>.
            /// </summary>
            public readonly Vector4[] colors;

            /// <summary>
            /// How many entries of the two arrays are in use.
            /// </summary>
            public readonly int count;

            internal Batch(Sprite sprite, Matrix4x4[] transforms, Vector4[] colors, int count)
            {
                this.sprite = sprite;
                this.transforms = transforms;
                this.colors = colors;
                this.count = count;
            }
        }

        /// <summary>
        /// Fills the given list with one entry per sprite being drawn this frame, so another pass can draw the same instances into
        /// a target of its own.
        /// </summary>
        /// <param name="batches">The list to fill. It is cleared first.</param>
        public void GetBatches(List<Batch> batches)
        {
            batches.Clear();

            foreach (var pair in m_Groups)
            {
                var group = pair.Value;
                var count = group.instances.Count;

                if (count == 0 || group.transforms == null)
                    continue;

                batches.Add(new Batch(pair.Key, group.transforms, group.colors, count));
            }
        }

        // The per-instance data the draw consumes; the member names are the contract with RenderSpriteInstanced.
        internal struct InstanceData
        {
            public Matrix4x4 objectToWorld;
            public Color spriteColor;
        }

        // All instances sharing one sprite, plus the reusable arrays their draw data is packed into.
        // The transforms and tints are kept separately as well, because an instanced mesh draw takes them as their own arrays.
        internal sealed class Group
        {
            public readonly List<SpriteInstance> instances = new();
            public SpriteParams spriteParams;
            public InstanceData[] buffer;
            public Matrix4x4[] transforms;
            public Vector4[] colors;
        }

        #region Internal

        // Instancing submits at most 511 instances per draw with default-sized matrices, so chunk safely below that.
        const int MaxInstancesPerDraw = 500;

        // The sprite material to draw with; the system uses a private instancing-enabled copy.
        [SerializeField] Material m_SourceMaterial;

        readonly Dictionary<Sprite, Group> m_Groups = new();
        Material m_Material;
        RenderParams m_RenderParams;

        #endregion
    }
}
