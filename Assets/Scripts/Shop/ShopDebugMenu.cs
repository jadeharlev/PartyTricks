using UnityEngine;
using UnityEngine.InputSystem;

public class ShopDebugMenu : MonoBehaviour
{
    public ShopItemsDisplay Display;
    public GameObject DebugMenu;
    private InputAction toggleDebugMenuAction;
    bool lastDebugMenuActiveState = false;

    private void Awake() {
        toggleDebugMenuAction = InputSystem.actions.FindAction("UI/ToggleDebugMenu");
    }

    private void Update() {
        if (toggleDebugMenuAction.WasPressedThisFrame()) {
            lastDebugMenuActiveState = !lastDebugMenuActiveState;
            DebugMenu.SetActive(lastDebugMenuActiveState);
        }
    }

    public void RefreshShop() {
        Display.Reset();
        Display.SetUpItems();
    }
}
