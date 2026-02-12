using UnityEngine;

public class RespawnStatue : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] Animator anim;

    [Header("Settings")]
    [SerializeField] string activateTrigger = "Activate";
    [SerializeField] string activatedBool = "Activated";

    bool isActivated = false;

    void Awake()
    {
        // Asegurar que empieza apagada
        anim.SetBool(activatedBool, false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        // Si ya está activada, no hacer nada
        if (isActivated)
            return;

        // Activar estatua
        ActivateStatue();
    }

    void ActivateStatue()
    {
        isActivated = true;

        // Animación de activación
        anim.SetTrigger(activateTrigger);

        // Cambiar al estado idle activado
        anim.SetBool(activatedBool, true);

        // Registrar este punto como respawn
        GameManager.Instance.SetRespawnPoint(transform.position);

        Debug.Log("[RespawnStatue] Nueva estatua activada como punto de respawn");
    }
}
