using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUITwoButtonInputHandler : MonoBehaviour, IDirectionalTwoButtonInputHandler {
    private PlayerInput playerInput;
    private InputAction navigateAction;
    private InputAction cancelAction;
    private InputAction selectAction;
    private bool selectIsPressed;
    private bool cancelIsPressed;

    public void Initialize(PlayerInput playerInput) {
        this.playerInput = playerInput;
        var actions = playerInput.currentActionMap;
        navigateAction = actions["Navigate"];
        cancelAction = actions["Cancel"];
        selectAction = actions["Submit"];
    }

    private void Update() {
        selectIsPressed = selectAction.WasPressedThisFrame();
        cancelIsPressed = cancelAction.WasPressedThisFrame();
    }

    public Vector2 GetNavigate() {
        return navigateAction.ReadValue<Vector2>();
    }

    public bool SelectIsPressed() {
        return selectIsPressed;
    }

    public bool CancelIsPressed() {
        return cancelIsPressed;
    }

    public bool IsActive() {
        return true;
    }
}
