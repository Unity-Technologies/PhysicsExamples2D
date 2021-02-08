using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An example top-down movement controller that uses a Kinematic Rigidbody2D.
/// Multiple iterations of a query are used with movement clamping that allows "sliding" along surfaces.
/// </summary>
public class KinematicTopDownController : MonoBehaviour
{
    public enum MovementInput
    {
        Keyboard,
        Mouse
    };

    // Allows movement via keyboard or mouse.
    public MovementInput MovementType = MovementInput.Mouse;

    // The speed of movement.
    public float m_MoveSpeed = 2.0f;

    // A small offset (standoff) from colliders to ensure we don't try to get too close.
    // Moving too close can mean we get hits when moving tangential to a surface which results
    // in the controller not being able to move.
    public float m_ContactOffset = 0.05f;

    // A method to ensure we only iterate a certain number of times. Depending on the geometry,
    // this can be increased but for most purposes 2 or 3 will suffice.
    public int m_MaxIterations = 2;

    // Controls what the controller considers hits.
    // Note: This allows us to control not only layers but potentially collision normal angles etc.
    public ContactFilter2D m_MovementFilter;

    private Rigidbody2D m_Rigidbody;
    private Vector2 m_Movement;
    private List<RaycastHit2D> m_MovementHits = new List<RaycastHit2D>(1);

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch(MovementType)
        {
            case MovementInput.Keyboard:
                {
                    // Super simple movement allowing us to have compound horizontal/vertical movement to test.
                    m_Movement = new Vector2(
                        (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0),
                        (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0));

                    return;
                }

            case MovementInput.Mouse:
                {
		            // Calculate the world position for the mouse.
		            m_Movement = (Vector2)Camera.main.ScreenToWorldPoint (Input.mousePosition) - m_Rigidbody.position;

                    return;
                }
        }

        // Reset the input movement.
        m_Movement = Vector2.zero;
    }

    void FixedUpdate()
    {
        // Don't perform any work if no movement is required.
        if (m_Movement.sqrMagnitude <= Mathf.Epsilon)
            return;

        // Grab the input movement unit direction.
        var movementDirection = m_Movement.normalized;

        // Calculate how much distance we'd like to cover this update.
        var distanceRemaining = Mathf.Min(m_MoveSpeed * Time.fixedDeltaTime, m_Movement.magnitude);
        
        var maxIterations = m_MaxIterations;

        // We're going to be repositioning the Rigidbody2D during the query iterations so we'll need to keep a note of its starting position.
        // NOTE: As mentioned below, we can avoid this altogether.
        var startPosition = m_Rigidbody.position;

        // Iterate up to a capped iteration limit or until we have no distance to move or we've clamped the direction of motion to zero.
        const float Epsilon = 0.005f;
        while(
            maxIterations-- > 0 &&
            distanceRemaining > Epsilon &&
            movementDirection.sqrMagnitude > Epsilon
            )
        {
            var distance = distanceRemaining;

            // Perform a cast in the current movement direction using the colliders on the Rigidbody.
            // Note: A potentially better way of doing this is to do an arbitrary shape cast such as Physics2D.CapsuleCast/BoxCast etc.
            // At least when performing a specific shape query, we have no need to reposition the Rigidbody2D before each query.
            var hitCount = m_Rigidbody.Cast(movementDirection, m_MovementFilter, m_MovementHits, distance);

            // Did we have any hits?
            if (hitCount > 0)
            {
                // Yes, so for this controller we're only interested in the first results which is the first hit.
                var hit = m_MovementHits[0];

                // We're only interested in movement if it's beyond the contact offset.
                if (hit.distance > m_ContactOffset)
                {
                    // Calculate the distance we'd like to move.
                    distance = hit.distance - m_ContactOffset;

                    // Reposition the Rigidbody2D to the hit point.
                    // NOTE: Again, this can be avoided by a different choice of query.
                    m_Rigidbody.position += movementDirection * distance;
                }
                else
                {
                    // We had a hit but it resulted in us touching or being inside the contact offset.
                    distance = 0f;
                }

                // Clamp the movement direction.
                // NOTE: This is effectively how we iterate and change direction for the queries.
                movementDirection -=  hit.normal * Vector2.Dot(movementDirection, hit.normal);
            }
            else
            {
                // No hit so move by the whole distance.
                m_Rigidbody.position += movementDirection * distance;
            }

            // Remove the distance we ended up moving from the remaining.
            distanceRemaining -= distance;
        };

        // Reset the input movement.
        m_Movement = Vector2.zero;

        // Reset the Rigidbody2D position due to changes during querying.
        // NOTE: We can avoid this setting of the Rigidbody2D position by a different choice of query.
        var targetPosition = m_Rigidbody.position;
        m_Rigidbody.position = startPosition;

        // Set-up a move for the Rigidbody2D.
        // NOTE: Technically here we're moving in a direct line to the final position but we potentially found
        // this position in separate directions but if the time-step and speed is small, it's not going to be a problem.
        m_Rigidbody.MovePosition(targetPosition);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        RemoveOverlap(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RemoveOverlap(collision);
    }

    void RemoveOverlap(Collision2D collision)
    {
        // If we're filtering out the collider we hit then ignore it.
        if (m_MovementFilter.IsFilteringLayerMask(collision.collider.gameObject))
            return;

        // Calculate the collider distance.
        var colliderDistance = Physics2D.Distance(collision.otherCollider, collision.collider);

        // If we're overlapped then remove the overlap.
        // NOTE: We could also ensure we move out of overlap by the contact offset here.
        if (colliderDistance.isOverlapped)
            collision.otherRigidbody.position += colliderDistance.normal * colliderDistance.distance;
    }
}
