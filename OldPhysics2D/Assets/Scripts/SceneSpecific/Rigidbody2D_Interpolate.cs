using System.Collections.Generic;
using UnityEngine;

public class Rigidbody2D_Interpolate : MonoBehaviour
{
    [Range(0.1f, 30f)]
    public float PositionDuration = 2f;

    [Range(0.1f, 30f)]
    public float RotationDuration = 3f;

    [Range(0.1f, 3f)]
    public float PositionDisplacement = 2.5f;

    [Range(3f, 240f)]
    public float FixedTimeFPS = 5;

    private float From;
    private float To;
    private float m_ElapsedTime;
    private float m_Position;
    private float m_Rotation;

    private List<Rigidbody2D> m_Rigidbodies = new List<Rigidbody2D>(2);


    void Start()
    {
        OnValidate();

        GetComponentsInChildren(m_Rigidbodies);
    }

    void FixedUpdate()
    {
        // Adjust the position.
        m_Position += ((To - From) / PositionDuration) * Time.fixedDeltaTime;

        // Adjust the rotation.
        m_Rotation += (360f / RotationDuration) * Time.fixedDeltaTime;

        // Request the move.
        foreach (var body in m_Rigidbodies)
        {
            body.MovePosition(new Vector2(m_Position, body.position.y));
            body.MoveRotation(m_Rotation);
        }

        // Flip flop.
        m_ElapsedTime += Time.fixedDeltaTime;
        if (m_ElapsedTime > PositionDuration)
        {
            m_ElapsedTime = 0f;
            From = -From;
            To = -To;
        }
    }

    private void OnValidate()
    {
        From = -PositionDisplacement;
        To = PositionDisplacement;
        m_Position = From;
        m_Rotation = 0f;
        m_ElapsedTime = 0f;
        
        if (Application.isPlaying)
        {
            Time.fixedDeltaTime = 1f / FixedTimeFPS;
        }
    }
}
