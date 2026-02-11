using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] ButtonController[] botones;
    [SerializeField] string playSceneName;
    [SerializeField] float navegationCooldown = 0.25f;

    ButtonController currentSelected = null;
    int currentIndex = 0;

    bool isLocked = false;
    bool canNavegate = true;

    private void Start()
    {
        if (botones.Length >  0)
        {
            currentSelected = botones[currentIndex];
            currentSelected.Select();
        }
    }

//! RATON
    public void SelectedButton(ButtonController boton)
    {
        if (isLocked) return;

        if (currentSelected != null && currentSelected != boton)
        {
            currentSelected.Deselect();
        }

        currentSelected = boton;
        currentIndex = System.Array.IndexOf(botones, boton);

        boton.Select();
    }

    public void PressedButton(ButtonController boton)
    {
        if (isLocked) return;

        if (currentSelected == boton)
        {
            StartCoroutine(ButtonAnimation(boton));
        }
    }

//! MANDO/TECLADO

    public void OnNavegate(InputAction.CallbackContext context)
    {
        if (isLocked || !canNavegate || !context.performed) return;

        float y = context.ReadValue<Vector2>().y;

        if (Mathf.Abs(y) < 0.5f) return;

        int direction = y > 0 ? -1 : 1;
        ChangeSelection(direction);

    }

    public void OnSubmit (InputAction.CallbackContext context)
    {
        if (!context.performed || isLocked) return;

        if (currentSelected != null) StartCoroutine(ButtonAnimation(currentSelected));
    }

    private void ChangeSelection(int direction)
    {
        currentSelected.Deselect();

        currentIndex += direction;

        if (currentIndex < 0) currentIndex = botones.Length - 1;
        else if (currentIndex >= botones.Length) currentIndex = 0;

        currentSelected = botones[currentIndex];
        currentSelected.Select();

        StartCoroutine(NavegationCooldown());
    }

    IEnumerator NavegationCooldown()
    {
        canNavegate = false;
        yield return new WaitForSeconds(navegationCooldown);
        canNavegate = true;
    }
    IEnumerator ButtonAnimation(ButtonController boton)
    {
        isLocked = true;

        boton.Pressed();
        yield return new WaitForSeconds(boton.GetPressedAnimationDuration());

        //SceneManager.LoadScene(playSceneName);
    }
}
