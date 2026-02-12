using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Basic Stats")]
    public int maxHealth = 3;
    public int damage = 1;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;

    [Header("Detection")]
    public float detectionRadius = 5f;
    public float returnDistance = 8f; // Distancia desde punto inicial para volver
    public bool requiresLineOfSight = true; // Si requiere "mirar" al player

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.5f;

    [Header("Attack Hitbox")]
    public Vector2 attackHitboxSize = new Vector2(1.5f, 1f);
    public Vector2 attackHitboxOffset = new Vector2(0.75f, 0f);

    [Header("Damage Reaction")]
    public float damageKnockbackForce = 3f;
    public float damageStunDuration = 0.3f;

    [Header("Layers")]
    public LayerMask playerLayer;
    public LayerMask groundLayer; // Para wall crawler

    [Header("Debug")]
    public bool showGizmos = true;
}