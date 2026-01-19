using System.Collections.Generic;
using Services;
using UnityEngine;

public class ShopPlayerManager {
    private ShopNavigationService navigationService;
    private ShopItemUI[] shopItemUIElements;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private List<ShopSlotSelector> activeSelectors = new();
    private IPlayerService playerService;

    public ShopPlayerManager(ShopNavigationService navigationService, ShopItemUI[] shopItemUIElements, PlayerCornerDisplay[] playerCornerDisplays) {
        this.navigationService = navigationService;
        this.shopItemUIElements = shopItemUIElements;
        this.playerCornerDisplays = playerCornerDisplays;
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
    }

    public void InitializePlayers() {
        InitializeCornerDisplays();
        CreatePlayerSelectors();
        SubscribeToSelectorEvents();
    }

    private void InitializeCornerDisplays() {
        for (int i = 0; i < playerCornerDisplays.Length; i++) {
            var profile = playerService.PlayerSlots[i].Profile;
            playerCornerDisplays[i].Initialize(profile);
        }
    }

    private void CreatePlayerSelectors() {
        activeSelectors.Clear();
        for (int i = 0; i < playerService.GetPlayerCount(); i++) {
            PlayerSlot slot = playerService.PlayerSlots[i];
            if (slot.InputHandler == null) continue;
            var selector = CreateSelector(i, slot);
            activeSelectors.Add(selector);
        }
    }

    private ShopSlotSelector CreateSelector(int index, PlayerSlot slot) {
        var selectorObject = new GameObject($"Player_{index}_ShopSlotSelector");
        var selector = selectorObject.AddComponent<ShopSlotSelector>();
        selector.Initialize(index, slot.InputHandler, navigationService, slot.Profile);
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

    public void DisableAllSelectors() {
        foreach (var selector in activeSelectors) {
            selector.CanAct = false;
        }
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