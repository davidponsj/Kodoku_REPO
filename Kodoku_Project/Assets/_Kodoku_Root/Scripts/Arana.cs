using UnityEngine;

/// <summary>
/// Araña - Camina por el suelo (versión simplificada sin trepar paredes)
/// </summary>
public class Arana : Enemy
{
    [Header("Spider Specific")]
    [SerializeField] Vector2 patrolPointA = Vector2.zero;
    [SerializeField] Vector2 patrolPointB = Vector2.zero;

    protected override void Start()
    {
        base.Start();

        // ARREGLADO - Ignorar colisiones físicas con el player
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            Collider2D enemyCollider = GetComponent<Collider2D>();

            if (playerCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
                Debug.Log($"[{gameObject.name}] Ignoring collisions with player");
            }
        }
    }

    protected override void SetupPatrolPoints()
    {
        // Si no se configuraron puntos en el inspector, usar posición inicial
        if (patrolPointA == Vector2.zero && patrolPointB == Vector2.zero)
        {
            patrolPoints = new Vector2[2];
            patrolPoints[0] = initialPosition + Vector2.left * 3f;
            patrolPoints[1] = initialPosition + Vector2.right * 3f;
        }
        else
        {
            patrolPoints = new Vector2[2];
            patrolPoints[0] = patrolPointA;
            patrolPoints[1] = patrolPointB;
        }

        currentPatrolIndex = 0;
    }

    protected override void PatrolBehavior()
    {
        Vector2 targetPoint = patrolPoints[currentPatrolIndex];
        FaceTarget(targetPoint);

        // Mover SOLO horizontal, mantener gravedad vertical
        float direction = Mathf.Sign(targetPoint.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * stats.patrolSpeed, rb.linearVelocity.y);

        // Comprobar si llegó al punto (solo eje X)
        float distanceX = Mathf.Abs(transform.position.x - targetPoint.x);
        if (distanceX < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    protected override void ChaseBehavior()
    {
        if (playerTransform == null) return;

        FaceTarget(playerTransform.position);

        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * stats.chaseSpeed, rb.linearVelocity.y);
    }

    protected override void ReturnBehavior()
    {
        FaceTarget(initialPosition);

        float direction = Mathf.Sign(initialPosition.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * stats.chaseSpeed, rb.linearVelocity.y);

        float distanceX = Mathf.Abs(transform.position.x - initialPosition.x);
        if (distanceX < 0.5f)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected override void AttackBehavior()
    {
        // Detenerse al atacar (solo horizontal)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (stats == null || !stats.showGizmos) return;

        // Dibujar puntos de patrulla
        if (patrolPointA != Vector2.zero && patrolPointB != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolPointA, 0.3f);
            Gizmos.DrawWireSphere(patrolPointB, 0.3f);
            Gizmos.DrawLine(patrolPointA, patrolPointB);
        }
    }
}