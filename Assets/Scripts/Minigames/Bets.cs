using System;
using UnityEngine;

public class Bets : MonoBehaviour {
    [SerializeField] private BetCard[] betCards;
    private BetPlayerManager playerManager;
    public int AllowedBettingDurationInSeconds = 10;
    [SerializeField] private CountdownTimer CountdownTimer;
    private void Start() {
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
        CountdownTimer.OnTimerEnd -= OnBetTimerEnd;
        playerManager?.Cleanup();
    }
}
