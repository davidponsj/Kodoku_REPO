using UnityEngine;
using System;

/// <summary>
/// Singleton que gestiona el estado global del juego: vida del player, upgrades, etc.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Health Settings")]
    [SerializeField] int startingMaxHealth = 5;
    [SerializeField] int healthUpgradeAmount = 2; // Cuánta vida aumenta cada upgrade

    int currentMaxHealth;
    int currentHealth;

    [Header("Player Upgrades")]
    [SerializeField] int startingMaxJumps = 1; // Solo salto simple al inicio
    int maxJumpsAllowed = 1;

    [Header("References")]
    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    Vector3 respawnPoint;


    [Header("Shield Settings")]
    [SerializeField] int maxShield = 2;
    [SerializeField] float shieldRegenTimePerPoint = 10f;

    int currentShield;
    float shieldRegenTimer;

    public event Action<int> OnShieldChanged; // Para PlayerHealth


    // Eventos para UI
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action<int> OnMaxHealthIncreased; // (newMaxHealth)
    public event Action<int> OnJumpUpgradeObtained; // (newMaxJumps)

    void Awake()
    {
        currentShield = 0;
        shieldRegenTimer = 0f;
        respawnPoint = player.transform.position; // primer respawn por defecto

        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializar valores
        currentMaxHealth = startingMaxHealth;
        currentHealth = currentMaxHealth;
        maxJumpsAllowed = startingMaxJumps;
    }

    void Update()
    {
        HandleShieldRegen();
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
    }

    void HandleShieldRegen()
    {
        if (currentShield >= maxShield)
            return;

        shieldRegenTimer += Time.deltaTime;

        if (shieldRegenTimer >= shieldRegenTimePerPoint)
        {
            shieldRegenTimer = 0f;
            currentShield++;

            OnShieldChanged?.Invoke(currentShield);
            Debug.Log($"[GameManager] Shield regenerated to {currentShield}");
        }
    }

    public void AddShield()
    {
        currentShield = maxShield;      // 2 golpes
        shieldRegenTimer = 0f;          // reiniciar regeneración

        OnShieldChanged?.Invoke(currentShield);

        Debug.Log("[GameManager] Shield restored to FULL!");
    }


    void Start()
    {
        // Encontrar referencias al player
        FindPlayerReferences();

        // Aplicar valores iniciales
        if (playerHealth != null)
            playerHealth.InitializeHealth(currentHealth, currentMaxHealth);

        if (playerMovement != null)
            ApplyJumpUpgrade();
    }

    void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("[GameManager] Player not found!");
        }
    }

    #region Health Management

    /// <summary>
    /// El player recibe daño. Llamado por enemigos/bosses.
    /// </summary>
    public void DamagePlayer(int damage, Vector2 damageSourcePosition)
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[GameManager] PlayerHealth reference is null!");
            return;
        }

        if (playerHealth.IsDead() || playerHealth.IsInvulnerable())
            return;

        // -----------------------------------------
        // 1. EL ESCUDO ABSORBE EL DAÑO PRIMERO
        // -----------------------------------------
        if (currentShield > 0)
        {
            currentShield--;                 // Reducir escudo
            shieldRegenTimer = 0f;           // Reiniciar regeneración
            OnShieldChanged?.Invoke(currentShield);

            Debug.Log($"[GameManager] Shield absorbed hit. Shield = {currentShield}");

            // Activar efectos visuales de daño (knockback, parpadeo)
            playerHealth.OnDamageTaken(1, damageSourcePosition);

            return; // No tocar vida
        }

        // -----------------------------------------
        // 2. SI NO HAY ESCUDO → DAÑO A LA VIDA
        // -----------------------------------------
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[GameManager] Player took {damage} damage. HP: {currentHealth}/{currentMaxHealth}");

        // Efectos visuales
        playerHealth.OnDamageTaken(damage, damageSourcePosition);

        // Actualizar UI de vida
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);

        // -----------------------------------------
        // 3. COMPROBAR MUERTE
        // -----------------------------------------
        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }


    /// <summary>
    /// Versión simplificada sin conocer la fuente
    /// </summary>
    public void DamagePlayer(int damage)
    {
        if (playerMovement != null)
        {
            Vector2 damageSource = playerMovement.transform.position +
                (playerMovement.isFacingRight ? Vector3.right : Vector3.left);
            DamagePlayer(damage, damageSource);
        }
        else
        {
            DamagePlayer(damage, Vector2.zero);
        }
    }

    /// <summary>
    /// Curar al player (corazones, pociones, etc.)
    /// </summary>
    public void HealPlayer(int amount)
    {
        if (playerHealth == null || playerHealth.IsDead())
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, currentMaxHealth);

        Debug.Log($"[GameManager] Player healed {amount}. HP: {currentHealth}/{currentMaxHealth}");

        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }

    /// <summary>
    /// Aumentar la vida MÁXIMA del player (power-up permanente)
    /// </summary>
    public void IncreaseMaxHealth()
    {
        currentMaxHealth += healthUpgradeAmount;
        currentHealth = currentMaxHealth; // Curar al máximo al obtener upgrade

        Debug.Log($"[GameManager] Max health increased! New max: {currentMaxHealth}");

        OnMaxHealthIncreased?.Invoke(currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);

        // Actualizar PlayerHealth
        if (playerHealth != null)
            playerHealth.UpdateMaxHealth(currentMaxHealth);
    }

    void PlayerDied()
    {
        Debug.Log("[GameManager] Player died!");

        if (playerHealth != null)
            playerHealth.TriggerDeath();

        // TODO: Game Over screen, respawn, etc.
    }

    #endregion

    #region Jump Upgrade Management

    /// <summary>
    /// Desbloquear doble salto (o triple, etc.)
    /// </summary>
    public void UnlockJumpUpgrade()
    {
        maxJumpsAllowed++;

        Debug.Log($"[GameManager] Jump upgrade unlocked! Max jumps: {maxJumpsAllowed}");

        OnJumpUpgradeObtained?.Invoke(maxJumpsAllowed);

        ApplyJumpUpgrade();
    }

    void ApplyJumpUpgrade()
    {
        if (playerMovement == null || playerMovement.movementStats == null)
            return;

        // Modificar el ScriptableObject directamente
        playerMovement.movementStats.jumpsAllowed = maxJumpsAllowed;

        Debug.Log($"[GameManager] Applied jump upgrade. Player can now jump {maxJumpsAllowed} times");
    }

    #endregion

    #region Getters

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => currentMaxHealth;
    public int GetMaxJumps() => maxJumpsAllowed;
    public bool IsPlayerDead() => currentHealth <= 0;

    #endregion

    #region Debug

    [ContextMenu("Debug: Damage Player (1 HP)")]
    void DebugDamagePlayer()
    {
        DamagePlayer(1);
    }

    [ContextMenu("Debug: Heal Player (2 HP)")]
    void DebugHealPlayer()
    {
        HealPlayer(2);
    }

    [ContextMenu("Debug: Increase Max Health")]
    void DebugIncreaseMaxHealth()
    {
        IncreaseMaxHealth();
    }

    [ContextMenu("Debug: Unlock Jump Upgrade")]
    void DebugUnlockJump()
    {
        UnlockJumpUpgrade();
    }

    #endregion
}