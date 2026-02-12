using UnityEngine;

/// <summary>
/// Zona de daño de enemigos - Trigger que detecta al player y le hace daño
/// </summary>
public class EnemyDamageZone : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 damagePosition = transform.position;
            GameManager.Instance.DamagePlayer(damage, damagePosition);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 damagePosition = transform.position;
            GameManager.Instance.DamagePlayer(damage, damagePosition);
        }
    }
}