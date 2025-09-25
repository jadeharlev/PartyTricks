using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour {
    [SerializeField] private ShopItemUI[] ShopItemUIElements;
    [SerializeField] public ShopItemsDisplay ShopItemDisplay;
    public int ShopDurationInSeconds = 10;
    private List<ShopSlotSelector> activeShopPlayerSelectors = new List<ShopSlotSelector>();
    private int timeRemaining;
    void Start() {
        ShopItemDisplay.SetShopItemUIElements(ShopItemUIElements);
        ShopItemDisplay.SetUpItems();
        InitializePlayerSelectors();
        StartShopLogic();
    }

    private void InitializePlayerSelectors() {
        if (GameSessionManager.Instance == null) {
            Debug.LogError("Error: GameSessionManager.Instance is null");
            return;
        }

        activeShopPlayerSelectors.Clear();
        for (int i = 0; i < GameSessionManager.Instance.PlayerSlots.Length; i++) {
            PlayerSlot playerSlot = GameSessionManager.Instance.PlayerSlots[i];
            if (playerSlot.Navigator != null) {
                GameObject selectorGameObject = new GameObject("Player_"+i+"_ShopSlotSelector");
                selectorGameObject.transform.SetParent(transform);
                ShopSlotSelector shopSlotSelector = selectorGameObject.AddComponent<ShopSlotSelector>();
                shopSlotSelector.Initialize(i, playerSlot.Navigator);
                activeShopPlayerSelectors.Add(shopSlotSelector);
                shopSlotSelector.GridColumns = 2;
                shopSlotSelector.GridRows = 2;
            }
        }
    }

    public void StartShopLogic() {
        foreach (var playerSelector in activeShopPlayerSelectors) {
            playerSelector.OnSelectionChanged += HandleSelectionChanged;
            playerSelector.OnLockChanged += HandleLockChanged;
            HandleSelectionChanged(playerSelector, playerSelector.CurrentShopItemIndex);
        }

        StartCoroutine(ShopCountdown());
    }

    private IEnumerator ShopCountdown() {
        timeRemaining = ShopDurationInSeconds;
        while (timeRemaining > 0) {
            Debug.Log("Shop time remaining: " + timeRemaining + " seconds");
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        ResolvePurchases();
    }

    private void HandleSelectionChanged(ShopSlotSelector slotSelector, int newIndex) {
        for (int i = 0; i < ShopItemUIElements.Length; i++) {
            bool isCurrentlySelected = (i == newIndex);
            ShopItemUIElements[i].OnPointedTo(slotSelector.PlayerIndex, isCurrentlySelected, slotSelector.IsLocked);
        }
    }

    private void HandleLockChanged(ShopSlotSelector slotSelector, bool locked) {
        ShopItemUIElements[slotSelector.CurrentShopItemIndex].OnPointedTo(slotSelector.PlayerIndex, true, locked);
    }

    private void ResolvePurchases() {
        Debug.Log("Shop closed! Resolving purchases.");
        foreach (var playerSelector in activeShopPlayerSelectors) {
            int index = playerSelector.CurrentShopItemIndex;
            playerSelector.Lock();
            playerSelector.CanAct = false;
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " buys item " + index);
        }
    }
}
