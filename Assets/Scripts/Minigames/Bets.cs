using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bets : MonoBehaviour {
    [SerializeField] 
    private BetCard[] betCards;
    private BetPlayerManager playerManager;
    public int AllowedBettingDurationInSeconds = 10;
    [SerializeField] 
    private CountdownTimer CountdownTimer;
    private IGamblingMinigame gamblingMinigameManager;

    private void Awake() {
        gamblingMinigameManager = GetComponent<IGamblingMinigame>();
        if (gamblingMinigameManager == null) {
            Debug.LogError("Error: IGamblingMinigame not found on Bets GameObject");
        }
    }

    private void Start() {
        StartCoroutine(WaitForManagerAndStart());
    }

    private IEnumerator WaitForManagerAndStart() {
        yield return new WaitUntil(() => gamblingMinigameManager != null);
        InitializeComponents();
        StartBets();
    }

    private void InitializeComponents() {
        playerManager = new BetPlayerManager(betCards);
        CountdownTimer.OnTimerEnd += OnBetTimerEnd;
    }

    private void StartBets() {
        playerManager.InitializePlayers();
        CountdownTimer.StartTimer(AllowedBettingDurationInSeconds);
    }

    private void OnBetTimerEnd() {
        Debug.Log("Timer over!");
        playerManager.LockAllSelectors();

        Dictionary<int, int> bets = playerManager.GetPlayerBets();

        if (gamblingMinigameManager != null && gamblingMinigameManager.OnBetTimerEnd != null) {
            gamblingMinigameManager.OnBetTimerEnd?.Invoke(bets);
        }
        else {
            Debug.LogError("OnBetTimerEnd event is not set up correctly.");
        }
    }

    public void Reset() {
        CountdownTimer.Reset();
        CountdownTimer.StartTimer(AllowedBettingDurationInSeconds);
        playerManager.EnableAllSelectors();
    }
    
    public void UnlockAISelectors() {
        playerManager.UnlockAISelectors();
    }
    

    private void OnDestroy() {
        if (CountdownTimer != null) {
            CountdownTimer.OnTimerEnd -= OnBetTimerEnd;
        }
        playerManager?.Cleanup();
    }
}
