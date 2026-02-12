using UnityEngine;

[CreateAssetMenu(menuName = "Boss Stats")]
public class BossStats : ScriptableObject
{
    [Header("Basic Stats - Phase 1")]
    public int phase1MaxHealth = 50;
    public int phase1Damage = 2;

    [Header("Basic Stats - Phase 2")]
    public int phase2MaxHealth = 30;
    public int phase2Damage = 3;

    [Header("Movement - Phase 1")]
    public float walkSpeed = 2.5f;

    [Header("Detection")]
    public float detectionRadius = 12f;
    public float attackRange = 3f;

    [Header("Attack Timings")]
    public float baseAttackDuration = 1.0f;
    public float baseAttackCooldown = 2.0f;

    [Header("Charge Attack")]
    public float chargeSpeed = 10f;
    public float chargeDistance = 8f;
    public float chargeStunDuration = 3f;

    [Header("Wave Attack")]
    public float waveAttackDuration = 1.5f;
    public float waveAttackCooldown = 5.0f;

    [Header("Attack Probabilities (Weights)")]
    [Tooltip("Peso del ataque base (más alto = más frecuente)")]
    public int baseAttackWeight = 50;
    [Tooltip("Peso del ataque de carga (más alto = más frecuente)")]
    public int chargeAttackWeight = 15;
    [Tooltip("Peso del ataque de onda (más alto = más frecuente)")]
    public int waveAttackWeight = 35;

    [Header("Attack Hitboxes")]
    public Vector2 baseAttackHitboxSize = new Vector2(2f, 1.5f);
    public Vector2 baseAttackHitboxOffset = new Vector2(1.2f, 0f);

    [Header("Shockwave Prefab")]
    public GameObject shockwavePrefab;
    public int shockwaveDamage = 1;
    public float shockwaveSpeed = 5f;
    public float shockwaveLifetime = 3f;

    [Header("Damage Reaction")]
    public float damageKnockbackForce = 2f;
    public float damageStunDuration = 0.5f;

    [Header("Layers")]
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Debug")]
    public bool showGizmos = true;
}