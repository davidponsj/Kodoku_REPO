using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] ParticleSystem pickupParticles;
    [SerializeField] AudioClip pickupSound;

    bool collected = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !collected)
        {
            collected = true;

            GameManager.Instance.AddShield();

            if (pickupParticles != null)
                Instantiate(pickupParticles, transform.position, Quaternion.identity);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}
