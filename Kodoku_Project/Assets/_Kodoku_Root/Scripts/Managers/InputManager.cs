using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    // Movimiento
    public static Vector2 movement;

    // Salto
    public static bool jumpPressed;
    public static bool jumpIsHeld;
    public static bool jumpWasReleased;

    // Correr
    public static bool runIsHeld;

    // NUEVO - Ataque
    public static bool attackPressed;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction runAction;
    InputAction attackAction; // NUEVO

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"]; // NUEVO
    }

    private void Update()
    {
        // Movimiento
        movement = moveAction.ReadValue<Vector2>();

        // Salto
        jumpPressed = jumpAction.WasPressedThisFrame();
        jumpIsHeld = jumpAction.IsPressed();
        jumpWasReleased = jumpAction.WasReleasedThisFrame();

        // NUEVO - Ataque
        attackPressed = attackAction.WasPressedThisFrame();
    }
}