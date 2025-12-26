using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] string playSceneName;

    ButtonController currentSelected = null;
    bool isLocked = false;

    public void SelectedButton(ButtonController boton)
    {
        if (isLocked) return;

        if (currentSelected != null && currentSelected != boton)
        {
            currentSelected.Deselect();
        }

        boton.Select();
        currentSelected = boton;
    }

    public void PressedButton(ButtonController boton)
    {
        if (isLocked) return;

        if (currentSelected == boton)
        {
            StartCoroutine(ButtonAnimation(boton));
        }
    }

    IEnumerator ButtonAnimation(ButtonController boton)
    {
        isLocked = true;

        boton.Pressed();
        yield return new WaitForSeconds(boton.GetPressedAnimationDuration());

        //SceneManager.LoadScene(playSceneName);
    }
}
