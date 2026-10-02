using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Knocks over the end domino on each of five shelves so a wave runs along every row, alternating direction from one shelf to the next.
/// The shelves and dominoes are all authored Physics Pose and Physics Area components, and this component only gives the first domino of each row its push.
/// </summary>
/// <remarks>
/// A push is a small impulse applied near the top of the domino, which tips it into its neighbor rather than sliding it along the shelf.
/// </remarks>
public sealed class DoubleDominoContents : MonoBehaviour
{
    private void Start()
    {
        foreach (var domino in m_TipRight)
            Push(domino, Vector2.right * PushImpulse);

        foreach (var domino in m_TipLeft)
            Push(domino, Vector2.left * PushImpulse);
    }

    // Applies the impulse to the domino at a point near its top, so it rotates as well as moves.
    private static void Push(PhysicsPose domino, Vector2 impulse)
    {
        if (domino == null)
            return;

        var body = domino.body;

        if (!body.isValid)
            return;

        body.ApplyLinearImpulse(impulse, body.position + PushOffset);
    }

    #region Internal

    // How hard each first domino is pushed, and how far above its middle the push lands.
    const float PushImpulse = 0.2f;
    static readonly Vector2 PushOffset = new(0f, 0.25f);

    [SerializeField] PhysicsPose[] m_TipRight = new PhysicsPose[0];
    [SerializeField] PhysicsPose[] m_TipLeft = new PhysicsPose[0];

    #endregion
}
