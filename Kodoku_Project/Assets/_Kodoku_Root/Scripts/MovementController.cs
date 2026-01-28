using UnityEngine;

public class MovementController : MonoBehaviour
{
    public const float CollisionPadding = 0.015f;

    [Range(2, 100)] public int numOfHorizontalRays = 4;
    [Range(2, 100)] public int numOfVerticalRays = 4;

    float horizontalRaySpace;
    float verticalRaySpace;

    BoxCollider2D coll;
    public RaycastCorners RayCastCorners;
    StatsMovement moveStats;

    public bool isCollidingAbove { get; private set; }
    public bool isCollidingBelow { get; private set; }
    public bool isCollidingLeft { get; private set; }
    public bool isCollidingRight { get; private set; }

    PlayerMovement playerMovement;
    Rigidbody2D rb;

    public struct RaycastCorners
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        moveStats = playerMovement.movementStats;
    }

    private void Start()
    {
        CalculateRaySpacing();
    }

    public void Move(Vector2 velocity)
    {
        UpdateRayCastCorners();
        ResetCollisionStates();

        ResolveHorizontalMovement(ref velocity);
        ResolveVerticalMovement(ref velocity);

        rb.MovePosition(rb.position + velocity);
    }

    private void ResetCollisionStates()
    {
        isCollidingAbove = false;
        isCollidingBelow = false;
        isCollidingLeft = false;
        isCollidingRight = false;
    }

    private void ResolveHorizontalMovement(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + CollisionPadding;

        for (int i = 0; i < numOfHorizontalRays; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? RayCastCorners.bottomLeft : RayCastCorners.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpace * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, moveStats.groundLayer);

            if (hit)
            {
                velocity.x = (hit.distance - CollisionPadding) * directionX;
                rayLength = hit.distance;

                if (directionX == -1)
                    isCollidingLeft = true;
                else if (directionX == 1)
                    isCollidingRight = true;
            }

            #region Debug Visualization
            
            if (moveStats.debugShowWallHit)
            {
                float debugRayLength = moveStats.extraRayDebugDistance;
                Vector2 debugRayOrigin = (directionX == 1) ? RayCastCorners.bottomLeft : RayCastCorners.bottomRight;
                debugRayOrigin += Vector2.up * (horizontalRaySpace * i);

                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.right * directionX, debugRayLength, moveStats.groundLayer);
                Color rayColor = didHit ? Color.cyan : Color.red;
                Debug.DrawRay(debugRayOrigin, Vector2.right * directionX * debugRayLength, rayColor);
            }

            #endregion
        }
    }

    private void ResolveVerticalMovement(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + CollisionPadding;

        for (int i = 0; i < numOfVerticalRays; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? RayCastCorners.bottomLeft : RayCastCorners.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpace * i + velocity.x);
            
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, moveStats.groundLayer);
            if (hit)
            {
                velocity.y = (hit.distance - CollisionPadding) * directionY;
                rayLength = hit.distance;

                if (directionY == -1)
                    isCollidingBelow = true;
                else if (directionY == 1)
                    isCollidingAbove = true;
            }

            #region Debug Visualization

            if (moveStats.debugShowIsGrounded)
            {
                float debugRayLength = moveStats.extraRayDebugDistance;
                Vector2 debugRayOrigin = RayCastCorners.bottomLeft + Vector2.right * (verticalRaySpace * i);
                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.down, debugRayLength, moveStats.groundLayer);
                Color rayColor = didHit ? Color.cyan : Color.red;
                Debug.DrawRay(debugRayOrigin, Vector2.down * debugRayLength, rayColor);

            }

            if (moveStats.debugShowHeadRays)
            {
                float debugRayLength = moveStats.extraRayDebugDistance;
                Vector2 debugRayOrigin = RayCastCorners.topLeft + Vector2.right * (verticalRaySpace * i);
                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.down, debugRayLength, moveStats.groundLayer);
                Color rayColor = didHit ? Color.cyan : Color.red;

                if(i == 0 || i == numOfVerticalRays - 1)
                {
                    rayColor = didHit ? Color.green : Color.magenta;
                }

                Debug.DrawRay(debugRayOrigin, Vector2.up * debugRayLength, rayColor);
            }

            #endregion
        }
    }

    private void UpdateRayCastCorners()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(CollisionPadding * -2);

        RayCastCorners.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayCastCorners.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayCastCorners.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayCastCorners.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(CollisionPadding * -2);

        horizontalRaySpace = bounds.size.y / (numOfHorizontalRays - 1);
        verticalRaySpace = bounds.size.x / (numOfVerticalRays - 1);
    }

    #region Helper Methods

    public bool isGrounded() => isCollidingBelow;

    public bool bumpedHead() => isCollidingAbove;

    public bool isTouchingWall(bool isFacingRight) => (isFacingRight && isCollidingRight) || (!isFacingRight && isCollidingLeft);

    public int getWallDirection()
    {
        if (isCollidingLeft) return -1;
        if (isCollidingRight) return 1;
        return 0;
    }

    #endregion

}
