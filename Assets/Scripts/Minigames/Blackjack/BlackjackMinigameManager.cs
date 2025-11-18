using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackMinigameManager : MonoBehaviour, IGamblingMinigame {
    private static readonly Color[] PlayerColors =
    {
        Color.darkRed,
        Color.darkBlue,
        Color.rebeccaPurple,
        Color.darkGreen
    };

    private static readonly Color InactiveColor = new(0.3f, 0.3f, 0.3f);

    [SerializeField] private bool isDoubleRound;

    [Header("Blackjack Settings")] [SerializeField]
    private int dealerDrawThreshold = 17;

    [SerializeField] private float blackjackPayoutMultiplier = 3f;
    [SerializeField] private float winPayoutMultiplier = 2f;
    [SerializeField] private float pushPayoutMultiplier = 1f;
    [SerializeField] private float losePayoutMultiplier;
    [SerializeField] private float aiDecisionDelayInSeconds = 1.5f;
    
    // how long after the dealer's cards until results are calculated?
    [SerializeField] private float blackjackResultsDisplayDurationInSeconds = 3f;
    
    // how long after results are calculated until the game moves on?
    [SerializeField] private float gameResultsDisplayDurationInSeconds = 3f;


    [Header("Game Objects")] [SerializeField]
    private BlackjackPlayerDisplay[] playerDisplays;

    [SerializeField] private BlackjackDealerDisplay dealerDisplay;

    [SerializeField] private Bets bettingSystem;

    [SerializeField] private GameObject blackjackPanel;

    private GamePhase currentPhase = GamePhase.Betting;
    private int currentPlayerIndex;
    private BlackjackPlayer dealer;
    private int[] playerBets;
    private List<BlackjackPlayerController> playerControllers;
    private List<BlackjackPlayer> players;

    private BlackjackShoe shoe;
    private bool hasBeenInitialized;

    private void Awake() {
        InitializeVariables();
        HideBlackjackPanel();
    }

    private void Start() {
        StartCoroutine(WaitForInitialization());
    }

    private void OnDestroy() {
        foreach (var controller in playerControllers)
            if (controller != null) {
                controller.OnHit -= HandlePlayerHit;
                controller.OnStand -= HandlePlayerStand;
            }
    }

    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public Action<Dictionary<int, int>> OnBetTimerEnd { get; set; }
    public bool IsDoubleRound => isDoubleRound;

    public void SetPlayerBets(int[] bets) {
        playerBets = bets;
        DebugLogger.Log(LogChannel.Systems, $"Best placed: [{string.Join(", ", bets)}]");
    }

    private void HideBlackjackPanel() {
        if (blackjackPanel != null) blackjackPanel.SetActive(false);
    }

    private void InitializeVariables() {
        shoe = new BlackjackShoe();
        dealer = new BlackjackPlayer();
        players = new List<BlackjackPlayer>();
        playerControllers = new List<BlackjackPlayerController>();

        for (var i = 0; i < 4; i++) {
            PlayerProfile playerProfile = GameSessionManager.Instance.PlayerSlots[i].Profile;
            int numOfOddsPowerups = 0;
            foreach (var itemDefinition in playerProfile.Inventory.Items) {
                if (itemDefinition.Id == "increaseBettingOdds") {
                    numOfOddsPowerups++;
                }
            }
            players.Add(new BlackjackPlayer(numOfOddsPowerups));
        }
    }

    private IEnumerator WaitForInitialization() {
        while (!hasBeenInitialized) yield return null;
        StartBettingPhase();
    }

    public void Initialize(bool isDoubleRound) {
        this.isDoubleRound = isDoubleRound;
        hasBeenInitialized = true;
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

    private void OnBettingComplete(Dictionary<int, int> bets) {
        OnBetTimerEnd -= OnBettingComplete;
        ProcessPlayerBets(bets);

        DebugLogger.Log(LogChannel.Systems, "Betting complete, starting game.");

        ShowBlackjackPanel();
        StartCoroutine(StartGameSequence());
    }

    private void ShowBlackjackPanel() {
        if (blackjackPanel != null) blackjackPanel.SetActive(true);
    }

    private void ProcessPlayerBets(Dictionary<int, int> bets) {
        playerBets = new int[4];
        for (var i = 0; i < 4; i++) {
            playerBets[i] = bets.GetValueOrDefault(i, 50);
            DeductPlayerBetFunds(i);
        }
    }

    private void DeductPlayerBetFunds(int playerIndex) {
        var slot = GameSessionManager.Instance.PlayerSlots[playerIndex];
        if (slot?.Profile != null) {
            slot.Profile.Wallet.RemoveFunds(playerBets[playerIndex]);
            DebugLogger.Log(LogChannel.Systems,
                $"Player {playerIndex} bet {playerBets[playerIndex]}. Remaining funds: {slot.Profile.Wallet.GetCurrentFunds()}");
        }
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

        for (var i = 0; i < 4; i++) {
            var slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot?.Navigator == null) continue;
            SetUpBlackjackPlayerControllerObject(i, slot);
        }
    }

    private void SetUpBlackjackPlayerControllerObject(int playerIndex, PlayerSlot slot) {
        var controllerObject = new GameObject("Player_{i}_BlackjackController");
        controllerObject.transform.SetParent(transform);

        var controller = SetUpBlackjackPlayerController(playerIndex, slot, controllerObject);

        playerControllers.Add(controller);
    }

    private BlackjackPlayerController SetUpBlackjackPlayerController(int playerIndex, PlayerSlot slot,
        GameObject controllerObject) {
        var controller = controllerObject.AddComponent<BlackjackPlayerController>();
        controller.Initialize(playerIndex, slot.Navigator, slot.IsAI);
        controller.OnHit += HandlePlayerHit;
        controller.OnStand += HandlePlayerStand;
        return controller;
    }

    private void InitializePlayerDisplays() {
        for (var i = 0; i < playerDisplays.Length; i++) {
            playerDisplays[i].UpdatePlayerNumberDisplay(i);

            var slot = GameSessionManager.Instance.PlayerSlots[i];
            var funds = slot?.Profile?.Wallet.GetCurrentFunds() ?? 0;
            playerDisplays[i].UpdateBetLabel(playerBets[i], funds);
            playerDisplays[i].ChangeToPlayerColor(InactiveColor);
            playerDisplays[i].HideControls();
        }

        dealerDisplay.UpdateCardValueLabel(0, 0);
    }

    private IEnumerator DealInitialCards() {
        yield return DrawInitialCardsForPlayers();
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to all players.");

        yield return DrawAndDisplayDealerCards();
        DebugLogger.Log(LogChannel.Systems, "Initial cards dealt to dealer.");
    }

    private IEnumerator DrawAndDisplayDealerCards() {
        var secondCard = shoe.DrawCard();
        var card = shoe.DrawCard();

        EnsureNoDealerInstantBlackjack(ref card, ref secondCard);

        dealer.DrawCard(card);
        dealerDisplay.AddCard(card);
        yield return new WaitForSeconds(0.3f);

        dealer.DrawCard(secondCard);
        dealerDisplay.AddCard(secondCard);
        yield return new WaitForSeconds(0.3f);

        dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(), dealer.GetHighValue());
    }

    private IEnumerator DrawInitialCardsForPlayers() {
        for (var i = 0; i < players.Count; i++) {
            var player = players[i];
            var playerDisplay = playerDisplays[i];

            DrawAndDisplayCard(player, playerDisplay, i);
            yield return new WaitForSeconds(0.3f);

            DrawAndDisplayCard(player, playerDisplay, i);
            UpdatePlayerDisplay(i);

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void DrawAndDisplayCard(BlackjackPlayer player, BlackjackPlayerDisplay playerDisplay, int playerIndex) {
        var card = shoe.DrawCard();
        var cardValue = CalculateCardValue(card);
        bool willBust = player.GetLowValue() + cardValue > 21;
        if (willBust) {
            if (player.TryToProtect() == true) {
                Debug.Log("Drew a " + cardValue + ", which would've caused a bust. Protection succeeded.");
                DrawAndDisplayCard(player, playerDisplay, playerIndex);
                return;
            }
        }
        player.DrawCard(card);
        playerDisplay.AddCard(card);
        UpdatePlayerDisplay(playerIndex);
    }

    private int CalculateCardValue(Card card) {
        int cardValue = 0;
        if (card.Value == 1) {
            cardValue++;
        }
        else if (card.IsFaceCard) {
            cardValue += 10;
        }
        else {
            cardValue += card.Value;
        }

        return cardValue;
    }

    private void EnsureNoDealerInstantBlackjack(ref Card firstCard, ref Card secondCard) {
        while (((secondCard.IsFaceCard || secondCard.Value == 10) && firstCard.Value == 1) || 
               ((firstCard.IsFaceCard || firstCard.Value == 10) && secondCard.Value == 1)) {
            firstCard = shoe.DrawCard();
            secondCard = shoe.DrawCard();
        }
    }

    private void StartPlayerTurn(int playerIndex) {
        currentPlayerIndex = playerIndex;

        var shouldTransitionToDealerTurn = currentPlayerIndex >= players.Count;
        if (shouldTransitionToDealerTurn) {
            currentPhase = GamePhase.DealerTurn;
            StartCoroutine(PlayDealerTurn());
            return;
        }

        var player = players[currentPlayerIndex];

        if (player.GotBlackjack) {
            DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} had blackjack; skipping turn.");
            EndPlayerTurn(currentPlayerIndex);
            return;
        }

        if (EndPlayerTurnIfHas21(player)) return;

        if (player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} already busted; skipping turn.");
            EndPlayerTurn(currentPlayerIndex);
            return;
        }

        playerDisplays[currentPlayerIndex].ChangeToPlayerColor(PlayerColors[currentPlayerIndex]);
        playerDisplays[currentPlayerIndex].ShowControls();

        var controller = playerControllers.Find(c => c.PlayerIndex == currentPlayerIndex);
        if (controller != null) {
            controller.EnableInput();

            if (controller.IsAI) StartCoroutine(HandleAITurn(currentPlayerIndex));
        }

        DebugLogger.Log(LogChannel.Systems, $"Started turn for player {currentPlayerIndex}. IsAI: {controller.IsAI}");
    }

    private bool EndPlayerTurnIfHas21(BlackjackPlayer player) {
        if (player.GetBestValue() == 21) {
            DebugLogger.Log(LogChannel.Systems, $"Player {currentPlayerIndex} had 21; skipping turn.");
            EndPlayerTurn(currentPlayerIndex);
            return true;
        }

        return false;
    }

    private IEnumerator HandleAITurn(int playerIndex) {
        yield return new WaitForSeconds(aiDecisionDelayInSeconds);
        var player = players[playerIndex];
        HitIfAppropriateOnAI(playerIndex, player);
        StandIfAppropriateOnAI(playerIndex, player);
    }

    private void StandIfAppropriateOnAI(int playerIndex, BlackjackPlayer player) {
        if (!player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"AI Player {playerIndex} chose to stand.");
            HandlePlayerStand(playerIndex);
        }
    }

    private void HitIfAppropriateOnAI(int playerIndex, BlackjackPlayer player) {
        while (!player.HasBusted() && player.GetBestValue() < 17) {
            DebugLogger.Log(LogChannel.Systems, $"AI Player {playerIndex} chose to hit.");
            HandlePlayerHit(playerIndex);
        }
    }

    private void HandlePlayerHit(int playerIndex) {
        if (currentPhase != GamePhase.PlayerTurns || playerIndex != currentPlayerIndex) return;

        var player = players[playerIndex];

        DrawAndDisplayCard(player, playerDisplays[playerIndex], playerIndex);
        DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} hit. New value is {player.GetBestValue()}");
        if (EndPlayerTurnIfHas21(player)) return;
        EndTurnIfBusted(playerIndex, player);
    }

    private void EndTurnIfBusted(int playerIndex, BlackjackPlayer player) {
        if (player.HasBusted()) {
            DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} busted!");
            EndPlayerTurn(playerIndex);
        }
    }

    private void HandlePlayerStand(int playerIndex) {
        if (currentPhase != GamePhase.PlayerTurns || playerIndex != currentPlayerIndex) return;
        DebugLogger.Log(LogChannel.Systems,
            $"Player {playerIndex} stands with a {players[playerIndex].GetBestValue()}");
        EndPlayerTurn(playerIndex);
    }

    private void EndPlayerTurn(int playerIndex) {
        var controller = playerControllers.Find(c => c.PlayerIndex == playerIndex);
        if (controller != null) controller.DisableInput();

        playerDisplays[playerIndex].HideControls();
        playerDisplays[playerIndex].ChangeToPlayerColor(InactiveColor);

        StartPlayerTurn(currentPlayerIndex + 1);
    }

    private void UpdatePlayerDisplay(int playerIndex) {
        var player = players[playerIndex];
        playerDisplays[playerIndex].UpdateCardValueLabel(player.GetLowValue(), player.GetHighValue());
    }

    private IEnumerator PlayDealerTurn() {
        yield return new WaitForSeconds(0.5f);
        DebugLogger.Log(LogChannel.Systems, "Started dealer's turn.");

        while (dealer.GetBestValue() < dealerDrawThreshold) {
            DrawAndDisplayDealerCard();

            DebugLogger.Log(LogChannel.Systems, $"Dealer drew. New value is {dealer.GetBestValue()}");
            yield return new WaitForSeconds(1f);
        }

        DebugLogger.Log(LogChannel.Systems,
            $"Dealer stands at {dealer.GetBestValue()}; Busted is {dealer.HasBusted()}");

        currentPhase = GamePhase.Results;
        yield return new WaitForSeconds(blackjackResultsDisplayDurationInSeconds);

        currentPhase = GamePhase.Ended;
        EvaluateWinners();
    }

    private void DrawAndDisplayDealerCard() {
        var card = shoe.DrawCard();
        dealer.DrawCard(card);
        dealerDisplay.AddCard(card);
        dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(), dealer.GetHighValue());
    }


    private void EvaluateWinners() {
        var results = new PlayerMinigameResult[4];

        var dealerValue = dealer.GetBestValue();
        if (dealer.HasBusted()) dealerValue = 0;

        var playerOutcomes = CalculatePlayerOutcomes(dealerValue);
        var rankedOutcomes = RankPlayerOutcomes(playerOutcomes);

        ProcessPlayerOutcomes(playerOutcomes, rankedOutcomes, results, dealerValue);

        DebugLogger.Log(LogChannel.Systems, "Blackjack game complete.");

        dealerDisplay.UpdateCardValueLabel(dealer.GetLowValue(), dealer.GetHighValue(), "Final");
        
        StartCoroutine(WaitAndTriggerMinigameEnd(results));
    }

    private IEnumerator WaitAndTriggerMinigameEnd(PlayerMinigameResult[] results) {
        yield return new WaitForSeconds(gameResultsDisplayDurationInSeconds);
        OnMinigameFinished?.Invoke(results);
    }

    private void ProcessPlayerOutcomes(
        (int currentPlayerIndex, OutcomeType outcome, int playerValue, int payout)[] playerOutcomes,
        int[] rankedOutcomes,
        PlayerMinigameResult[] results, int dealerValue) {
        for (var i = 0; i < players.Count; i++) {
            var (playerIndex, outcome, playerValue, finalPayout) = playerOutcomes[i];
            var rank = UpdatePlayerMinigameResult(rankedOutcomes, playerIndex, finalPayout, results, out var bet,
                out var amountWonOrLost);
            UpdatePlayerDisplayForEndOfGame(playerIndex, dealerValue, amountWonOrLost, outcome == OutcomeType.Blackjack);
            LogPlayerResult(amountWonOrLost, playerIndex, bet, finalPayout, rank);
        }
    }

    private static void LogPlayerResult(int amountWonOrLost, int playerIndex, int bet, int finalPayout, int rank) {
        string netText;
        if (amountWonOrLost >= 0)
            netText = $"+{amountWonOrLost}";
        else
            netText = amountWonOrLost.ToString();
        DebugLogger.Log(LogChannel.Systems,
            $"Player {playerIndex}: Bet {bet}, Payout {finalPayout}, Net {netText}, Rank {rank + 1}");
    }

    private void UpdatePlayerDisplayForEndOfGame(int playerIndex, int dealerValue, int amountWonOrLost, bool gotBlackjack) {
        var lowValue = players[playerIndex].GetLowValue();
        var highValue = players[playerIndex].GetHighValue();
        playerDisplays[playerIndex].UpdateForEndOfGame(lowValue, highValue, dealerValue, amountWonOrLost, gotBlackjack);
    }

    private int UpdatePlayerMinigameResult(int[] rankedOutcomes, int playerIndex, int finalPayout,
        PlayerMinigameResult[] results, out int bet, out int amountWonOrLost) {
        var rank = rankedOutcomes[playerIndex];
        bet = playerBets[playerIndex];
        amountWonOrLost = finalPayout - bet;
        results[playerIndex] = new PlayerMinigameResult(playerIndex, rank, finalPayout, bet);
        return rank;
    }

    private (int currentPlayerIndex, OutcomeType outcome, int playerValue, int payout)[] CalculatePlayerOutcomes(int dealerValue) {
        var playerOutcomes = new (int currentPlayerIndex, OutcomeType outcome, int playerValue, int payout)[4];
        for (var i = 0; i < players.Count; i++) {
            var player = players[i];
            var playerValue = player.GetBestValue();
            if (player.HasBusted()) playerValue = 0;
            var bet = playerBets[i];

            CalculatePlayerOutcome(player, i, dealerValue, playerValue, out var payoutMultiplier, out var outcome);

            var finalPayout = CalculateFinalPayout(bet, payoutMultiplier);
            playerOutcomes[i] = (i, outcome, playerValue, finalPayout);
        }

        return playerOutcomes;
    }

    private int CalculateFinalPayout(int bet, float payoutMultiplier) {
        var basePayout = Mathf.RoundToInt(bet * payoutMultiplier);
        var finalPayout = basePayout;
        if (isDoubleRound) finalPayout *= 2;

        return finalPayout;
    }

    private int[] RankPlayerOutcomes((int playerIndex, OutcomeType outcome, int playerValue, int payout)[] outcomes) {
        var ranks = new int[4];
        var sortedPlayers = new List<(int index, OutcomeType outcome, int playerValue)>();

        for (var i = 0; i < outcomes.Length; i++)
            sortedPlayers.Add((outcomes[i].playerIndex, outcomes[i].outcome, outcomes[i].playerValue));

        DeterminePlayerOutcomeOrder(sortedPlayers);

        for (var i = 0; i < sortedPlayers.Count; i++) ranks[sortedPlayers[i].index] = i;

        return ranks;
    }

    private static void DeterminePlayerOutcomeOrder(
        List<(int index, OutcomeType outcome, int playerValue)> sortedPlayers) {
        sortedPlayers.Sort((a, b) =>
        {
            var outcomeCompare = a.playerValue.CompareTo(b.playerValue);
            if (outcomeCompare != 0) return outcomeCompare;
            return b.playerValue.CompareTo(a.playerValue);
        });
    }

    private void CalculatePlayerOutcome(BlackjackPlayer player, int i, int dealerValue, int playerValue,
        out float payoutMultiplier, out OutcomeType outcome) {
        if (player.GotBlackjack && dealer.GetBestValue() != 21) {
            payoutMultiplier = blackjackPayoutMultiplier;
            outcome = OutcomeType.Blackjack;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Blackjack ({playerValue} vs dealer {dealerValue})");
        }
        else if (player.HasBusted()) {
            payoutMultiplier = losePayoutMultiplier;
            outcome = OutcomeType.Bust;
            DebugLogger.Log(LogChannel.Systems,
                $"Player {i}: Busted ({player.GetBestValue()} vs dealer {dealerValue})");
        }
        else if (dealer.HasBusted() || playerValue > dealerValue) {
            payoutMultiplier = winPayoutMultiplier;
            outcome = OutcomeType.Win;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Won ({playerValue} vs dealer {dealerValue})");
        }
        else if (playerValue == dealerValue) {
            payoutMultiplier = pushPayoutMultiplier;
            outcome = OutcomeType.Push;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Pushed ({playerValue} vs dealer {dealerValue})");
        }
        else {
            payoutMultiplier = losePayoutMultiplier;
            outcome = OutcomeType.Loss;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Lost ({playerValue} vs dealer {dealerValue})");
        }
    }


    private enum GamePhase {
        Betting,
        InitialDeal,
        PlayerTurns,
        DealerTurn,
        Results,
        Ended
    }

    private enum OutcomeType {
        Blackjack = 0,
        Win = 1,
        Push = 2,
        Loss = 3,
        Bust = 4
    }
}