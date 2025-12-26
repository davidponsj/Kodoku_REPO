using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] Animator background;
    [SerializeField] Animator worm;
    [SerializeField] MainMenuController menuController;
    [SerializeField] float pressedAnimationDuration = 1f;

    void Awake()
    {
        if (menuController == null)
            Debug.LogError("MenuController no asignado", this);

        if (background == null)
            Debug.LogError("Background Animator no asignado", this);

        if (worm == null)
            Debug.LogError("Worm Animator no asignado", this);
    }

    // Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        menuController.SelectedButton(this);
    }

    // Click
    public void OnPointerClick(PointerEventData eventData)
    {
        menuController.PressedButton(this);
    }

    // Animaciones
    public void Select()
    {
        background.Play("AC_Background_Select");
        worm.Play("AC_Worm_Selected");
    }

    public void Deselect()
    {
        background.Play("AC_Background_Idle");
        worm.Play("AC_Worm_Idle");
    }

    public void Pressed()
    {
        worm.Play("AC_Worm_Confirm");
        background.Play("AC_Background_Idle");
    }

    // Duración animación de confirmación
    public float GetPressedAnimationDuration()
    {
        return pressedAnimationDuration;
    }
}
