using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlotManager {
    private PlayerSlot[] playerSlots;

    public PlayerSlotManager(PlayerSlot[] playerSlots) {
        this.playerSlots = playerSlots;
        InitializeSlots();
    }

    private void InitializeSlots() {
        for (int i = 0; i < playerSlots.Length; i++) {
            if (playerSlots[i] == null) {
                Debug.LogError("GameSessionManager: PlayerSlot at index " + i + " is not assigned in the inspector.");
                continue;
            }
            SetUpAIShopInput(i);
        }
    }
    
    private void SetUpAIShopInput(int playerSlotIndex) {
        var aiGameObject = new GameObject("AIInput_P" + playerSlotIndex);
        aiGameObject.transform.SetParent(playerSlots[playerSlotIndex].transform);
        var aiHandler = aiGameObject.AddComponent<AIShopInputHandler>();
        playerSlots[playerSlotIndex].Initialize(playerSlotIndex, aiHandler, true);
    }
    
    public void AssignPlayer(PlayerInput playerInput) {
        Debug.Log("Player joined: " + playerInput.playerIndex);
        int slotIndex = FindFirstAI();
        if (slotIndex == -1) {
            Debug.Log("All slots are already human!!");
            Object.Destroy(playerInput.gameObject);
            return;
        }

        playerInput.transform.SetParent(playerSlots[slotIndex].transform);
        var shopHandler = playerInput.gameObject.AddComponent<PlayerShopInputHandler>();
        shopHandler.Initialize(playerInput);
        playerSlots[slotIndex].ReplaceShopNavigator(shopHandler, false);
    }
    
    private int FindFirstAI() {
        for (int i = 0; i < playerSlots.Length; i++) {
            if (playerSlots[i].Navigator is AIShopInputHandler) {
                return i;
            }
        }

        return -1;
    }
}