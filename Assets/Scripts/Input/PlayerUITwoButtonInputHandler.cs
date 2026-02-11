using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUITwoButtonInputHandler : MonoBehaviour, IDirectionalTwoButtonInputHandler {
    private PlayerInput playerInput;
    private InputAction navigateAction;
    private InputAction cancelAction;
    private InputAction selectAction;
    private InputAction chargeAction;
    private bool selectIsPressed;
    private bool cancelIsPressed;
    private bool chargeIsPressed;

    public void Initialize(PlayerInput playerInput) {
        this.playerInput = playerInput;
        var actions = playerInput.currentActionMap;
        navigateAction = actions["Navigate"];
        cancelAction = actions["Cancel"];
        selectAction = actions["Submit"];
        chargeAction = actions["Charge"];
    }

    private void Update() {
        selectIsPressed = selectAction.WasPressedThisFrame();
        cancelIsPressed = cancelAction.WasPressedThisFrame();
        chargeIsPressed =  chargeAction.WasPressedThisFrame();
        if (chargeIsPressed) {
            Debug.Log($"Charge is pressed.");
        }
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
    
    public bool ChargeIsPressed() {
        return chargeAction.WasPressedThisFrame();
    }

    public bool ChargeIsHeld() {
        return chargeAction.IsPressed();
    }

    public bool ChargeIsReleased() {
        return chargeAction.WasReleasedThisFrame();
    }
}
