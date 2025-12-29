using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    public static Vector2 movement;
    public static bool jumpPressed;
    public static bool jumpIsHeld;
    public static bool jumpWasReleased;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction runAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
    }

    private void Update()
    {
        movement = moveAction.ReadValue<Vector2>();
        jumpPressed = jumpAction.WasPressedThisFrame();
        jumpIsHeld = jumpAction.IsPressed();
        jumpWasReleased = jumpAction.WasReleasedThisFrame();

        // debug
        Debug.Log("Move: " + movement + " | JumpPressed: " + jumpPressed);
    }
}
