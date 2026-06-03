using UnityEngine;

public class Rigidbody2D_MovePositionAndRotation : MonoBehaviour
{
    public enum MoveMode
    {
        None,
        MovePositionOnly,
        MoveRotationOnly,
        MovePositionAndRotation
    }

    public MoveMode Mode = MoveMode.None;
    public float PositionSpeed = 25f;
    public float RotationSpeed = 45f;

    private Rigidbody2D m_Rigidbody;
    private float m_PositionPhase;
    private float m_Rotation;
    private float m_VerticalOffset;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_VerticalOffset = transform.position.y;
    }

    void FixedUpdate()
    {
        m_PositionPhase += Time.deltaTime * PositionSpeed;
        m_Rotation += Time.deltaTime * RotationSpeed;

        var testPosition = new Vector2((m_PositionPhase % 100f) - 50.0f, m_VerticalOffset + 5f * Mathf.Sin(((m_PositionPhase % 100f) * 10f) * Mathf.Deg2Rad));
        var testRotation = m_Rotation % 360f;

        switch (Mode)
        {

            case MoveMode.MovePositionOnly:
                m_Rigidbody.MovePosition(testPosition);
                break;

            case MoveMode.MoveRotationOnly:
                m_Rigidbody.MoveRotation(testRotation);
                break;

            case MoveMode.MovePositionAndRotation:
                m_Rigidbody.MovePositionAndRotation(testPosition, testRotation);
                break;

            case MoveMode.None:
            default:
                break;
        }

    }
}
