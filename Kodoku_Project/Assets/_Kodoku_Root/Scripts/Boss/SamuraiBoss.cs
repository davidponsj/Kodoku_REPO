using UnityEngine;

public class SamuraiBoss : Boss
{
    [Header("Charge Attack")]
    [SerializeField] LayerMask wallLayer;
    bool isCharging, isStunned;
    float chargeTimer, stunnedTimer;
    Vector2 chargeDirection;

    [Header("Attack Cooldowns")]
    float baseAttackCooldown, waveAttackCooldown;

    [Header("Shockwave Spawn")]
    [SerializeField] float shockwaveSpawnYOffset = -1.5f;

    float attackDecisionTimer;
    float timeBetweenAttacks = 1f;

    protected override void Start()
    {
        base.Start();
        baseAttackCooldown = 0f;
        waveAttackCooldown = 0f;
    }

    protected override void Update()
    {
        base.Update();
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    protected override void UpdateStateBehavior()
    {
        switch (currentState)
        {
            case BossState.Idle: IdleBehavior(); break;
            case BossState.Walking: WalkingBehavior(); break;
            case BossState.BaseAttack: rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); break;
            case BossState.ChargeAttack: ChargeAttackBehavior(); break;
            case BossState.WaveAttack: rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); break;
            case BossState.Stunned: StunnedBehavior(); break;
            case BossState.PhaseTransition: rb.linearVelocity = Vector2.zero; break;
        }
    }

    protected override void CountTimers(float deltaTime)
    {
        base.CountTimers(deltaTime);
        if (baseAttackCooldown > 0f) baseAttackCooldown -= deltaTime;
        if (waveAttackCooldown > 0f) waveAttackCooldown -= deltaTime;
        if (attackDecisionTimer > 0f) attackDecisionTimer -= deltaTime;
        if (isStunned)
        {
            stunnedTimer -= deltaTime;
            if (stunnedTimer <= 0f) ExitStun();
        }
    }

    #region State Behaviors

    void IdleBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (IsPlayerDetected())
        {
            attackDecisionTimer = timeBetweenAttacks;
            ChangeState(BossState.Walking);
        }
    }

    void WalkingBehavior()
    {
        if (!IsPlayerDetected())
        {
            ChangeState(BossState.Idle);
            return;
        }
        FacePlayer();
        float direction = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * stats.walkSpeed, rb.linearVelocity.y);
        if (attackDecisionTimer <= 0f && IsPlayerInAttackRange())
            DecideAttack();
    }

    void DecideAttack()
    {
        int totalWeight = 0;
        bool canBase = baseAttackCooldown <= 0f;
        bool canCharge = true;
        bool canWave = waveAttackCooldown <= 0f;

        if (canBase) totalWeight += stats.baseAttackWeight;
        if (canCharge) totalWeight += stats.chargeAttackWeight;
        if (canWave) totalWeight += stats.waveAttackWeight;

        if (totalWeight == 0)
        {
            StartBaseAttack();
            return;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        if (canBase)
        {
            currentWeight += stats.baseAttackWeight;
            if (randomValue < currentWeight) { StartBaseAttack(); return; }
        }
        if (canCharge)
        {
            currentWeight += stats.chargeAttackWeight;
            if (randomValue < currentWeight) { StartChargeAttack(); return; }
        }
        if (canWave)
        {
            StartWaveAttack();
        }
    }

    void ChargeAttackBehavior()
    {
        if (!isCharging) return;
        rb.linearVelocity = new Vector2(chargeDirection.x * stats.chargeSpeed, rb.linearVelocity.y);
        if (DetectWallCollision()) HitWall();
        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0f) HitWall();
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    #endregion

    #region Attack Starters

    void StartBaseAttack()
    {
        ChangeState(BossState.BaseAttack);
        baseAttackCooldown = stats.baseAttackCooldown;
        anim.SetTrigger("BaseAttack");
    }

    void StartChargeAttack()
    {
        ChangeState(BossState.ChargeAttack);
        FacePlayer();
        chargeDirection = isFacingRight ? Vector2.right : Vector2.left;
        chargeTimer = stats.chargeDistance / stats.chargeSpeed;
        isCharging = true;
        anim.SetTrigger("ChargeStart");
    }

    void StartWaveAttack()
    {
        ChangeState(BossState.WaveAttack);
        waveAttackCooldown = stats.waveAttackCooldown;
        anim.SetTrigger("WaveAttack");
    }

    #endregion

    #region Animation Events

    public void ActivateBaseAttackHitbox()
    {
        Vector2 hitboxOffset = stats.baseAttackHitboxOffset;
        if (!isFacingRight) hitboxOffset.x *= -1f;
        Vector2 hitboxCenter = (Vector2)transform.position + hitboxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, stats.baseAttackHitboxSize, 0f, stats.playerLayer);
        if (hits.Length > 0)
            DamagePlayer(GetDamageForCurrentPhase());
    }

    public void EndBaseAttack()
    {
        attackDecisionTimer = timeBetweenAttacks;
        ChangeState(BossState.Walking);
    }

    public void StartCharging()
    {
        isCharging = true;
        anim.SetTrigger("AttackCharge");
    }

    public void SpawnShockwave()
    {
        if (stats.shockwavePrefab == null) return;
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + shockwaveSpawnYOffset);

        GameObject waveRight = Instantiate(stats.shockwavePrefab, spawnPos, Quaternion.identity);
        ShockwaveAttack waveRightScript = waveRight.GetComponent<ShockwaveAttack>();
        if (waveRightScript != null)
            waveRightScript.Initialize(Vector2.right, stats.shockwaveSpeed, stats.shockwaveDamage, stats.shockwaveLifetime);

        GameObject waveLeft = Instantiate(stats.shockwavePrefab, spawnPos, Quaternion.identity);
        ShockwaveAttack waveLeftScript = waveLeft.GetComponent<ShockwaveAttack>();
        if (waveLeftScript != null)
            waveLeftScript.Initialize(Vector2.left, stats.shockwaveSpeed, stats.shockwaveDamage, stats.shockwaveLifetime);
    }

    public void EndWaveAttack()
    {
        attackDecisionTimer = timeBetweenAttacks;
        ChangeState(BossState.Walking);
    }

    #endregion

    #region Charge Collision

    bool DetectWallCollision()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, 1f, wallLayer);
        return hit.collider != null;
    }

    void HitWall()
    {
        isCharging = false;
        isStunned = true;
        stunnedTimer = stats.chargeStunDuration;
        rb.linearVelocity = Vector2.zero;
        ChangeState(BossState.Stunned);
        anim.SetTrigger("Fall");
        Invoke(nameof(TriggerGetUp), stats.chargeStunDuration - 0.5f);
    }

    void TriggerGetUp() => anim.SetTrigger("GetUp");

    void ExitStun()
    {
        isStunned = false;
        attackDecisionTimer = timeBetweenAttacks;
        ChangeState(BossState.Walking);
    }

    public void OnGetUpComplete() => ExitStun();

    #endregion

    public void OnDefeatAnimationComplete() => gameObject.SetActive(false);

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (stats == null || !stats.showGizmos) return;
        if (Application.isPlaying && currentState == BossState.BaseAttack)
        {
            Vector2 offset = stats.baseAttackHitboxOffset;
            if (!isFacingRight) offset.x *= -1f;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube((Vector2)transform.position + offset, stats.baseAttackHitboxSize);
        }
    }
}