using UnityEngine;

/// <summary>
/// Power-up que aumenta la vida MÁXIMA del player permanentemente.
/// Se puede colocar en el nivel como pickup.
/// </summary>
public class HealthUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] bool destroyOnPickup = true;
    [SerializeField] bool healToFullOnPickup = true;

    [Header("Visual Feedback")]
    [SerializeField] ParticleSystem pickupParticles;
    [SerializeField] AudioClip pickupSound;

    [Header("Animation (Optional)")]
    [SerializeField] Animator anim;
    [SerializeField] string pickupAnimTrigger = "Pickup";

    bool hasBeenCollected;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenCollected) return;

        if (collision.CompareTag("Player"))
        {
            CollectUpgrade();
        }
    }

    void CollectUpgrade()
    {
        hasBeenCollected = true;

        Debug.Log("[HealthUpgrade] Collected by player!");

        // Aumentar vida máxima en GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncreaseMaxHealth();
        }
        else
        {
            Debug.LogError("[HealthUpgrade] GameManager not found!");
        }

        // Efectos visuales y de sonido
        SpawnPickupEffects();

        // Animar o destruir
        if (destroyOnPickup)
        {
            Destroy(gameObject, 0.1f);
        }
        else if (anim != null && !string.IsNullOrEmpty(pickupAnimTrigger))
        {
            anim.SetTrigger(pickupAnimTrigger);
            // Desactivar collider para que no se recoja dos veces
            GetComponent<Collider2D>().enabled = false;
        }
    }

    void SpawnPickupEffects()
    {
        // Partículas
        if (pickupParticles != null)
        {
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        }

        // Sonido (requiere AudioSource en el objeto o usar AudioSource.PlayClipAtPoint)
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    void OnDrawGizmos()
    {
        // Dibujar icono visual en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position, "d_SceneViewFx", true);
    }
}