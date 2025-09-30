using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour {
    [SerializeField] private ShopItemUI[] ShopItemUIElements;
    [SerializeField] public ShopItemsDisplay ShopItemDisplay;
    [SerializeField] private ShopTimer ShopTimer;
    private ShopNavigationService shopNavigationService;
    private ShopPurchaseService shopPurchaseService;
    public int GridRows = 2;
    public int GridColumns = 2;
    public int ShopDurationInSeconds = 10;
    private List<ShopSlotSelector> activeShopPlayerSelectors = new List<ShopSlotSelector>();

    private void Start() {
        ShopItemDisplay.SetShopItemUIElements(ShopItemUIElements);
        ShopItemDisplay.SetUpItems();
        shopPurchaseService = new ShopPurchaseService();
        shopNavigationService = new ShopNavigationService(GridRows, GridColumns);
        InitializePlayerSelectors();
        StartShopLogic();
        ShopTimer.OnTimerEnd += ResolvePurchases;
    }

    public void Reset() {
        ShopTimer.Reset();
        ShopTimer.StartTimer(ShopDurationInSeconds);
        foreach (ShopSlotSelector selector in activeShopPlayerSelectors) {
            selector.CanAct = true;
        }
    }

    public void UnlockAISelectors() {
        foreach (ShopSlotSelector selector in activeShopPlayerSelectors) {
            if (selector.Navigator is AIShopInputHandler) {
                selector.Unlock();
            }
        }
    }

    private void OnDestroy() {
        ShopTimer.OnTimerEnd -= ResolvePurchases;
        foreach (var playerSelector in activeShopPlayerSelectors) {
            playerSelector.OnSelectionChanged -= HandleSelectionChanged;
            playerSelector.OnLockChanged -= HandleLockChanged;
        }
    }

    private void InitializePlayerSelectors() {
        if (GameSessionManager.Instance == null) {
            Debug.LogError("Error: GameSessionManager.Instance is null");
            return;
        }

        activeShopPlayerSelectors.Clear();
        for (int i = 0; i < GameSessionManager.Instance.PlayerSlots.Length; i++) {
            PlayerSlot playerSlot = GameSessionManager.Instance.PlayerSlots[i];
            if (playerSlot.Navigator == null) continue;
            CreatePlayerShopSlotSelector(i, playerSlot);
        }
    }

    private void CreatePlayerShopSlotSelector(int playerIndex, PlayerSlot playerSlot) {
        var shopSlotSelector = CreateShopSlotSelectorGameObject(playerIndex, playerSlot);
        activeShopPlayerSelectors.Add(shopSlotSelector);
    }

    private ShopSlotSelector CreateShopSlotSelectorGameObject(int playerIndex, PlayerSlot playerSlot) {
        GameObject selectorGameObject = new GameObject("Player_"+playerIndex+"_ShopSlotSelector");
        selectorGameObject.transform.SetParent(transform);
        ShopSlotSelector shopSlotSelector = selectorGameObject.AddComponent<ShopSlotSelector>();
        shopSlotSelector.Initialize(playerIndex, playerSlot.Navigator, shopNavigationService);
        return shopSlotSelector;
    }

    public void StartShopLogic() {
        foreach (var playerSelector in activeShopPlayerSelectors) {
            playerSelector.OnSelectionChanged += HandleSelectionChanged;
            playerSelector.OnLockChanged += HandleLockChanged;
            HandleSelectionChanged(playerSelector, playerSelector.CurrentShopItemIndex);
        }

        ShopTimer.StartTimer(ShopDurationInSeconds);
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
        shopPurchaseService.ResolvePurchases(activeShopPlayerSelectors, ShopItemUIElements);
    }
}
