using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour {
    public Mode mode = Mode.UI;
    private static bool initialized = false;

    public enum Mode {
        Gameplay,
        UI
    }
    
    private void Awake() {
        if (initialized) return;
        initialized = true;
        if (this.mode == Mode.UI) {
            InputSystem.actions.FindActionMap("Player").Disable();
            InputSystem.actions.FindActionMap("UI").Enable();
        }
        else {
            InputSystem.actions.FindActionMap("UI").Disable();
            InputSystem.actions.FindActionMap("Player").Enable();
        }
    }
}
