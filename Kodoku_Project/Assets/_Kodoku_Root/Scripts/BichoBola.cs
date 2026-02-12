using UnityEngine;

/// <summary>
/// Bicho bola - Se hace bola invulnerable al llegar a 0 HP
/// </summary>
public class RollBugEnemy : Enemy
{
    [Header("Roll Bug Specific")]
    [SerializeField] float rollDuration = 2f; // Tiempo invulnerable
    [SerializeField] Vector2 patrolPointA = Vector2.zero; // Punto A de patrulla
    [SerializeField] Vector2 patrolPointB = Vector2.zero; // Punto B de patrulla

    bool isRolled; // Estado de bola
    bool isUnrolling; // Estado de desenrollándose
    float rollTimer;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // ARREGLADO - Ignorar colisiones físicas con el player
        // Esto evita que se empujen o se bugeen
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
        if (isRolled || isUnrolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Mantener gravedad
            return;
        }

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
        if (isRolled || isUnrolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (playerTransform == null) return;

        FaceTarget(playerTransform.position);

        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * stats.chaseSpeed, rb.linearVelocity.y);
    }

    protected override void ReturnBehavior()
    {
        if (isRolled || isUnrolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

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
        if (isRolled || isUnrolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // ARREGLADO - Detener completamente al atacar
        // Aplicar pequeña fuerza hacia abajo para mantenerlo en el suelo
        rb.linearVelocity = new Vector2(0, -0.5f);
    }

    public override void TakeDamage(int damage)
    {
        if (isRolled || isInvulnerable) return; // Invulnerable en bola O durante cooldown

        currentHealth -= damage;

        // Activar invulnerabilidad temporal
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{stats.maxHealth}");

        // Si llega a 0 HP → se enrolla y regenera
        if (currentHealth <= 0)
        {
            EnterRollState();
        }
        else
        {
            // Daño normal - trigger animación TakeDamage
            anim.SetTrigger("TakeDamage");
            ChangeState(EnemyState.TakingDamage);

            // Knockback
            if (playerTransform != null)
            {
                Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
                rb.linearVelocity = knockbackDirection * stats.damageKnockbackForce;
            }
        }
    }

    void EnterRollState()
    {
        isRolled = true;
        rollTimer = rollDuration;

        rb.linearVelocity = Vector2.zero;

        // Trigger animación StartMuerte (hacerse bola)
        anim.SetTrigger("Roll");

        Debug.Log($"[{gameObject.name}] ENTERED ROLL STATE - INVULNERABLE for {rollDuration}s");
    }

    void ExitRollState()
    {
        isRolled = false;
        isUnrolling = true; // NUEVO - Marcar que está desenrollándose

        // REGENERAR VIDA COMPLETA
        currentHealth = stats.maxHealth;

        // FORZAR la transición a FinalMuerte
        anim.SetTrigger("Unroll");

        // Debug detallado
        Debug.Log($"[{gameObject.name}] EXITING ROLL STATE:");
        Debug.Log($"  → HP regenerated: {currentHealth}/{stats.maxHealth}");
        Debug.Log($"  → Unroll trigger ACTIVATED");
        Debug.Log($"  → isUnrolling = true (will stay still)");

        // NO cambiar a Patrol aquí - esperar a que termine FinalMuerte
        // El Animation Event OnUnrollComplete() lo hará
    }

    /// <summary>
    /// Llamado por Animation Event al FINAL de la animación FinalMuerte
    /// </summary>
    public void OnUnrollComplete()
    {
        isUnrolling = false; // NUEVO - Ya terminó de desenrollarse

        Debug.Log($"[{gameObject.name}] OnUnrollComplete - Volviendo a Patrol");
        ChangeState(EnemyState.Patrol);
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