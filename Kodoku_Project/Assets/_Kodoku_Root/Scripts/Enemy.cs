using UnityEngine;

/// <summary>
/// Clase base para todos los enemigos.
/// Maneja: vida, daño, detección, estados básicos, muerte
/// </summary>
public abstract class Enemy : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    [SerializeField] protected Animator anim;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    protected Rigidbody2D rb;
    protected Transform playerTransform;

    // Estado
    protected EnemyState currentState;
    protected int currentHealth;
    protected bool isDead;
    protected bool isTakingDamage;
    protected float damageTimer;

    // NUEVO - Invulnerabilidad temporal para evitar múltiples golpes
    protected bool isInvulnerable;
    protected float invulnerabilityTimer;
    protected float invulnerabilityDuration = 0.5f; // Medio segundo de invulnerabilidad

    // Patrulla
    protected Vector2 initialPosition;
    protected int currentPatrolIndex;
    protected Vector2[] patrolPoints;
    protected bool isFacingRight = true;

    // Attack
    protected float attackCooldownTimer;
    protected bool isAttacking;
    protected float attackTimer;

    // Direcciones de ataque posibles
    protected enum AttackDirection { Forward, Up, Down }

    public enum EnemyState
    {
        Patrol,
        Chase,
        Return,
        Attack,
        TakingDamage,
        Dead
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = stats.maxHealth;
        initialPosition = transform.position;

        // Buscar al player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Start()
    {
        SetupPatrolPoints();
        ChangeState(EnemyState.Patrol);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        CountTimers(Time.deltaTime);

        // Máquina de estados
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            case EnemyState.Chase:
                ChaseBehavior();
                break;
            case EnemyState.Return:
                ReturnBehavior();
                break;
            case EnemyState.Attack:
                AttackBehavior();
                break;
            case EnemyState.TakingDamage:
                TakingDamageBehavior();
                break;
        }

        CheckStateTransitions();
    }

    protected virtual void CountTimers(float deltaTime)
    {
        if (damageTimer > 0f)
            damageTimer -= deltaTime;
        else if (isTakingDamage)
        {
            isTakingDamage = false;
            ChangeState(EnemyState.Patrol);
        }

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= deltaTime;

        if (attackTimer > 0f)
            attackTimer -= deltaTime;
        else if (isAttacking)
        {
            EndAttack();
        }

        // NUEVO - Timer de invulnerabilidad
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }
    }

    #region State Machine

    protected virtual void CheckStateTransitions()
    {
        if (isDead || isTakingDamage) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float distanceToInitial = Vector2.Distance(transform.position, initialPosition);

        // Si está muy lejos del punto inicial → Return
        if (distanceToInitial > stats.returnDistance && currentState != EnemyState.Return)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        // Detección del player
        bool playerDetected = IsPlayerDetected();

        if (playerDetected)
        {
            // ARREGLADO - Comprobar si el player está en rango de ataque usando overlap
            bool playerInAttackRange = IsPlayerInAttackRange();

            // Si está en rango de ataque → Attack
            if (playerInAttackRange && attackCooldownTimer <= 0f)
            {
                if (currentState != EnemyState.Attack)
                    ChangeState(EnemyState.Attack);
            }
            // Si está detectado pero no en rango → Chase
            else if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
            {
                ChangeState(EnemyState.Chase);
            }
        }
        else
        {
            // Player no detectado
            if (currentState == EnemyState.Chase)
            {
                // Si está cerca del punto inicial → Patrol
                if (distanceToInitial < 0.5f)
                    ChangeState(EnemyState.Patrol);
                // Si está lejos → Return
                else
                    ChangeState(EnemyState.Return);
            }
        }
    }

    // NUEVO - Método para detectar si el player está en rango de ataque
    protected virtual bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;

        // Crear un área de detección para el ataque (más grande que la hitbox)
        Vector2 attackCheckOffset = new Vector2(isFacingRight ? stats.attackRange : -stats.attackRange, 0);
        Vector2 attackCheckCenter = (Vector2)transform.position + attackCheckOffset;

        // Usar un overlap circle para detectar al player
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCheckCenter, stats.attackRange * 0.8f, stats.playerLayer);

        return hits.Length > 0;
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                // Estado de patrulla - no cambia animator parameters
                break;
            case EnemyState.Chase:
                // Estado de persecución - no cambia animator parameters
                break;
            case EnemyState.Return:
                // Estado de retorno - no cambia animator parameters
                break;
            case EnemyState.Attack:
                isAttacking = true;
                attackTimer = stats.attackDuration;
                attackCooldownTimer = stats.attackCooldown;
                anim.SetTrigger("Attack");
                break;
            case EnemyState.TakingDamage:
                isTakingDamage = true;
                damageTimer = stats.damageStunDuration;
                break;
        }
    }

    #endregion

    #region Behaviors (Abstract - implementados por hijos)

    protected abstract void SetupPatrolPoints();
    protected abstract void PatrolBehavior();
    protected abstract void ChaseBehavior();
    protected abstract void ReturnBehavior();
    protected abstract void AttackBehavior();

    #endregion

    #region Detection

    protected virtual bool IsPlayerDetected()
    {
        if (playerTransform == null) return false;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > stats.detectionRadius)
            return false;

        // SIMPLIFICADO - Siempre detecta si está en rango (sin check de dirección)
        return true;
    }

    #endregion

    #region Combat

    public virtual void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return; // ARREGLADO - evita múltiples golpes

        currentHealth -= damage;

        // Activar invulnerabilidad temporal
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{stats.maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
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

    protected virtual void TakingDamageBehavior()
    {
        // El enemigo está stunneado, no hace nada
        // El timer en CountTimers() manejará la salida de este estado
    }

    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;

        anim.SetBool("IsWalking", false);
        anim.SetBool("IsChasing", false);
        anim.SetTrigger("Die");

        // Desactivar collider
        GetComponent<Collider2D>().enabled = false;

        // Desactivar rigidbody gravity para que no caiga
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // Destruir después de la animación (ajusta el tiempo según tu animación)
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// Llamado por Animation Event para activar la hitbox del ataque
    /// </summary>
    public virtual void ActivateAttackHitbox()
    {
        if (isDead) return;

        Vector2 hitboxOffset = stats.attackHitboxOffset;

        if (!isFacingRight)
            hitboxOffset.x *= -1f;

        Vector2 hitboxCenter = (Vector2)transform.position + hitboxOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitboxCenter,
            stats.attackHitboxSize,
            0f,
            stats.playerLayer
        );

        foreach (var hit in hits)
        {
            // Aquí el Singleton de GameManager manejará el daño al player
            Debug.Log($"Enemy hit player: {hit.name}");

            // TODO: GameManager.Instance.DamagePlayer(stats.damage);
            // Por ahora solo detecta
        }
    }

    /// <summary>
    /// Llamado por Animation Event para desactivar la hitbox del ataque
    /// OPCIONAL - puede usarse si quieres control explícito de cuándo termina el hitbox
    /// </summary>
    public virtual void DeactivateAttackHitbox()
    {
        // Este método existe por si necesitas lógica al desactivar
        // Por ahora no hace nada, pero está disponible para Animation Events
        Debug.Log($"{gameObject.name} - Attack hitbox deactivated");
    }

    protected virtual void EndAttack()
    {
        isAttacking = false;
        attackTimer = 0f;

        // Volver al estado anterior
        if (IsPlayerDetected() && Vector2.Distance(transform.position, playerTransform.position) <= stats.detectionRadius)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);
    }

    #endregion

    #region Movement Helpers

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    protected virtual void FaceTarget(Vector2 target)
    {
        bool shouldFaceRight = target.x > transform.position.x;

        if (shouldFaceRight && !isFacingRight)
            Flip();
        else if (!shouldFaceRight && isFacingRight)
            Flip();
    }

    #endregion

    #region Gizmos

    protected virtual void OnDrawGizmosSelected()
    {
        if (stats == null || !stats.showGizmos) return;

        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRadius);

        // Distancia de retorno
        Vector2 initPos = Application.isPlaying ? initialPosition : (Vector2)transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(initPos, stats.returnDistance);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);

        // Hitbox de ataque
        if (Application.isPlaying && isAttacking)
        {
            Vector2 offset = stats.attackHitboxOffset;
            if (!isFacingRight) offset.x *= -1f;

            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube((Vector2)transform.position + offset, stats.attackHitboxSize);
        }
    }

    #endregion
}