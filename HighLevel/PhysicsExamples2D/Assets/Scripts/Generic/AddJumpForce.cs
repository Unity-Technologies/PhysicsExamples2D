using UnityEngine;
using UnityEngine.InputSystem;

public class AddJumpForce : MonoBehaviour
{
    public float JumpImpulse = 9f;
    public LayerMask GroundingLayer;

    private Rigidbody2D Body;
    private bool DoJump;

    void Start()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Can only jump if touching grounding layer(s) and the jump key has been pressed.
        if (Body.IsTouchingLayers(GroundingLayer.value) &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DoJump = true;
        }
    }

    void FixedUpdate()
    {
        // Add jump impulse if required.
        if (DoJump)
        {
            Body.AddForce(new Vector2(0f, JumpImpulse), ForceMode2D.Impulse);

            DoJump = false;
        }
    }
}
