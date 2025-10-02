using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopPlayerManager {
    private ShopNavigationService navigationService;
    private ShopItemUI[] shopItemUIElements;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private List<ShopSlotSelector> activeSelectors = new();

    public ShopPlayerManager(ShopNavigationService navigationService, ShopItemUI[] shopItemUIElements, PlayerCornerDisplay[] playerCornerDisplays) {
        this.navigationService = navigationService;
        this.shopItemUIElements = shopItemUIElements;
        this.playerCornerDisplays = playerCornerDisplays;
    }

    public void InitializePlayers() {
        InitializeCornerDisplays();
        CreatePlayerSelectors();
        SubscribeToSelectorEvents();
    }

    private void InitializeCornerDisplays() {
        for (int i = 0; i < playerCornerDisplays.Length; i++) {
            var profile = GameSessionManager.Instance.PlayerSlots[i].Profile;
            playerCornerDisplays[i].Initialize(profile);
        }
    }

    private void CreatePlayerSelectors() {
        activeSelectors.Clear();
        for (int i = 0; i < GameSessionManager.Instance.PlayerSlots.Length; i++) {
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot.Navigator == null) continue;
            var selector = CreateSelector(i, slot);
            activeSelectors.Add(selector);
        }
    }

    private ShopSlotSelector CreateSelector(int index, PlayerSlot slot) {
        var selectorObject = new GameObject($"Player_{index}_ShopSlotSelector");
        var selector = selectorObject.AddComponent<ShopSlotSelector>();
        selector.Initialize(index, slot.Navigator, navigationService, slot.Profile);
        return selector;
    }

    private void SubscribeToSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged += HandleSelectionChanged;
            selector.OnLockChanged += HandleLockChanged;
            HandleSelectionChanged(selector, selector.CurrentShopItemIndex);
        }
    }

    private void HandleLockChanged(ShopSlotSelector selector, bool locked) {
        shopItemUIElements[selector.CurrentShopItemIndex].OnPointedTo(selector.PlayerIndex, true, locked);
    }

    private void HandleSelectionChanged(ShopSlotSelector selector, int newIndex) {
        for (int i = 0; i < shopItemUIElements.Length; i++) {
            bool isSelected = (i == newIndex);
            shopItemUIElements[i].OnPointedTo(selector.PlayerIndex, isSelected, selector.IsLocked);
        }
    }

    public void EnableAllSelectors() {
        UnsubscribeFromSelectorEvents();
        foreach (var selector in activeSelectors) {
            selector.CanAct = true;
        }
		SubscribeToSelectorEvents();
    }
    
    private void UnsubscribeFromSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
        }
    }

    public void UnlockAISelectors() {
        foreach (ShopSlotSelector selector in activeSelectors) {
            if (selector.Navigator is AIShopInputHandler) {
                selector.Unlock();
            }
        }
    }

    public List<ShopSlotSelector> GetSelectors() => activeSelectors;

    public void Cleanup() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
        }
        activeSelectors.Clear();
    }
}