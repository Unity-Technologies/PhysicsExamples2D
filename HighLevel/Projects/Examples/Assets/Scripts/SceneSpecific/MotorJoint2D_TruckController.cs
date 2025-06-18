using UnityEngine;

/// <summary>
/// A basic controlled for a truck using WheelJoint2D for the wheels.
/// </summary>
public class MotorJoint2D_TruckController : MonoBehaviour
{
    [Range(1, 500)]
    public float m_MotorSpeed = 500f;

    [Range(1, 10000)]
    public float m_MotorMaximumForce = 10000f;

    public bool m_InvertMotor = false;

    private WheelJoint2D[] m_WheelJoints;

	void Start ()
	{
        m_WheelJoints = GetComponentsInChildren<WheelJoint2D> ();
	}
	
	void Update ()
	{
        // Finish if we have no wheel joints to control.
        if (m_WheelJoints == null || m_WheelJoints.Length == 0)
            return;

        // Calculate motor speed based upon selected motor inversion.
        var motorSpeed = m_InvertMotor ? -m_MotorSpeed : m_MotorSpeed;

        // Setup initial motor state.
        bool useMotor = false;
        JointMotor2D jointMotor = new JointMotor2D ();
        jointMotor.maxMotorTorque = m_MotorMaximumForce;

        // If we're pressing the forward then turn on the motor forwards.
        if (Input.GetKey (KeyCode.RightArrow))
        {
            useMotor = true;
            jointMotor.motorSpeed = motorSpeed;
        }
        // If we're pressing the forward then turn on the motor backwards.
        else if (Input.GetKey (KeyCode.LeftArrow))
        {
            useMotor = true;
            jointMotor.motorSpeed = -motorSpeed;
        }

        // Set all the wheel joint motor states.
        foreach (var wheelJoint in m_WheelJoints)
        {
            wheelJoint.useMotor = useMotor;

            if (useMotor)
                wheelJoint.motor = jointMotor;
        }
	}
}
