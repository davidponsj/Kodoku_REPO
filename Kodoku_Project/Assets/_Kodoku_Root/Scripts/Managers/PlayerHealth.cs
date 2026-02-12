using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Invulnerability")]
    [SerializeField] float invulnerabilityDuration = 1.5f;
    bool isInvulnerable;
    float invulnerabilityTimer;

    [Header("Shield UI Reference")]
    [SerializeField] ShieldUI shieldUI;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float knockbackUpForce = 3f;

    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer spriteRenderer;

    Rigidbody2D rb;
    PlayerMovement playerMovement;
    PlayerCombat playerCombat;
    bool isDead;

    [Header("Visual Feedback")]
    [SerializeField] float blinkInterval = 0.1f;
    Coroutine blinkCoroutine;

    int currentHealth;
    int maxHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Start()
    {
        // Asegurar que el escudo empieza apagado
        if (shieldUI != null)
            shieldUI.SetShieldVisual(0);

        // Escuchar cambios de escudo
        GameManager.Instance.OnShieldChanged += UpdateShieldVisual;
    }

    void UpdateShieldVisual(int shield)
    {
        if (shieldUI != null)
            shieldUI.SetShieldVisual(shield);
    }

    void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;

                // Detener parpadeo
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    blinkCoroutine = null;
                }

                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }
    }

    // Inicializar vida desde GameManager
    public void InitializeHealth(int health, int max)
    {
        currentHealth = health;
        maxHealth = max;
        isDead = false;

        Debug.Log($"[PlayerHealth] Initialized with {currentHealth}/{maxHealth} HP");
    }

    public void UpdateMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = newMax;
    }

    // Efectos visuales al recibir daño
    public void OnDamageTaken(int damage, Vector2 damageSourcePosition)
    {
        if (isDead || isInvulnerable) return;

        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        if (anim != null)
            anim.SetTrigger("Hit");

        ApplyKnockback(damageSourcePosition);

        if (spriteRenderer != null && blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkEffect());

        Debug.Log($"[PlayerHealth] Visual effects for {damage} damage applied");
    }

    void ApplyKnockback(Vector2 damageSourcePosition)
    {
        if (rb == null) return;

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

        Vector2 knockbackVelocity = new Vector2(
            knockbackDirection.x * knockbackForce,
            knockbackUpForce
        );

        if (playerMovement != null)
            playerMovement.Velocity = knockbackVelocity;
    }

    IEnumerator BlinkEffect()
    {
        while (isInvulnerable)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true;
        blinkCoroutine = null;
    }

    // MUERTE DEL PLAYER
    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("[PlayerHealth] Death triggered");

        if (anim != null)
            anim.SetTrigger("Die");

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerCombat != null)
            playerCombat.enabled = false;

        StartCoroutine(DisablePlayerAfterDelay(2f));
        StartCoroutine(RespawnAfterDelay(2f));
    }

    IEnumerator DisablePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Debug.Log("[PlayerHealth] Player disabled after death");
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameManager.Instance.RespawnPlayer();
    }

    // REVIVIR TRAS RESPAWN
    public void Revive()
    {
        isDead = false;

        // Reactivar controles
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerCombat != null)
            playerCombat.enabled = true;

        // Reactivar collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        // Reset animación
        if (anim != null)
            anim.Play("AC_Idle_Player");

        // Reset invulnerabilidad
        isInvulnerable = false;

        // Asegurar sprite visible
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        Debug.Log("[PlayerHealth] Player revived");
    }

    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
}
