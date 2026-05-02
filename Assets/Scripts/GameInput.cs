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
        inputActions.Player.Enable();

        inputActions.Player.Menu.performed += Menu_performed;

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

    // UI Navigation - these are now in Player map
    public Vector2 GetNavigationInput()
    {
        return inputActions.Player.Navigate.ReadValue<Vector2>();
    }

    public bool IsSubmitPressed()
    {
        return inputActions.Player.Submit.triggered;
    }

    public bool IsCancelPressed()
    {
        return inputActions.Player.Cancel.triggered;
    }

    public void DisableSubmitAction()
    {
        inputActions.Player.Submit.Disable();
    }

    public void EnableSubmitAction()
    {
        inputActions.Player.Submit.Enable();
    }
}