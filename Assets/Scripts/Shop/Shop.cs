using System;
using System.Collections;
using System.Collections.Generic;
using Services;
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
    private IGameFlowService gameFlowService;
    private IPauseService pauseService;
    public int GridRows = 2;
    public int GridColumns = 2;
    public int ShopDurationInSeconds = 10;

    private void Awake() {
        gameFlowService = GlobalServiceLocator.Instance.GetService<IGameFlowService>();
        pauseService = GlobalServiceLocator.Instance.GetService<IPauseService>();
        if (pauseService != null) {
            pauseService.OnPause += OnPause;
            pauseService.OnUnpause += OnUnpause;
        }
    }
    
    private void OnPause() => playerManager?.DisableAllSelectors();
    private void OnUnpause() => playerManager?.EnableAllSelectors();

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
        if (gameFlowService != null) {
            StartCoroutine(WaitAndThenMoveToNextMinigame());
        }
        else {
            Debug.LogError($"Shop: GameFlowManager is missing!");
        }
    }

    private IEnumerator WaitAndThenMoveToNextMinigame() {
        int numberOfSecondsToWait = 5;
        yield return new WaitForSeconds(numberOfSecondsToWait);
        gameFlowService.OnShopEnd();
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
        if (pauseService != null) {
            pauseService.OnPause -= OnPause;
            pauseService.OnUnpause -= OnUnpause;
        }
    }
}
