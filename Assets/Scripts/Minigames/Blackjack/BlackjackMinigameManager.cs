using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackMinigameManager : MonoBehaviour, IGamblingMinigame {
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public Action<Dictionary<int, int>> OnBetTimerEnd { get; set; }
    
    [SerializeField] private bool isDoubleRound = false;
    public bool IsDoubleRound => isDoubleRound;
    public bool HasBeenInitialized { get; private set; } = false;
    
    [Header("Blackjack Settings")]
    [SerializeField] private int dealerDrawThreshold = 17;

    [SerializeField] private float winPayoutMultiplier = 2f;
    [SerializeField] private float pushPayoutMultiplier = 1f;
    [SerializeField] private float losePayoutMultiplier = 0f;
    [SerializeField] private float aiDecisionDelayInSeconds = 1.5f;
    [SerializeField] private float resultsDisplayDurationInSeconds = 3f;
    

    [Header("Game Objects")] 
    [SerializeField]
    private BlackjackPlayerDisplay[] playerDisplays;
    [SerializeField]
    private BlackjackDealerDisplay dealerDisplay;
    [SerializeField]
    private Bets bettingSystem;
    [SerializeField] 
    private GameObject blackjackPanel;
    
    private BlackjackShoe shoe;
    private int currentPlayerIndex = 0;
    private List<BlackjackPlayer> players;
    private BlackjackPlayer dealer;
    private int[] playerBets;
    private List<BlackjackPlayerController> playerControllers;

    private static readonly Color[] PlayerColors =
    {
        Color.darkRed,
        Color.darkBlue,
        Color.rebeccaPurple,
        Color.darkGreen
    };
    
    private static readonly Color InactiveColor = new Color(0.3f, 0.3f, 0.3f);
    

    private enum GamePhase {
        Betting,
        InitialDeal,
        PlayerTurns,
        DealerTurn,
        Results,
        Ended
    }
    
    private GamePhase currentPhase = GamePhase.Betting;

    private void Awake() {
        shoe = new BlackjackShoe();
        dealer = new BlackjackPlayer();
        players = new List<BlackjackPlayer>();
        playerControllers = new List<BlackjackPlayerController>();
        
        for (int i = 0; i < 4; i++) {
            players.Add(new BlackjackPlayer());
        }

        if (blackjackPanel != null) {
            blackjackPanel.SetActive(false);
        }
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
        if (bettingSystem != null) {
            OnBetTimerEnd += OnBettingComplete;
        }
        else {
            Debug.LogError("BlackjackMinigameManager: bettingSystem has not been assigned.");
            return;
        }
        bettingSystem.gameObject.SetActive(true);
    }

    public void SetPlayerBets(int[] bets) {
        playerBets = bets;
        DebugLogger.Log(LogChannel.Systems, $"Best placed: [{string.Join(", ", bets)}]");
    }

    private void OnBettingComplete(Dictionary<int, int> bets) {
        OnBetTimerEnd -= OnBettingComplete;
        playerBets = new int[4];
        for (int i = 0; i < 4; i++) {
            playerBets[i] = bets.GetValueOrDefault(i, 50);
        }
        DebugLogger.Log(LogChannel.Systems, $"Betting complete, starting game.");

        if (blackjackPanel != null) {
            blackjackPanel.SetActive(true);
        }

        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence() {
        InitializePlayerControllers();
        InitializePlayerDisplays();

        yield return new WaitForSeconds(0.5f);

        currentPhase = GamePhase.InitialDeal;
        yield return StartCoroutine(DealInitialCards());

        currentPhase = GamePhase.PlayerTurns;
        StartPlayerTurn(0);
    }
    private void InitializePlayerControllers() {
        playerControllers.Clear();

        for (int i = 0; i < 4; i++) {
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot == null || slot.Navigator == null) continue;

            GameObject controllerObject = new GameObject("Player_{i}_BlackjackController");
            controllerObject.transform.SetParent(transform);
            
            BlackjackPlayerController controller = controllerObject.AddComponent<BlackjackPlayerController>();
            controller.Initialize(i, slot.Navigator, slot.IsAI);
            controller.OnHit += HandlePlayerHit;
            controller.OnStand += HandlePlayerStand;
            
            playerControllers.Add(controller);
        }
    }

    private void InitializePlayerDisplays() {
        for (int i = 0; i < playerDisplays.Length; i++) {
            playerDisplays[i].UpdatePlayerNumberDisplay(i);
            
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[i];
            int funds = slot?.Profile?.Wallet.GetCurrentFunds() ?? 0;
            playerDisplays[i].UpdateBetLabel(playerBets[i], funds);
            playerDisplays[i].ChangeToPlayerColor(InactiveColor);
            playerDisplays[i].HideControls();
        }
        
        dealerDisplay.UpdateCardValueLabel(0, 0);
    }
    
    private IEnumerator DealInitialCards() {
        Card card;
        for (var i = 0; i<players.Count; i++) {
            var player = players[i];
            var playerDisplay = playerDisplays[i];
            
            card = shoe.DrawCard();
            player.DrawCard(card);
            playerDisplay.AddCard(card);
            UpdatePlayerDisplay(i);
            yield return new WaitForSeconds(0.3f);
            
            card = shoe.DrawCard();
            player.DrawCard(card);
            playerDisplay.AddCard(card);
            UpdatePlayerDisplay(i);
            yield return new WaitForSeconds(0.3f);
        }
        
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to all players.");

        card = shoe.DrawCard();
        dealer.DrawCard(card);
        dealerDisplay.AddCard(card);
        yield return new WaitForSeconds(0.3f);
        
        card = shoe.DrawCard();
        dealer.DrawCard(card);
        dealerDisplay.AddCard(card);
        yield return new WaitForSeconds(0.3f);
        
        dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(), dealer.GetHighValue());
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to dealer.");
    }

    private void StartPlayerTurn(int playerIndex) {
        currentPlayerIndex = playerIndex;
        if (currentPlayerIndex >= players.Count) {
            currentPhase = GamePhase.DealerTurn;
            StartCoroutine(PlayDealerTurn());
            return;
        }
        
        BlackjackPlayer player = players[currentPlayerIndex];

        if (player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} already busted; skipping turn.");
            EndPlayerTurn(currentPlayerIndex);
            return;
        }
        
        playerDisplays[currentPlayerIndex].ChangeToPlayerColor(PlayerColors[currentPlayerIndex]);
        playerDisplays[currentPlayerIndex].ShowControls();

        BlackjackPlayerController controller = playerControllers.Find(c => c.PlayerIndex == currentPlayerIndex);
        if (controller != null) {
            controller.EnableInput();

            if (controller.IsAI) {
                StartCoroutine(HandleAITurn(currentPlayerIndex));
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, $"Started turn for player {currentPlayerIndex}. IsAI: {controller.IsAI}");
    }

    private IEnumerator HandleAITurn(int playerIndex) {
        yield return new WaitForSeconds(aiDecisionDelayInSeconds);
        BlackjackPlayer player = players[playerIndex];
        while(!player.HasBusted() && player.GetBestValue() < 17) {
            DebugLogger.Log(LogChannel.Systems, $"AI Player {playerIndex} chose to hit.");
            HandlePlayerHit(playerIndex);
        }

        if (!player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"AI Player {playerIndex} chose to stand.");
            HandlePlayerStand(playerIndex);
        }
    }

    public void HandlePlayerHit(int playerIndex) {
        if (currentPhase != GamePhase.PlayerTurns || playerIndex != currentPlayerIndex) return;
        BlackjackPlayer player = players[currentPlayerIndex];
        Card newCard = shoe.DrawCard();
        player.DrawCard(newCard);
        
        playerDisplays[playerIndex].AddCard(newCard);
        UpdatePlayerDisplay(playerIndex);
        DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} hit. New value is {player.GetBestValue()}");
        
        if (player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} busted!");
            EndPlayerTurn(playerIndex);
        }
    }

    public void HandlePlayerStand(int playerIndex) {
        if (currentPhase != GamePhase.PlayerTurns || playerIndex != currentPlayerIndex) return;
        DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} stands with a {players[playerIndex].GetBestValue()}");
        EndPlayerTurn(playerIndex);
    }

    private void EndPlayerTurn(int playerIndex) {
        BlackjackPlayerController controller = playerControllers.Find(c => c.PlayerIndex == playerIndex);
        if (controller != null) {
            controller.DisableInput();
        }

        playerDisplays[playerIndex].HideControls();
        playerDisplays[playerIndex].ChangeToPlayerColor(InactiveColor);
        
        StartPlayerTurn(currentPlayerIndex+1);
    }

    private void UpdatePlayerDisplay(int playerIndex) {
        BlackjackPlayer player = players[playerIndex];
        playerDisplays[playerIndex].UpdateCardValueLabel(player.GetLowValue(), player.GetHighValue());
    }

    private IEnumerator PlayDealerTurn() {
        yield return new WaitForSeconds(0.5f);
        DebugLogger.Log(LogChannel.Systems, "Started dealer's turn.");
        while (dealer.GetBestValue() < dealerDrawThreshold) {
            Card newCard = shoe.DrawCard();
            dealer.DrawCard(newCard);
            dealerDisplay.AddCard(newCard);
            dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(),dealer.GetHighValue());
            
            DebugLogger.Log(LogChannel.Systems, $"Dealer drew. New value is {dealer.GetBestValue()}");
            yield return new WaitForSeconds(1f);
        }
        
        DebugLogger.Log(LogChannel.Systems, $"Dealer stands at {dealer.GetBestValue()}; Busted is {dealer.HasBusted()}");

        currentPhase = GamePhase.Results;
        yield return new WaitForSeconds(resultsDisplayDurationInSeconds);

        currentPhase = GamePhase.Ended;
        EvaluateWinners();
    }


    private void EvaluateWinners() {
        var results = new PlayerMinigameResult[4];
        int dealerValue = dealer.GetBestValue();
        if (dealer.HasBusted()) dealerValue = 0;

        for (int i = 0; i < players.Count; i++) {
            var player = players[i];
            int lowValue = player.GetLowValue();
            int highValue = player.GetHighValue();
            int playerValue = player.GetBestValue();
            if (player.HasBusted()) playerValue = 0;
            int bet = playerBets[i];

            float payoutMultiplier;
            int rank;
            int amountWonOrLost;

            CalculatePlayerResults(player, i, dealerValue, playerValue, out payoutMultiplier, out rank);
            
            int basePayout = Mathf.RoundToInt(bet * payoutMultiplier);
            int finalPayout = basePayout;
            if (isDoubleRound) {
                finalPayout *= 2;
                DebugLogger.Log(LogChannel.Systems, $"Double round: {basePayout} -> {finalPayout}");
            }

            amountWonOrLost = finalPayout - bet;
            results[i] = new PlayerMinigameResult(i, rank, finalPayout, bet);
            playerDisplays[i].UpdateForEndOfGame(lowValue, highValue, dealerValue, amountWonOrLost);
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Bet {bet}, Payout {finalPayout}, Net {amountWonOrLost}");
        }
        DebugLogger.Log(LogChannel.Systems, "Blackjack game complete.");
        dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(), dealer.GetHighValue(), "Final");
        OnMinigameFinished?.Invoke(results);
    }

    private void CalculatePlayerResults(BlackjackPlayer player, int i, int dealerValue, int playerValue,
        out float payoutMultiplier, out int rank) {
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
        } else {
            payoutMultiplier = losePayoutMultiplier;
            rank = 2;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Lost ({playerValue} vs dealer {dealerValue})");
        }
    }

    private void OnDestroy() {
        foreach (var controller in playerControllers) {
            if (controller != null) {
                controller.OnHit -= HandlePlayerHit;
                controller.OnStand -= HandlePlayerStand;
            }
        }
    }
}