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
        // IMPORTANTE: evitar null
        if (shieldUI != null)
        {
            shieldUI.SetShieldVisual(0); // empezar apagado
        }

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

                // Detener parpadeo y asegurar que sprite es visible
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

    /// <summary>
    /// Inicializar valores de vida (llamado por GameManager)
    /// </summary>
    public void InitializeHealth(int health, int max)
    {
        currentHealth = health;
        maxHealth = max;
        isDead = false;

        Debug.Log($"[PlayerHealth] Initialized with {currentHealth}/{maxHealth} HP");
    }

    /// <summary>
    /// Actualizar vida máxima cuando se obtiene upgrade
    /// </summary>
    public void UpdateMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = newMax; // Curar al máximo
    }

    /// <summary>
    /// Llamado por GameManager cuando el player recibe daño.
    /// Solo maneja efectos visuales (animación, knockback, parpadeo).
    /// </summary>
    public void OnDamageTaken(int damage, Vector2 damageSourcePosition)
    {
        if (isDead || isInvulnerable) return;

        // Activar invulnerabilidad
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        // Activar animación de daño
        if (anim != null)
            anim.SetTrigger("Hit");

        // Aplicar knockback
        ApplyKnockback(damageSourcePosition);

        // Iniciar parpadeo visual
        if (spriteRenderer != null && blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkEffect());

        Debug.Log($"[PlayerHealth] Visual effects for {damage} damage applied");
    }

    void ApplyKnockback(Vector2 damageSourcePosition)
    {
        if (rb == null) return;

        // Calcular dirección del knockback (alejarse de la fuente de daño)
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

        // Aplicar knockback con componente vertical
        Vector2 knockbackVelocity = new Vector2(
            knockbackDirection.x * knockbackForce,
            knockbackUpForce
        );

        // Sobreescribir velocidad del player
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

        // Asegurar que sprite queda visible al final
        spriteRenderer.enabled = true;
        blinkCoroutine = null;
    }

    /// <summary>
    /// Llamado por GameManager cuando el player muere
    /// </summary>
    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("[PlayerHealth] Death triggered");

        // Activar animación de muerte
        if (anim != null)
            anim.SetTrigger("Die");

        // Detener movimiento
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Desactivar controles
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Desactivar combate
        if (playerCombat != null)
            playerCombat.enabled = false;

        // Desactivar collider después de 2 segundos
        StartCoroutine(DisablePlayerAfterDelay(2f));
    }

    IEnumerator DisablePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Desactivar collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Debug.Log("[PlayerHealth] Player disabled after death");

        // TODO: Aquí GameManager podría mostrar Game Over
    }

    // Getters públicos
    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
}