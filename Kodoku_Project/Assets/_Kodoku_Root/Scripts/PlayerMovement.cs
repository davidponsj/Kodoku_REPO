using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //! VARIABLES
    [Header("Referencias")]
    public StatsMovement movementStats;
    [SerializeField] Collider2D bodyCollider;
    [SerializeField] Collider2D feetCollider;

    Rigidbody2D rb;

//TODO MOVIMIENTO
    Vector2 moveVelocity;
    bool isFacingRight;

//TODO COLISIONES
    RaycastHit2D groundHit;
    RaycastHit2D headHit;
    bool isGrounded;
    bool bumpedHead;

//TODO SALTO
    public float verticalVelocity { get; private set; }
    bool isJumping;
    bool isFastFalling;
    bool isFalling;
    float fastFallTime;
    float fastFallRelaseSpeed;
    int numberOfJumpsUsed;

//TODO APEX
    float apexPoint;
    float timePastApexThreshold;
    bool isPastApexThreshold;

//TODO JumpBuffer
    float jumpBufferTimer;
    bool jumpRelasedDuringBuffer;

//TODO Coyote Time
    float coyoteTimer;


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

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if(isGrounded)
            Mover(movementStats.groundAcceleration, movementStats.groundDeceleration, InputManager.movement);
        else
            Mover(movementStats.airAcceleration, movementStats.airDeceleration, InputManager.movement);

    }

    #region Movimiento


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

    #region Salto

    private void JumpChecks()
    {
 //TODO BOTON APRETADO
        if (InputManager.jumpPressed)
        {
        jumpBufferTimer = movementStats.jumpBufferTime;
        jumpRelasedDuringBuffer = false;
        }

 //TODO BOTON SOLTADO
        if (InputManager.jumpWasReleased)
        {
            if(jumpBufferTimer > 0f)
            {
                    jumpRelasedDuringBuffer = true;
            }

            if(isJumping && verticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = movementStats.timeForUpwardsCancel;
                    verticalVelocity = Mathf.Min(verticalVelocity, 0f);

                }
                else
                {
                    isFastFalling = true;
                    fastFallRelaseSpeed = verticalVelocity;
                }
            }
        }

 //TODO INICIAR SALTO CON JUMP BUFFER Y COYOTE
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpRelasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallRelaseSpeed = verticalVelocity;
            }
        }

        //TODO DOBLE SALTO
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < movementStats.jumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //TODO SALTO EN EL AIRE DESPUES DE COYOTE TIME
        else if (jumpBufferTimer > 0f && isFalling && numberOfJumpsUsed < movementStats.jumpsAllowed - 1)
        {
            isFastFalling = false;
            InitiateJump(2);
        }

        //TODO CAIDA
        if ((isJumping || isFalling) && isGrounded && verticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;

            verticalVelocity = Physics2D.gravity.y;
        }

    }

    private void InitiateJump(int jumpsToConsume)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += jumpsToConsume;
        verticalVelocity = movementStats.initialJumpVelocity;
    }


    private void Jump()
    {
 //TODO APLICAR VELOCIDAD MIENTRAS SALTA
        if (isJumping)
        {
     //TODO CHECK HEAD BUMP
            if (bumpedHead)
            {
                isFastFalling = true;
            }

     //TODO GRAVEDAD ASCENDIENDO
            if (verticalVelocity >= 0f)
            {
         //TODO CONTROLES APEX 
                apexPoint = Mathf.InverseLerp(movementStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint > movementStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < movementStats.apexHangTime)
                        {
                            verticalVelocity = Mathf.Max(verticalVelocity, -0.01f);
                        }
                        else
                        {
                            isPastApexThreshold = false;
                            isJumping = false;
                            isFalling = true;
                        }
                    }
                }

         //TODO GRAVEDAD DESCENDIENDO PERO SIN QUE HAYA PASADO APEX TRESHOLD
                else
                {
                    verticalVelocity += movementStats.Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                    }
                }

            }

     //TODO GRAVEDAD DESCENDIENDO
            else if (isFastFalling)
            {
                verticalVelocity += movementStats.Gravity  * movementStats.gravitOnRelaseMultiplier * Time.fixedDeltaTime;
            }

            else if (verticalVelocity < 0f)
            {
                if(!isFalling)
                    isFalling = true;
            }

        }

 //TODO JUMP CUT
        if (isFastFalling)
        {
            if (fastFallTime >= movementStats.timeForUpwardsCancel)
            {
                verticalVelocity += movementStats.Gravity * movementStats.gravitOnRelaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < movementStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(fastFallRelaseSpeed, 0f, fastFallTime / movementStats.timeForUpwardsCancel);
            }

            fastFallTime += Time.fixedDeltaTime;
        }

 //TODO GRAVEDAD NORMAL MIENTRAS CAE
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += movementStats.Gravity * Time.fixedDeltaTime;
        }


 //TODO CLAMP VELOCIDAD DE CAIDA
        verticalVelocity = Mathf.Clamp(verticalVelocity, -movementStats.maxFallSpeed, 50f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
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

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * movementStats.headwidth, movementStats.headDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, movementStats.headDetectionRayLength, movementStats.groundLayer);
        if (headHit.collider != null)
            bumpedHead = true;
        else
            bumpedHead = false;

        #region Debug Visuals

        if (movementStats.debugShowHeadBumpBox)
        {
            float headWidth = movementStats.headwidth;

            Color rayColor;
            if(bumpedHead)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * movementStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * movementStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + movementStats.headDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Temporizadores

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if(!isGrounded)
            coyoteTimer -= Time.deltaTime;
        else
            coyoteTimer = movementStats.coyoteTime;
    }

    #endregion
}
