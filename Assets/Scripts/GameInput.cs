using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputActions inputActions;
    private InputAction shootAction;

    public event EventHandler OnMenuButtonPressed;

    public void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Enable();

        inputActions.Player.Menu.performed += Menu_performed;

        // GET SHOOT ACTION FROM InputActions (not PlayerInputActions)
        shootAction = inputActions.Player.Shoot;
    }

    private void Menu_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnMenuButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }

    public bool IsUpActionPressed()
    {
        return inputActions.Player.LanderUp.IsPressed();
    }

    public bool IsLeftActionPressed()
    {
        return inputActions.Player.LanderLeft.IsPressed();
    }

    public bool IsRightActionPressed()
    {
        return inputActions.Player.LanderRight.IsPressed();
    }

    public bool IsShootPressed()
    {
        return shootAction.IsPressed();
    }
}