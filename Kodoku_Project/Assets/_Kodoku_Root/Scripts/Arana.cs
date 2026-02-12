using UnityEngine;

/// <summary>
/// Ara�a - Camina por suelo, paredes y techos usando raycast
/// </summary>
public class Arana : Enemy
{
    [Header("Wall Crawler Specific")]
    [SerializeField] float raycastDistance = 0.6f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool startGoingRight = true;

    Vector2 currentSurfaceNormal = Vector2.up;
    bool movingRight;

    protected override void Start()
    {
        base.Start();
        movingRight = startGoingRight;

        if (movingRight && !isFacingRight)
            Flip();
        else if (!movingRight && isFacingRight)
            Flip();
    }

    protected override void SetupPatrolPoints()
    {
        // La ara�a no usa puntos de patrulla, se mueve continuamente
        // detectando el borde y dando la vuelta
    }

    protected override void PatrolBehavior()
    {
        WalkOnSurface(stats.patrolSpeed);
    }

    protected override void ChaseBehavior()
    {
        if (playerTransform == null) return;

        // Determinar si debe ir hacia la derecha o izquierda para perseguir
        bool playerIsOnRight = playerTransform.position.x > transform.position.x;

        if (playerIsOnRight != movingRight)
        {
            movingRight = playerIsOnRight;
            Flip();
        }

        WalkOnSurface(stats.chaseSpeed);
    }

    protected override void ReturnBehavior()
    {
        // Determinar direcci�n hacia el punto inicial
        bool initialIsOnRight = initialPosition.x > transform.position.x;

        if (initialIsOnRight != movingRight)
        {
            movingRight = initialIsOnRight;
            Flip();
        }

        WalkOnSurface(stats.chaseSpeed);

        // Si lleg� cerca del punto inicial, volver a Patrol
        float distance = Vector2.Distance(transform.position, initialPosition);
        if (distance < 0.5f)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected override void AttackBehavior()
    {
        // Detenerse al atacar
        rb.linearVelocity = Vector2.zero;
    }

    void WalkOnSurface(float speed)
    {
        // Detectar superficie debajo/delante
        DetectSurface();

        // Mover en la direcci�n de la superficie
        Vector2 moveDirection = movingRight ? Vector2.right : Vector2.left;

        // Rotar la direcci�n seg�n la normal de la superficie
        moveDirection = RotateVectorByNormal(moveDirection, currentSurfaceNormal);

        rb.linearVelocity = moveDirection * speed;

        // Rotar el sprite seg�n la superficie
        AlignToSurface();
    }

    void DetectSurface()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = -currentSurfaceNormal; // Apuntar hacia la superficie

        // Raycast hacia abajo/superficie
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance, groundLayer);

        if (hit.collider != null)
        {
            currentSurfaceNormal = hit.normal;

            // Ajustar posici�n para pegarse a la superficie
            float distanceToSurface = hit.distance;
            if (distanceToSurface > 0.4f)
            {
                transform.position = hit.point + hit.normal * 0.4f;
            }
        }
        else
        {
            // No hay superficie, detectar borde y dar la vuelta
            DetectEdge();
        }
    }

    void DetectEdge()
    {
        // Raycast hacia adelante para detectar si hay suelo/pared
        Vector2 forwardDirection = movingRight ? Vector2.right : Vector2.left;
        Vector2 rayOrigin = (Vector2)transform.position + forwardDirection * 0.3f;

        RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, -currentSurfaceNormal, raycastDistance, groundLayer);

        if (hitForward.collider == null)
        {
            // No hay suelo adelante, dar la vuelta
            movingRight = !movingRight;
            Flip();
        }
    }

    Vector2 RotateVectorByNormal(Vector2 vector, Vector2 normal)
    {
        // Rotar el vector para que sea perpendicular a la normal
        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
        return Quaternion.Euler(0, 0, angle) * vector;
    }

    void AlignToSurface()
    {
        // Rotar el sprite para alinearse con la superficie
        float angle = Mathf.Atan2(currentSurfaceNormal.y, currentSurfaceNormal.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (stats == null || !stats.showGizmos) return;

        // Dibujar raycast de superficie
        Gizmos.color = Color.green;
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Application.isPlaying ? -currentSurfaceNormal : Vector2.down;
        Gizmos.DrawRay(rayOrigin, rayDirection * raycastDistance);

        // Dibujar raycast de detecci�n de borde
        Gizmos.color = Color.blue;
        Vector2 forwardDirection = (Application.isPlaying && movingRight) || (!Application.isPlaying && startGoingRight)
            ? Vector2.right : Vector2.left;
        Vector2 edgeRayOrigin = (Vector2)transform.position + forwardDirection * 0.3f;
        Gizmos.DrawRay(edgeRayOrigin, rayDirection * raycastDistance);
    }
}