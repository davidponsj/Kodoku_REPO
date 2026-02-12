using UnityEngine;

/// <summary>
/// Clase base para bosses. Usa GameManager para hacer daño al player.
/// </summary>
public abstract class Boss : MonoBehaviour
{
    [Header("References")]
    public BossStats stats;
    [SerializeField] protected Animator anim;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    protected Rigidbody2D rb;
    protected Transform playerTransform;

    // Estado
    protected BossState currentState;
    protected int currentPhase = 1;
    protected int currentHealth;
    protected bool isDead;
    protected bool isTakingDamage;
    protected float damageTimer;

    // Dirección
    protected bool isFacingRight = false;

    // Invulnerabilidad
    protected bool isInvulnerable;
    protected float invulnerabilityTimer;
    protected float invulnerabilityDuration = 0.5f;

    public enum BossState
    {
        Idle, Walking, BaseAttack, ChargeAttack, WaveAttack,
        Stunned, TakingDamage, PhaseTransition, Dead
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Start()
    {
        currentHealth = stats.phase1MaxHealth;
        ChangeState(BossState.Idle);

        // Ignorar colisiones con player
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            Collider2D bossCollider = GetComponent<Collider2D>();
            if (playerCollider != null && bossCollider != null)
                Physics2D.IgnoreCollision(playerCollider, bossCollider, true);
        }

        // Ignorar pogos
        GameObject[] pogos = GameObject.FindGameObjectsWithTag("Pogo");
        Collider2D bossCol = GetComponent<Collider2D>();
        foreach (GameObject pogo in pogos)
        {
            Collider2D pogoCol = pogo.GetComponent<Collider2D>();
            if (pogoCol != null && bossCol != null)
                Physics2D.IgnoreCollision(bossCol, pogoCol, true);
        }

        int pogoLayer = LayerMask.NameToLayer("Pogo");
        if (pogoLayer != -1)
            Physics2D.IgnoreLayerCollision(gameObject.layer, pogoLayer, true);
    }

    protected virtual void Update()
    {
        if (isDead) return;
        CountTimers(Time.deltaTime);
        UpdateStateBehavior();
    }

    protected virtual void CountTimers(float deltaTime)
    {
        if (damageTimer > 0f)
            damageTimer -= deltaTime;
        else if (isTakingDamage)
        {
            isTakingDamage = false;
            ChangeState(BossState.Idle);
        }

        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }
    }

    protected abstract void UpdateStateBehavior();

    protected virtual void ChangeState(BossState newState)
    {
        currentState = newState;
    }

    #region Combat

    public virtual void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        if (currentHealth <= 0)
        {
            if (currentPhase == 1)
                StartPhaseTransition();
            else
                Die();
        }
        else
        {
            anim.SetTrigger("TakeDamage");
            ChangeState(BossState.TakingDamage);

            if (playerTransform != null)
            {
                Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
                rb.linearVelocity = new Vector2(knockbackDirection.x * stats.damageKnockbackForce, rb.linearVelocity.y);
            }
        }
    }

    /// <summary>
    /// Hacer daño al player usando GameManager
    /// </summary>
    protected virtual void DamagePlayer(int damage)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[BOSS] GameManager not found!");
            return;
        }

        GameManager.Instance.DamagePlayer(damage, transform.position);
    }

    protected virtual void StartPhaseTransition()
    {
        currentPhase = 2;
        ChangeState(BossState.PhaseTransition);
        anim.SetTrigger("Defeat");
    }

    protected virtual void Die()
    {
        isDead = true;
        currentState = BossState.Dead;
        anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    protected int GetMaxHealthForCurrentPhase() => currentPhase == 1 ? stats.phase1MaxHealth : stats.phase2MaxHealth;
    protected int GetDamageForCurrentPhase() => currentPhase == 1 ? stats.phase1Damage : stats.phase2Damage;

    #endregion

    #region Detection

    protected virtual bool IsPlayerDetected()
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= stats.detectionRadius;
    }

    protected virtual bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= stats.attackRange;
    }

    #endregion

    #region Movement

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    protected virtual void FacePlayer()
    {
        if (playerTransform == null) return;
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
    }

    #endregion
}