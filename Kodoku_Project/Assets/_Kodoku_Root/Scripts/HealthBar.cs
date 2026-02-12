using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] Image healthFill;

    void Start()
    {
        // Suscribirse al evento del GameManager
        GameManager.Instance.OnHealthChanged += UpdateHealthBar;

        // Inicializar barra al empezar
        UpdateHealthBar(GameManager.Instance.GetCurrentHealth(), GameManager.Instance.GetMaxHealth());
    }

    void UpdateHealthBar(int current, int max)
    {
        healthFill.fillAmount = (float)current / max;
    }
}
