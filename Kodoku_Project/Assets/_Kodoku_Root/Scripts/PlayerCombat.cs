using UnityEngine;

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

    // Direcciones de ataque
    public enum AttackDirection { None, Forward, Up, Down }
    AttackDirection currentAttack = AttackDirection.None;

    // Input (capturados en el frame)
    bool attackPressed;
    Vector2 moveInput;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Capturar inputs del InputManager estático
        if (InputManager.attackPressed)
        {
            attackPressed = true;
            moveInput = InputManager.movement; // Capturar dirección en el momento del ataque
        }
    }

    void FixedUpdate()
    {
        CountTimers(Time.fixedDeltaTime);
        HandleAttackInput();
        UpdateAttack(Time.fixedDeltaTime);

        // Reset input
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

        // Determinar dirección del ataque según input
        AttackDirection direction = DetermineAttackDirection(moveInput);

        // Almacenar en buffer
        attackBufferTimer = combatStats.attackBufferTime;
        bufferedAttack = direction;

        // Intentar ejecutar inmediatamente si es posible
        TryExecuteBufferedAttack();
    }

    AttackDirection DetermineAttackDirection(Vector2 input)
    {
        // Prioridad: Arriba > Abajo > Adelante
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
        if (isAttacking || attackCooldownTimer > 0f) return;

        // Ejecutar ataque
        StartAttack(bufferedAttack);

        // Resetear buffer
        attackBufferTimer = 0f;
        bufferedAttack = AttackDirection.None;
    }

    void StartAttack(AttackDirection direction)
    {
        isAttacking = true;
        currentAttack = direction;
        attackCooldownTimer = combatStats.attackCooldown;

        // Configurar duración según tipo de ataque
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

        // Activar hitbox durante el ataque
        ActivateHitbox(currentAttack);
    }

    void ActivateHitbox(AttackDirection direction)
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

        // Invertir offset si mira a la izquierda
        if (!playerMovement.isFacingRight)
            hitboxOffset.x *= -1f;

        Vector2 hitboxCenter = (Vector2)transform.position + hitboxOffset;

        // Detectar enemigos
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, combatStats.enemyLayer);

        foreach (var hit in hits)
        {
            // Aquí llamarías a TakeDamage() del enemigo
            Debug.Log($"Hit enemy: {hit.name}");
        }

        // POGO específico para ataque hacia abajo
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
        // Activar animación del farolillo
        Pogo pogo = pogoObject.GetComponent<Pogo>();
        if (pogo != null)
            pogo.Activate();

        // Aplicar impulso hacia arriba al jugador
        playerMovement.Velocity = new Vector2(playerMovement.Velocity.x, combatStats.pogoForce);

        // Cancelar el ataque actual para permitir encadenar otro
        EndAttack();

        Debug.Log("Pogo executed!");
    }

    void EndAttack()
    {
        isAttacking = false;
        currentAttack = AttackDirection.None;
        attackTimer = 0f;
    }

    // Para que Animator pueda acceder
    public bool IsAttacking() => isAttacking;

    void OnDrawGizmos()
    {
        if (combatStats == null) return;
        if (!combatStats.showHitboxGizmos || !isAttacking) return;

        Vector2 hitboxSize = Vector2.zero;
        Vector2 hitboxOffset = Vector2.zero;

        switch (currentAttack)
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