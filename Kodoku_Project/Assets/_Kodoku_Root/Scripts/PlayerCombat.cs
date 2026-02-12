using UnityEditor.PackageManager.UI;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public CombatStats combatStats;
    [SerializeField] Animator anim;

    PlayerMovement playerMovement;
    Rigidbody2D rb;

    // Estados de ataque
    bool isAttacking;
    float attackTimer;
    float attackCooldownTimer;

    // Input buffer
    float attackBufferTimer;
    AttackDirection bufferedAttack;

    // NUEVO - Control de hitbox
    bool hitboxActive;
    AttackDirection activeHitboxDirection;

    // Direcciones de ataque
    public enum AttackDirection { None, Forward, Up, Down }
    AttackDirection currentAttack = AttackDirection.None;

    // Input
    bool attackPressed;
    Vector2 moveInput;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (InputManager.attackPressed)
        {
            attackPressed = true;
            moveInput = InputManager.movement;
        }
    }

    void FixedUpdate()
    {
        CountTimers(Time.fixedDeltaTime);
        HandleAttackInput();
        UpdateAttack(Time.fixedDeltaTime);

        // NUEVO - Solo detectar colisiones cuando la hitbox está activa
        if (hitboxActive)
        {
            CheckHitbox(activeHitboxDirection);
        }

        attackPressed = false;
    }

    void CountTimers(float deltaTime)
    {
        if (attackBufferTimer > 0f)
            attackBufferTimer -= deltaTime;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= deltaTime;

        if (attackTimer > 0f)
            attackTimer -= deltaTime;
    }

    void HandleAttackInput()
    {
        if (!attackPressed) return;

        AttackDirection direction = DetermineAttackDirection(moveInput);

        attackBufferTimer = combatStats.attackBufferTime;
        bufferedAttack = direction;

        TryExecuteBufferedAttack();
    }

    AttackDirection DetermineAttackDirection(Vector2 input)
    {
        if (input.y > 0.5f)
            return AttackDirection.Up;
        else if (input.y < -0.5f)
            return AttackDirection.Down;
        else
            return AttackDirection.Forward;
    }

    void TryExecuteBufferedAttack()
    {
        if (attackBufferTimer <= 0f) return;
        if (bufferedAttack == AttackDirection.None) return;

        // MODIFICADO - Cooldown previene spam
        if (isAttacking || attackCooldownTimer > 0f) return;

        StartAttack(bufferedAttack);

        attackBufferTimer = 0f;
        bufferedAttack = AttackDirection.None;
    }

    void StartAttack(AttackDirection direction)
    {
        isAttacking = true;
        currentAttack = direction;

        // MODIFICADO - Cooldown se inicia al comenzar el ataque
        attackCooldownTimer = combatStats.attackCooldown;

        // NUEVO - Hitbox desactivada al inicio
        hitboxActive = false;

        switch (direction)
        {
            case AttackDirection.Forward:
                attackTimer = combatStats.forwardAttackDuration;
                anim.SetTrigger("AttackForward");
                break;
            case AttackDirection.Up:
                attackTimer = combatStats.upAttackDuration;
                anim.SetTrigger("AttackUp");
                break;
            case AttackDirection.Down:
                attackTimer = combatStats.downAttackDuration;
                anim.SetTrigger("AttackDown");
                break;
        }
    }

    void UpdateAttack(float deltaTime)
    {
        if (!isAttacking) return;

        if (attackTimer <= 0f)
        {
            EndAttack();
            return;
        }
    }

    // ===== NUEVOS MÉTODOS - LLAMADOS POR ANIMATION EVENTS =====

    /// <summary>
    /// Llamado por Animation Event para activar la hitbox
    /// </summary>
    public void ActivateHitbox()
    {
        hitboxActive = true;
        activeHitboxDirection = currentAttack;
    }

    /// <summary>
    /// Llamado por Animation Event para desactivar la hitbox
    /// </summary>
    public void DeactivateHitbox()
    {
        hitboxActive = false;
    }

    // ===== FIN NUEVOS MÉTODOS =====

    void CheckHitbox(AttackDirection direction)
    {
        Vector2 hitboxSize = Vector2.zero;
        Vector2 hitboxOffset = Vector2.zero;

        switch (direction)
        {
            case AttackDirection.Forward:
                hitboxSize = combatStats.forwardHitboxSize;
                hitboxOffset = combatStats.forwardHitboxOffset;
                break;
            case AttackDirection.Up:
                hitboxSize = combatStats.upHitboxSize;
                hitboxOffset = combatStats.upHitboxOffset;
                break;
            case AttackDirection.Down:
                hitboxSize = combatStats.downHitboxSize;
                hitboxOffset = combatStats.downHitboxOffset;
                break;
        }

        if (!playerMovement.isFacingRight)
            hitboxOffset.x *= -1f;

        Vector2 hitboxCenter = (Vector2)transform.position + hitboxOffset;

        // Detectar enemigos
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, combatStats.enemyLayer);

        foreach (var hit in hits)
        {
            Debug.Log($"Hit enemy: {hit.name}");
            // TODO: hit.GetComponent<Enemy>().TakeDamage(damage);
        }

        // POGO para ataque hacia abajo
        if (direction == AttackDirection.Down)
        {
            Collider2D[] pogoHits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, combatStats.pogoLayer);

            if (pogoHits.Length > 0)
            {
                ExecutePogo(pogoHits[0]);
            }
        }
    }

    void ExecutePogo(Collider2D pogoObject)
    {
        Pogo pogo = pogoObject.GetComponent<Pogo>();
        if (pogo != null)
            pogo.Activate();

        playerMovement.Velocity = new Vector2(playerMovement.Velocity.x, combatStats.pogoForce);

        EndAttack();

        Debug.Log("Pogo executed!");
    }

    void EndAttack()
    {
        isAttacking = false;
        currentAttack = AttackDirection.None;
        attackTimer = 0f;

        // NUEVO - Asegurar que hitbox se desactiva
        hitboxActive = false;
    }

    public bool IsAttacking() => isAttacking;

    void OnDrawGizmos()
    {
        if (combatStats == null) return;

        // MODIFICADO - Solo mostrar cuando hitbox está activa
        if (!combatStats.showHitboxGizmos || !hitboxActive) return;

        Vector2 hitboxSize = Vector2.zero;
        Vector2 hitboxOffset = Vector2.zero;

        switch (activeHitboxDirection)
        {
            case AttackDirection.Forward:
                hitboxSize = combatStats.forwardHitboxSize;
                hitboxOffset = combatStats.forwardHitboxOffset;
                Gizmos.color = Color.red;
                break;
            case AttackDirection.Up:
                hitboxSize = combatStats.upHitboxSize;
                hitboxOffset = combatStats.upHitboxOffset;
                Gizmos.color = Color.blue;
                break;
            case AttackDirection.Down:
                hitboxSize = combatStats.downHitboxSize;
                hitboxOffset = combatStats.downHitboxOffset;
                Gizmos.color = Color.yellow;
                break;
        }

        if (playerMovement != null && !playerMovement.isFacingRight)
            hitboxOffset.x *= -1f;

        Vector2 center = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}