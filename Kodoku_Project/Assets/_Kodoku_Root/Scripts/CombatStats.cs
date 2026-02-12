using UnityEngine;

[CreateAssetMenu(menuName = "Player Combat Stats")]
public class CombatStats : ScriptableObject
{
    [Header("Attack Timing")]
    public float forwardAttackDuration = 0.3f;
    public float upAttackDuration = 0.35f;
    public float downAttackDuration = 0.25f;

    [Header("Attack Cooldowns")]
    public float attackCooldown = 0.1f;

    [Header("Input Buffer")]
    [Range(0f, 0.5f)] public float attackBufferTime = 0.15f;

    [Header("Hitbox Settings")]
    public Vector2 forwardHitboxSize = new Vector2(1.5f, 1f);
    public Vector2 forwardHitboxOffset = new Vector2(0.75f, 0f);

    public Vector2 upHitboxSize = new Vector2(1f, 1.5f);
    public Vector2 upHitboxOffset = new Vector2(0f, 1f);

    public Vector2 downHitboxSize = new Vector2(1f, 1.5f);
    public Vector2 downHitboxOffset = new Vector2(0f, -0.75f);

    [Header("Down Attack / Pogo")]
    public float pogoForce = 15f;
    public LayerMask pogoLayer;

    [Header("Combat Layers")]
    public LayerMask enemyLayer;

    [Header("Damage")] // NUEVO
    public int baseDamage = 1; // Daño base del player (puede ser modificado por el Singleton)

    [Header("Debug")]
    public bool showHitboxGizmos = true;
}