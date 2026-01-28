using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //! VARIABLES
    [Header("Referencias")]
    public StatsMovement movementStats;
    [SerializeField] Collider2D coll;

    Rigidbody2D rb;

//TODO MOVIMIENTO

    public bool isFacingRight { get; private set; }
    public MovementController controller { get; private set; }
    [HideInInspector] public Vector2 Velocity;

    //TODO INPUT
    Vector2 moveInput;
    bool runHeld;
    bool jumpPressed;
    bool jumpRelased;

    //TODO SALTO
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

//TODO HeadBump
    float jumpStartY;
    bool isHeadBumpSliding;
    int headBumpSlideDirection;
    bool justFinishedSlide;


    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<MovementController>();
    }

    private void Update()
    {
        moveInput = InputManager.movement;
        runHeld = InputManager.runIsHeld;
        if (InputManager.jumpPressed) jumpPressed = true;
        if (InputManager.jumpWasReleased) jumpRelased = true;
    }

    private void FixedUpdate()
    {
        justFinishedSlide = false;

        CountTimers(Time.fixedDeltaTime);

        JumpChecks();
        LandCheck();

        HandleHorizontalMovement(Time.fixedDeltaTime);
        HandleHeadBumpSlide();
        Jump(Time.fixedDeltaTime);
        Fall(Time.fixedDeltaTime);

        ClampVelocity();

        controller.Move(Velocity * Time.fixedDeltaTime);

        //reset inputs
        jumpPressed = false;
        jumpRelased = false;

    }

    private void ClampVelocity()
    {
        //TODO CLAMP VELOCIDAD DE CAIDA
        Velocity.y = Mathf.Clamp(Velocity.y, -movementStats.maxFallSpeed, 50f);
    }

    private void OnDrawGizmos()
    {
        if (movementStats != null && coll != null)
            DrawJumpArc(movementStats.maxWalkSpeed, Color.blue);
    }


    #region Movimiento
    private void HandleHorizontalMovement(float timeStep)
    {
        if (isHeadBumpSliding) return;

        TurnCheck(moveInput);
        float targetVelocityX = 0f;

        if (Mathf.Abs(moveInput.x) >= movementStats.moveThreshold)
        {
            float moveDirection = Mathf.Sign(moveInput.x);
            targetVelocityX = runHeld ? moveDirection * movementStats.maxRunSpeed : moveDirection * movementStats.maxWalkSpeed;
        }

        float acceleration = controller.isGrounded() ? movementStats.groundAcceleration : movementStats.airAcceleration;
        float deceleration = controller.isGrounded() ? movementStats.groundAcceleration : movementStats.airDeceleration;

        if (Mathf.Abs(moveInput.x) >= movementStats.moveThreshold)
        {
            Velocity.x = Mathf.Lerp(Velocity.x, targetVelocityX, acceleration * timeStep);
        }
        else
        {
            Velocity.x = Mathf.Lerp(Velocity.x, 0f, deceleration * timeStep);
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

    private void HandleHeadBumpSlide()
    {
        // INICIO DEL SLIDE
        if (!isHeadBumpSliding && isJumping && controller.bumpedHead()
            && !controller.isHittingBothCorners && !controller.isHittingCeilingCenter)
        {
            isHeadBumpSliding = true;
            headBumpSlideDirection = controller.HeadBumpSlideDirection;
        }

        if (!isHeadBumpSliding)
            return;

        Velocity.y = 0f;

        if (controller.HeadBumpSlideDirection == 0
            || !controller.bumpedHead()
            || controller.isHittingCeilingCenter
            || controller.isHittingBothCorners)
        {
            isHeadBumpSliding = false;
            Velocity.x = 0f;

            float compensationFactor = (1 - movementStats.jumpHeightCompensationFactor) + 1;
            float jumpPeakY = jumpStartY + (movementStats.jumpHeight * compensationFactor);
            float remainingHeight = jumpPeakY - rb.position.y;

            if (remainingHeight > 0f)
            {
                float requiredVelocity =
                    Mathf.Sqrt(2 * Mathf.Abs(movementStats.Gravity) * remainingHeight);
                Velocity.y = requiredVelocity;
            }

            justFinishedSlide = true;
        }
        else
        {
            Velocity.x = headBumpSlideDirection * movementStats.headBumpSlideSpeed;
        }
    }


    #endregion

    #region Caida
    private void LandCheck()
    {
        if (controller.isGrounded())
        {
            if ((isJumping || isFalling || isHeadBumpSliding) && Velocity.y <= 0f)
            {
                isHeadBumpSliding = false;

                ResetJumpValues();

                numberOfJumpsUsed = 0;
            }

            if (Velocity.y <= 0f)
            {
                Velocity.y = -2f;
            }
        }
    }

    private void Fall(float timeStep)
    {
        //TODO GRAVEDAD NORMAL MIENTRAS CAE
        if (!controller.isGrounded() && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            Velocity.y += movementStats.Gravity * timeStep;
        }
    }


    #endregion

    #region Salto
    private void ResetJumpValues()
    {
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        fastFallTime = 0f;
        isPastApexThreshold = false;
    }

    private void JumpChecks()
    {
     //TODO BOTON APRETADO
            if (jumpPressed)
            {
            jumpBufferTimer = movementStats.jumpBufferTime;
            jumpRelasedDuringBuffer = false;
            }

     //TODO BOTON SOLTADO
            if (jumpRelased)
            {
                if(jumpBufferTimer > 0f)
                {
                        jumpRelasedDuringBuffer = true;
                }

                if(isJumping && Velocity.y > 0f)
                {
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                        isFastFalling = true;
                        fastFallTime = movementStats.timeForUpwardsCancel;
                        Velocity.y = Mathf.Min(Velocity.y, 0f);

                    }
                    else
                    {
                        isFastFalling = true;
                        fastFallRelaseSpeed = Velocity.y;
                    }
                }
            }

     //TODO INICIAR SALTO CON JUMP BUFFER Y COYOTE
            if (jumpBufferTimer > 0f && !isJumping && (controller.isGrounded() || coyoteTimer > 0f))
            {
                InitiateJump(1);

                if (jumpRelasedDuringBuffer)
                {
                    isFastFalling = true;
                    fastFallRelaseSpeed = Velocity.y;
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

    }

    private void InitiateJump(int jumpsToConsume)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += jumpsToConsume;
        Velocity.y = movementStats.initialJumpVelocity;

        jumpStartY = rb.position.y;
    }


    private void Jump(float timeStep)
    {
        //TODO APLICAR VELOCIDAD MIENTRAS SALTA
        if (isJumping)
        {
            //TODO CHECK HEAD BUMP
            if (controller.bumpedHead() && !isHeadBumpSliding)
            {
                if (controller.HeadBumpSlideDirection != 0 && !controller.isHittingCeilingCenter && !controller.isHittingBothCorners)
                {

                }
                else
                {
                    Velocity.y = 0f;
                    isFastFalling = true;
                }
            }

            if (isHeadBumpSliding)
            {
                Velocity.y = 0f;
                return;
            }

            if (!justFinishedSlide)
            {
                //TODO GRAVEDAD ASCENDIENDO
                if (Velocity.y >= 0f)
                {
                    //TODO CONTROLES APEX 
                    apexPoint = Mathf.InverseLerp(movementStats.initialJumpVelocity, 0f, Velocity.y);

                    if (apexPoint > movementStats.apexThreshold)
                    {
                        if (!isPastApexThreshold)
                        {
                            isPastApexThreshold = true;
                            timePastApexThreshold = 0f;
                        }

                        if (isPastApexThreshold)
                        {
                            timePastApexThreshold += timeStep;
                            if (timePastApexThreshold < movementStats.apexHangTime)
                            {
                                Velocity.y = Mathf.Max(Velocity.y, -0.01f);
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
                    else if (!isFastFalling)
                    {
                        Velocity.y += movementStats.Gravity * timeStep;
                        if (isPastApexThreshold)
                        {
                            isPastApexThreshold = false;
                        }
                    }
                }
                //TODO GRAVEDAD DESCENDIENDO
                else if (isFastFalling)
                {
                    Velocity.y += movementStats.Gravity * movementStats.gravitOnRelaseMultiplier * timeStep;
                }

                else if (Velocity.y < 0f)
                {
                    if (!isFalling)
                        isFalling = true;
                }

            }
        }

        //TODO JUMP CUT
        if (isFastFalling)
        {
            if (fastFallTime >= movementStats.timeForUpwardsCancel)
            {
                Velocity.y += movementStats.Gravity * movementStats.gravitOnRelaseMultiplier * timeStep;
            }
            else if (fastFallTime < movementStats.timeForUpwardsCancel)
            {
                Velocity.y = Mathf.Lerp(fastFallRelaseSpeed, 0f, fastFallTime / movementStats.timeForUpwardsCancel);
            }

            fastFallTime += timeStep;
        }
    }

    #region Debug Visuals
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(coll.bounds.center.x, coll.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;

        if (movementStats.drawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }

        Vector2 velocity = new Vector2(speed, movementStats.initialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * movementStats.timeTillJumpApex / movementStats.arcResolution;

        for (int i = 0; i < movementStats.visualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < movementStats.timeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0f, movementStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < movementStats.timeTillJumpApex + movementStats.apexHangTime)
            {
                float apexTime = simulationTime - movementStats.timeTillJumpApex;
                displacement = velocity * movementStats.timeTillJumpApex + 0.5f * new Vector2(0f, movementStats.Gravity) * movementStats.timeTillJumpApex * movementStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationTime - (movementStats.timeTillJumpApex + movementStats.apexHangTime);
                displacement = velocity * movementStats.timeTillJumpApex + 0.5f * new Vector2(0f, movementStats.Gravity) * movementStats.timeTillJumpApex * movementStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * movementStats.apexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0f, movementStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (movementStats.stopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), movementStats.groundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }

    }

    #endregion

    #endregion

    #region Temporizadores

    private void CountTimers(float timeStep)
    {
        //buffer
        jumpBufferTimer -= timeStep;

        //coyote
        if (!controller.isGrounded())
            coyoteTimer -= timeStep;
        else
            coyoteTimer = movementStats.coyoteTime;
    }

    #endregion

}
