using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour {
    [SerializeField] private ShopItemUI[] ShopItemUIElements;
    [SerializeField] private PlayerCornerDisplay[] PlayerCornerDisplays;
    [SerializeField] public ShopItemsDisplay ShopItemDisplay;
    [SerializeField] private ShopTimer ShopTimer;
    private ShopPlayerManager playerManager;
    private ShopNavigationService shopNavigationService;
    private ShopPurchaseService shopPurchaseService;
    public int GridRows = 2;
    public int GridColumns = 2;
    public int ShopDurationInSeconds = 10;

    private void Start() {
        InitializeComponents();
        StartShop();
    }

    private void InitializeComponents() {
        ShopItemDisplay.SetShopItemUIElements(ShopItemUIElements);
        ShopItemDisplay.SetUpItems();
        shopPurchaseService = new ShopPurchaseService();
        shopNavigationService = new ShopNavigationService(GridRows, GridColumns);
        playerManager = new ShopPlayerManager(shopNavigationService, ShopItemUIElements, PlayerCornerDisplays);
        ShopTimer.OnTimerEnd += OnShopTimerEnd;
    }

    private void StartShop() {
        playerManager.InitializePlayers();
        ShopTimer.StartTimer(ShopDurationInSeconds);
    }
    
    private void OnShopTimerEnd() {
        shopPurchaseService.ResolvePurchases(playerManager.GetSelectors(), ShopItemUIElements);
    }

    public void Reset() {
        ShopTimer.Reset();
        ShopTimer.StartTimer(ShopDurationInSeconds);
        playerManager.EnableAllSelectors();
    }

    public void UnlockAISelectors() {
        playerManager.UnlockAISelectors();
    }
    

    private void OnDestroy() {
        ShopTimer.OnTimerEnd -= OnShopTimerEnd;
        playerManager?.Cleanup();
    }
}
