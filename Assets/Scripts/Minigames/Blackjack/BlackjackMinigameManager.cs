using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackMinigameManager : MonoBehaviour, IGamblingMinigame {
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    [SerializeField] private bool isDoubleRound = false;
    public bool IsDoubleRound => isDoubleRound;
    public bool HasBeenInitialized { get; private set; } = false;
    
    [Header("Blackjack Settings")]
    [SerializeField] private int dealerDrawThreshold = 17;

    [SerializeField] private float winPayoutMultiplier = 2f;
    [SerializeField] private float pushPayoutMultiplier = 1f;
    [SerializeField] private float losePayoutMultiplier = 0f;
    // [SerializeField] private BlackjackBettingUIController bettingUIController;
    
    private BlackjackShoe shoe;
    private int currentPlayerIndex = 0;
    private List<BlackjackPlayer> players;
    private BlackjackPlayer dealer;
    // private BlackjackTestUIController uiController;
    private int[] playerBets;

    private enum GamePhase {
        Betting,
        PlayerTurns,
        DealerTurn,
        Ended
    }
    
    private GamePhase currentPhase = GamePhase.Betting;

    private void Awake() {
        shoe = new BlackjackShoe();
        dealer = new BlackjackPlayer();
        players = new List<BlackjackPlayer>();
        
        for (int i = 0; i < 4; i++) {
            players.Add(new BlackjackPlayer());
        }

        // uiController = FindFirstObjectByType<BlackjackTestUIController>();

        // if (bettingUIController == null) {
            // bettingUIController = FindFirstObjectByType<BlackjackBettingUIController>();
        // }
    }

    private void Start() {
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization() {
        while (!HasBeenInitialized) {
            yield return null;
        }

        StartBettingPhase();
    }
    
    public void Initialize(bool isDoubleRound) {
        this.isDoubleRound = isDoubleRound;
        HasBeenInitialized = true;
        DebugLogger.Log(LogChannel.Systems, $"Blackjack initiated. Double round: {isDoubleRound}");
    }

    private void StartBettingPhase() {
        currentPhase = GamePhase.Betting;
        // if (bettingUIController != null) {
        //     bettingUIController.Initialize(this);
        //     bettingUIController.OnBettingComplete += OnBettingComplete;
        // }
        // else {
        //     Debug.LogError("BettingUIController not found: skipping betting phase.");
        playerBets = new int[] { 50, 50, 50, 50 };
        OnBettingComplete(playerBets);
        // }
    }

    public void SetPlayerBets(int[] bets) {
        playerBets = bets;
        DebugLogger.Log(LogChannel.Systems, $"Best placed: [{string.Join(", ", bets)}]");
    }

    private void OnBettingComplete(int[] bets) {
        // if (bettingUIController != null) {
        //     bettingUIController.OnBettingComplete -= OnBettingComplete;
        // }

        StartGame();
    }

    private void StartGame() {
        DealInitialCards();
        currentPhase = GamePhase.PlayerTurns;
        // if (uiController != null) {
        //     uiController.UpdateUI();
        // }
    }

    private void DealInitialCards() {
        foreach (var player in players) {
            player.DrawCard(shoe.DrawCard());
            player.DrawCard(shoe.DrawCard());
        }
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to all players.");
        
        dealer.DrawCard(shoe.DrawCard());
        dealer.DrawCard(shoe.DrawCard());
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to dealer.");
        
    }

    public void PlayerHit() {
        if (currentPhase != GamePhase.PlayerTurns) return;
        BlackjackPlayer currentPlayer = players[currentPlayerIndex];
        currentPlayer.DrawCard(shoe.DrawCard());
        DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} hit. New value is {currentPlayer.GetBestValue()}");
        if (currentPlayer.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} busted!");
            StartNextPlayerTurn();
        }
    }

    public void PlayerStand() {
        if (currentPhase != GamePhase.PlayerTurns) return;
        DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} stands with a {players[currentPlayerIndex].GetBestValue()}");
        StartNextPlayerTurn();
    }

    private void StartNextPlayerTurn() {
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count) {
            currentPhase = GamePhase.DealerTurn;
            StartCoroutine(PlayDealerTurnWithDelay());
        }
        else {
            DebugLogger.Log(LogChannel.Systems, $"Moving to player {currentPlayerIndex}'s turn");
            // if (uiController != null) {
            //     uiController.EnableButtons();
            //     uiController.UpdateUI();
            // }
        }
    }

    private IEnumerator PlayDealerTurnWithDelay() {
        yield return new WaitForSeconds(0.5f);
        DebugLogger.Log(LogChannel.Systems, "Started dealer's turn.");
        while (dealer.GetBestValue() < dealerDrawThreshold) {
            dealer.DrawCard(shoe.DrawCard());
            DebugLogger.Log(LogChannel.Systems, $"Dealer drew. New value is {dealer.GetBestValue()}");
            // if (uiController != null) {
            //     uiController.UpdateUI();
            // }
            yield return new WaitForSeconds(1f);
        }
        
        DebugLogger.Log(LogChannel.Systems, $"Dealer stands at {dealer.GetBestValue()}; Busted is {dealer.HasBusted()}");

        currentPhase = GamePhase.Ended;
        yield return new WaitForSeconds(1f);
        EvaluateWinners();

    }


    private void EvaluateWinners() {
        var results = new PlayerMinigameResult[4];
        int dealerValue = dealer.GetBestValue();
        if (dealer.HasBusted()) dealerValue = 0;

        for (int i = 0; i < players.Count; i++) {
            var player = players[i];
            int playerValue = player.GetBestValue();
            if (player.HasBusted()) playerValue = 0;
            int bet = playerBets[i];

            float payoutMultiplier;
            int rank;

            if (player.HasBusted()) {
                payoutMultiplier = losePayoutMultiplier;
                rank = 3;
                DebugLogger.Log(LogChannel.Systems,
                    $"Player {i}: Busted ({player.GetBestValue()} vs dealer {dealerValue})");
            }
            else if (dealer.HasBusted() || playerValue > dealerValue) {
                payoutMultiplier = winPayoutMultiplier;
                rank = 0;
                DebugLogger.Log(LogChannel.Systems, $"Player {i}: Won ({playerValue} vs dealer {dealerValue})");
            }
            else if (playerValue == dealerValue) {
                payoutMultiplier = pushPayoutMultiplier;
                rank = 1;
                DebugLogger.Log(LogChannel.Systems, $"Player {i}: Pushed ({playerValue} vs dealer {dealerValue})");
            }  else {
                payoutMultiplier = losePayoutMultiplier;
                rank = 2;
                DebugLogger.Log(LogChannel.Systems, $"Player {i}: Lost ({playerValue} vs dealer {dealerValue})");
            }
            
            int basePayout = Mathf.RoundToInt(bet * payoutMultiplier);
            int finalPayout = basePayout;
            if (isDoubleRound) {
                finalPayout *= 2;
                DebugLogger.Log(LogChannel.Systems, $"Double round: {basePayout} -> {finalPayout}");
            }
            
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Bet {bet}, Payout {finalPayout}, Net {finalPayout-bet}");
            results[i] = new PlayerMinigameResult(i, rank, finalPayout, bet);
        }
        DebugLogger.Log(LogChannel.Systems, "Blackjack game complete.");
        OnMinigameFinished?.Invoke(results);
    }

    public BlackjackPlayer GetCurrentPlayer() {
        if (currentPhase != GamePhase.PlayerTurns || currentPlayerIndex >= players.Count) {
            return null;
        }
        return players[currentPlayerIndex];
    }

    public BlackjackPlayer GetDealer() {
        return dealer;
    }
}