using UnityEngine;

/// <summary>
/// Cucaracha voladora - Patrulla volando, persigue al player en el aire
/// </summary>
public class Cucaracha : Enemy
{
    [Header("Flying Specific")]
    [SerializeField] Vector2[] customPatrolPoints; // Puntos de patrulla opcionales en Inspector
    [SerializeField] float patrolWaitTime = 1f; // Tiempo de espera en cada punto

    float patrolWaitTimer;
    bool waitingAtPoint;

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
        // Si hay puntos custom en el inspector, usarlos
        if (customPatrolPoints != null && customPatrolPoints.Length > 0)
        {
            patrolPoints = customPatrolPoints;
        }
        else
        {
            // Si no, crear patrulla simple de 2 puntos (izquierda-derecha)
            patrolPoints = new Vector2[2];
            patrolPoints[0] = initialPosition + Vector2.left * 3f;
            patrolPoints[1] = initialPosition + Vector2.right * 3f;
        }

        currentPatrolIndex = 0;
    }

    protected override void PatrolBehavior()
    {
        if (waitingAtPoint)
        {
            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0f)
            {
                waitingAtPoint = false;
                // Siguiente punto
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }

            return;
        }

        Vector2 targetPoint = patrolPoints[currentPatrolIndex];
        FaceTarget(targetPoint);

        // Moverse hacia el punto
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * stats.patrolSpeed;

        // Comprobar si llegó al punto
        float distance = Vector2.Distance(transform.position, targetPoint);
        if (distance < 0.3f)
        {
            waitingAtPoint = true;
            patrolWaitTimer = patrolWaitTime;
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected override void ChaseBehavior()
    {
        if (playerTransform == null) return;

        FaceTarget(playerTransform.position);

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * stats.chaseSpeed;
    }

    protected override void ReturnBehavior()
    {
        FaceTarget(initialPosition);

        Vector2 direction = (initialPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * stats.chaseSpeed;

        // Si llegó cerca del punto inicial, volver a Patrol
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

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (stats == null || !stats.showGizmos) return;

        // Dibujar puntos de patrulla
        if (customPatrolPoints != null && customPatrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < customPatrolPoints.Length; i++)
            {
                Gizmos.DrawWireSphere(customPatrolPoints[i], 0.3f);

                // Línea al siguiente punto
                int nextIndex = (i + 1) % customPatrolPoints.Length;
                Gizmos.DrawLine(customPatrolPoints[i], customPatrolPoints[nextIndex]);
            }
        }
    }
}