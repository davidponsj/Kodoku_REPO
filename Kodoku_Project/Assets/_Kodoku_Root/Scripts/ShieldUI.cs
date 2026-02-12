using UnityEngine;
using UnityEngine.UI;

public class ShieldUI : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Image shieldImage;

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (shieldImage == null) shieldImage = GetComponent<Image>();

        // Empezar apagado
        SetShieldVisual(0);
    }

    public void SetShieldVisual(int shield)
    {
        anim.SetInteger("ShieldState", shield);

        // Mostrar solo si shield > 0
        shieldImage.enabled = shield > 0;
    }
}
