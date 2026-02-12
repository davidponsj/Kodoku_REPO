using UnityEngine;

/// <summary>
/// Proyectil de onda expansiva. Daña al player usando GameManager.
/// </summary>
public class ShockwaveAttack : MonoBehaviour
{
    Vector2 direction;
    float speed;
    int damage;
    float lifetime;
    Rigidbody2D rb;
    float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 moveDirection, float moveSpeed, int dmg, float life)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = dmg;
        lifetime = life;
        timer = lifetime;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = -1;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(damage, transform.position);
            }
            // La onda NO se destruye al tocar al player
        }
    }
}