using UnityEngine;

/// <summary>
/// Bicho bola - Se hace bola invulnerable al recibir daño
/// </summary>
public class RollBugEnemy : Enemy
{
    [Header("Roll Bug Specific")]
    [SerializeField] float rollDuration = 2f; // Tiempo invulnerable
    [SerializeField] Vector2 patrolPointA = Vector2.zero; // Punto A de patrulla
    [SerializeField] Vector2 patrolPointB = Vector2.zero; // Punto B de patrulla

    bool isRolled; // Estado de bola
    float rollTimer;

    RollBugState rollState;

    enum RollBugState
    {
        Normal,
        Rolled // Invulnerable
    }

    protected override void Awake()
    {
        base.Awake();
        rollState = RollBugState.Normal;
    }

    protected override void Update()
    {
        base.Update();

        // Timer de bola
        if (isRolled)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                ExitRollState();
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
        if (isRolled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPoint = patrolPoints[currentPatrolIndex];
        FaceTarget(targetPoint);

        // Mover hacia el punto
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * stats.patrolSpeed, rb.linearVelocity.y);

        // Comprobar si llegó al punto
        float distance = Vector2.Distance(transform.position, targetPoint);
        if (distance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    protected override void ChaseBehavior()
    {
        if (isRolled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform == null) return;

        FaceTarget(playerTransform.position);

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * stats.chaseSpeed, rb.linearVelocity.y);
    }

    protected override void ReturnBehavior()
    {
        if (isRolled)
        {
            rb. linearVelocity = Vector2.zero;
            return;
        }

        FaceTarget(initialPosition);

        Vector2 direction = (initialPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * stats.chaseSpeed, rb.   linearVelocity.y);

        float distance = Vector2.Distance(transform.position, initialPosition);
        if (distance < 0.5f)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected override void AttackBehavior()
    {
        if (isRolled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    public override void TakeDamage(int damage)
    {
        if (isDead || isRolled) return; // INVULNERABLE cuando está en bola

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{stats.maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Entrar en estado de bola invulnerable
            EnterRollState();
        }
    }

    void EnterRollState()
    {
        isRolled = true;
        rollTimer = rollDuration;
        rollState = RollBugState.Rolled;

        rb.linearVelocity = Vector2.zero;

        // Trigger animación de hacerse bola
        anim.SetTrigger("Roll");
        anim.SetBool("IsRolled", true);

        Debug.Log($"{gameObject.name} entered ROLL state - INVULNERABLE for {rollDuration}s");
    }

    void ExitRollState()
    {
        isRolled = false;
        rollState = RollBugState.Normal;

        anim.SetBool("IsRolled", false);
        anim.SetTrigger("Unroll");

        // Volver al estado normal
        ChangeState(EnemyState.Patrol);

        Debug.Log($"{gameObject.name} exited ROLL state - vulnerable again");
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

        // Indicador visual de invulnerabilidad
        if (Application.isPlaying && isRolled)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}