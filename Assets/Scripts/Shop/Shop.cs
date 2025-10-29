using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Shop : MonoBehaviour {
    [SerializeField] private ShopItemUI[] ShopItemUIElements;
    [SerializeField] private PlayerCornerDisplay[] PlayerCornerDisplays;
    [SerializeField] public ShopItemsDisplay ShopItemDisplay;
    [FormerlySerializedAs("ShopTimer")] [SerializeField] private CountdownTimer CountdownTimer;
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
        CountdownTimer.OnTimerEnd += OnShopTimerEnd;
    }

    private void StartShop() {
        playerManager.InitializePlayers();
        CountdownTimer.StartTimer(ShopDurationInSeconds);
    }
    
    private void OnShopTimerEnd() {
        shopPurchaseService.ResolvePurchases(playerManager.GetSelectors(), ShopItemUIElements);
        if (GameFlowManager.Instance != null) {
            StartCoroutine(WaitAndThenMoveToNextMinigame());
        }
        else {
            Debug.LogError($"Shop: GameFlowManager is missing!");
        }
    }

    private IEnumerator WaitAndThenMoveToNextMinigame() {
        int numberOfSecondsToWait = 5;
        yield return new WaitForSeconds(numberOfSecondsToWait);
        GameFlowManager.Instance.OnShopEnd();
    }

    public void Reset() {
        CountdownTimer.Reset();
        CountdownTimer.StartTimer(ShopDurationInSeconds);
        playerManager.EnableAllSelectors();
    }

    public void UnlockAISelectors() {
        playerManager.UnlockAISelectors();
    }
    

    private void OnDestroy() {
        CountdownTimer.OnTimerEnd -= OnShopTimerEnd;
        playerManager?.Cleanup();
    }
}
