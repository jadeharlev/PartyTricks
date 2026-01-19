using System.Collections.Generic;
using Services;
using UnityEngine;

public class BetPlayerManager {
    private BetCard[] betCards;
    private List<BetSelector> activeSelectors = new();
    private Dictionary<int, int> playerBets = new();
    private PlayerCornerDisplay[] playerCornerDisplays;
    private IPlayerService playerService;
    
    public BetPlayerManager(BetCard[] betCards, PlayerCornerDisplay[] playerCornerDisplays) {
        this.betCards = betCards;
        this.playerCornerDisplays = playerCornerDisplays;
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
    }

    public void InitializePlayers() {
        CreatePlayerSelectors();
        SubscribeToSelectorEvents();
        InitializePlayerBets();
        InitializePlayerCornerDisplays();
    }

    private void InitializePlayerCornerDisplays() {
        if (playerCornerDisplays == null || playerCornerDisplays.Length == 0) {
            Debug.LogWarning("No PlayerCornerDisplays assigned in BetPlayerManager.");
            return;
        }

        for (int i = 0; i < playerCornerDisplays.Length; i++) {
            PlayerProfile profile = playerService.GetPlayerProfile(i);
            if (profile != null) {
                playerCornerDisplays[i].Initialize(profile);
            }
        }
    }

    private void InitializePlayerBets() {
        playerBets.Clear();
        for (int i = 0; i < betCards.Length; i++) {
            playerBets[i] = 0;
            betCards[i].UpdateBetText(0);
            UpdateArrowVisibility(i);
        }
    }

    private void UpdateArrowVisibility(int playerIndex) {
        var selector = activeSelectors.Find(s => s.PlayerIndex == playerIndex);
        if (selector == null || selector.IsLocked) return;

        int currentBet = playerBets[playerIndex];
        int maxBetAllowed = GetMaxBet(selector.Profile);
        var card = betCards[playerIndex];
        
        if (currentBet <= 0) {
            card.HideDownArrow();
        } else {
            card.ShowDownArrow();
        }
        
        if (currentBet >= maxBetAllowed) {
            card.HideUpArrow();
        } else {
            card.ShowUpArrow();
        }
    }

    private int GetMaxBet(PlayerProfile profile) {
        int currentFunds = profile.Wallet.GetCurrentFunds();
        int maxFundsToBet = currentFunds;
        foreach (var itemDefinition in profile.Inventory.Items) {
            if (itemDefinition.Id == "increaseBettingAmounts") {
                maxFundsToBet = Mathf.RoundToInt(1.5f*maxFundsToBet);
            }
        }
        return Mathf.Max(maxFundsToBet, 50);
    }

    private void CreatePlayerSelectors() {
        activeSelectors.Clear();
        for (int i = 0; i < playerService.GetPlayerCount(); i++) {
            PlayerSlot slot = playerService.PlayerSlots[i];
            if (!slot.IsOccupied) continue;
            var selector = CreateSelector(i, slot);
            activeSelectors.Add(selector);
        }
    }

    private BetSelector CreateSelector(int index, PlayerSlot slot) {
        var selectorObject = new GameObject($"Player_{index}_BetSelector");
        var selector = selectorObject.AddComponent<BetSelector>();
        selector.Initialize(index, slot.InputHandler, slot.Profile);
        return selector;
    }

    private void SubscribeToSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged += HandleSelectionChanged;
            selector.OnLockChanged += HandleLockChanged;
        }
    }
    
    public void EnableAllSelectors() {
        UnsubscribeFromSelectorEvents();
        foreach (var selector in activeSelectors) {
            selector.CanAct = true;
        }
        SubscribeToSelectorEvents();
    }

    public void LockAllSelectors() {
        foreach (var selector in activeSelectors) {
            HandleLockChanged(selector, true);
        }
        UnsubscribeFromSelectorEvents();
    }
    
    private void UnsubscribeFromSelectorEvents() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
        }
    }
    
    public void UnlockAISelectors() {
        foreach (BetSelector selector in activeSelectors) {
            if (selector.Navigator is AIShopInputHandler) {
                selector.Unlock();
            }
        }
    }
    
    public List<BetSelector> GetSelectors() => activeSelectors;
    
    private void HandleLockChanged(BetSelector selector, bool locked) {
        int playerIndex = selector.PlayerIndex;
        var card = betCards[playerIndex];
        if(locked) {
            card.SwitchToLockedSprite();
            card.HideUpArrow();
            card.HideDownArrow();
        }
        else {
            card.SwitchToUnlockedSprite();
            UpdateArrowVisibility(playerIndex);
        }
    }

    private void HandleSelectionChanged(BetSelector selector, int betDelta) {
        int playerIndex = selector.PlayerIndex;
        int currentBet = playerBets[playerIndex];
        int maxBetAllowed = GetMaxBet(selector.Profile);
        
        int newBet = Mathf.Clamp(currentBet + betDelta, 0, maxBetAllowed);
        
        if(newBet == currentBet) return;
        
        playerBets[playerIndex] = newBet;
        betCards[playerIndex].UpdateBetText(newBet);
        UpdateArrowVisibility(playerIndex);
        bool isIncrease = newBet > currentBet;
        
        bool shouldFlash = (isIncrease && newBet < maxBetAllowed) || (!isIncrease && newBet > 0);
        if (shouldFlash) {
            betCards[playerIndex].FlashArrow(isIncrease);
        }
    }
    
    public Dictionary<int, int> GetPlayerBets() => playerBets;
    
    public void Cleanup() {
        foreach (var selector in activeSelectors) {
            selector.OnSelectionChanged -= HandleSelectionChanged;
            selector.OnLockChanged -= HandleLockChanged;
        }
        activeSelectors.Clear();
    }
}
