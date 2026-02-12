using UnityEngine;

/// <summary>
/// Power-up que desbloquea un salto adicional (doble salto, triple salto, etc.).
/// Se puede colocar en el nivel como pickup.
/// </summary>
public class JumpUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] bool destroyOnPickup = true;

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

        Debug.Log("[JumpUpgrade] Collected by player!");

        // Desbloquear salto adicional en GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockJumpUpgrade();
        }
        else
        {
            Debug.LogError("[JumpUpgrade] GameManager not found!");
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

        // Sonido
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    void OnDrawGizmos()
    {
        // Dibujar icono visual en el editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.8f, transform.position + Vector3.up * 1.2f);
    }
}