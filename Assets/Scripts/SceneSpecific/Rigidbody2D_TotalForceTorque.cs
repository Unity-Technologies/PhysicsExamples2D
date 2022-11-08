using UnityEngine;

public class Rigidbody2D_TotalForceTorque : MonoBehaviour
{
    public bool ResetForce;
    public bool ResetTorque;
    public bool SetTotalForce;
    public bool SetTotalTorque;

    public Vector2 Force;
    public float Torque;

    void Start()
    {
        // Fetch the rigidbody.
        var body = GetComponent<Rigidbody2D>();

        // Apply Force.
        if (SetTotalForce)
            body.totalForce = Force;
        else
            body.AddForce(Force);

        // Apply Torque.
        if (SetTotalTorque)
            body.totalTorque = Torque;
        else
            body.AddTorque(Torque);

        if (ResetForce)
            body.totalForce = Vector2.zero;

        if (ResetTorque)
            body.totalTorque = 0f;
    }
}
