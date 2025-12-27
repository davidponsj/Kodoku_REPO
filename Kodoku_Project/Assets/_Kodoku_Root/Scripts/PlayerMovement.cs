using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //! VARIABLES
    [Header("Referencias")]
    public PlayerMovementStats movementStats;
    [SerializeField] Collider2D bodyCollider;
    [SerializeField] Collider2D feetCollider;

    Rigidbody2D rb;

    Vector2 moveVelocity;
    bool isFacingRight;

    RaycastHit2D groundHit;
    RaycastHit2D headHit;

    bool isGrounded;
    bool boompedHead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (movementStats == null)
            Debug.LogError("PlayerMovementStats no asignado", this);
        if (bodyCollider == null)
            Debug.LogError("Body Collider no asignado", this);
        if (feetCollider == null)
            Debug.LogError("Feet Collider no asignado", this);
    }

    private void FixedUpdate()
    {
        CollisionChecks();

        if(isGrounded)
            Mover(movementStats.groundAcceleration, movementStats.groundDeceleration, InputManager.movement);
        else
            Mover(movementStats.airAcceleration, movementStats.airDeceleration, InputManager.movement);

        Movimiento();
    }

    #region Movimiento

    private void Movimiento()
    {
        rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
    }

    private void Mover(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.runIsHeld)
                targetVelocity = new Vector2(moveInput.x, 0f) * movementStats.maxRunSpeed;
            else
                targetVelocity = new Vector2(moveInput.x, 0f) * movementStats.maxWalkSpeed;

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }

    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !isFacingRight)
            Turn();
        else if (moveInput.x < 0 && isFacingRight)
            Turn();
    }

    private void Turn()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
    }

    #endregion

    #region Colisiones

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, movementStats.groundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, movementStats.groundDetectionRayLength, movementStats.groundLayer);
        if (groundHit.collider != null)
            isGrounded = true;
        else
            isGrounded = false;

        #region Debug Visuals
        if (movementStats.debugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * movementStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * movementStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - movementStats.groundDetectionRayLength), Vector2.right * boxCastSize, rayColor);
        }

        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion
}
