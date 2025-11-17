using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// TODO configure to userankfallback with economyservice

public class CoinTiltMinigameManager : MonoBehaviour, IMinigameManager
{
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }

    [Header("Minigame Settings")] 
    [SerializeField] private int gameDurationInSeconds = 30;
    [SerializeField] private int countdownDurationInSeconds = 5;
    [SerializeField] private float resultsDisplayDurationInSeconds = 5f;
    [SerializeField] private int[] fundsPerRank = new[] { 100, 80, 60, 50 };

    [Header("References")] 
    [SerializeField] private CoinTiltPlayer[] players = new CoinTiltPlayer[4];
    [SerializeField] private TiltingPlatform[] tiltingPlatforms = new TiltingPlatform[4];
    [SerializeField] private CoinSpawner[] coinSpawners = new CoinSpawner[4];
    [SerializeField] private PlayerCornerDisplay[] playerCornerDisplays = new PlayerCornerDisplay[4];
    [FormerlySerializedAs("countdown")] [SerializeField] private MinigameStartCountdown StartCountdown;
    [SerializeField] private MinigameTimer gameTimer;
    [SerializeField] private PlacesDisplay placesDisplay;
    private bool hasBeenInitialized;
    private readonly int[] playerScores = new int[4];

    private void Start() {
        StartCoroutine(WaitForInitialization());
    }

    public void Initialize(bool isDoubleRound) {
        this.IsDoubleRound = isDoubleRound;
        AdjustGameDurationForDoubleRound();
        CheckForGameObjectAssignments();
        InitializeVariables();
        
        hasBeenInitialized = true;
        DebugLogger.Log(LogChannel.Systems, $"CoinTiltMinigame initialized. Double round: {isDoubleRound}");
    }

    private void InitializeVariables() {
        InitializePlayerScores();
        InitializeCountdown();
        InitializeGameTimer();
        InitializePlacesDisplay();
    }

    private void InitializeGameTimer() {
        if (gameTimer == null) {
            Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Timer assigned!");
        }
        else {
            gameTimer.Initialize(gameDurationInSeconds);
            gameTimer.OnTimerEnd += EndGame;
        }
    }

    private void InitializeCountdown() {
        if (StartCountdown == null) {
            Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Countdown assigned!");
        }
        else {
            StartCountdown.Initialize(countdownDurationInSeconds);
        }
    }

    private void InitializePlacesDisplay() {
        if (placesDisplay == null) {
            Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Places Display assigned!");
        }
        else {
            placesDisplay.Hide();
        }
    }

    private void CheckForGameObjectAssignments() {
        CheckForPlayerAssignments();
        CheckForTiltingPlatformAssignments();
        CheckForCoinSpawnerAssignments();
    }

    private void AdjustGameDurationForDoubleRound() {
        if (IsDoubleRound) {
            gameDurationInSeconds *= 2;
        }
    }

    private void InitializePlayerScores() {
        for (int i = 0; i < 4; i++) {
            playerScores[i] = 0;
        }
    }

    private void CheckForCoinSpawnerAssignments() {
        if (coinSpawners == null || coinSpawners.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 Coin Spawners assigned!");
        }
    }

    private void CheckForTiltingPlatformAssignments() {
        if (tiltingPlatforms == null || tiltingPlatforms.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 Tilting Platforms assigned!");
        }
    }

    private void CheckForPlayerAssignments() {
        if (players == null || players.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 players assigned!");
        }
    }

    private IEnumerator WaitForInitialization() {
        while (!hasBeenInitialized) {
            yield return null;
        }

        StartCountdownPhase();
    }

    private void StartCountdownPhase() {
        InitializePlayers();
        InitializePlayerDisplays();
        
        DebugLogger.Log(LogChannel.Systems, "Starting countdown phase...");
        StartCountdown.StartTimer();
        StartCountdown.OnTimerEnd += StartPlayingPhase;
    }

    private void InitializePlayers() {
        for (int i = 0; i < players.Length; i++) {
            if (!players[i]) {
                Debug.LogError($"CoinTiltMinigameManager does not have a player in slot {i}!");
                continue;
            }

            var slot = GameSessionManager.Instance.PlayerSlots[i];
            if (!slot) {
                Debug.LogError($"PlayerSlot {i} not found!");
                continue;
            }
            
            InitializePlayerWithEvents(i, slot);
            InitializeTiltingPlatformForPlayer(i);
        }
    }

    private void InitializeTiltingPlatformForPlayer(int playerIndex) {
        if (tiltingPlatforms[playerIndex] != null) {
            tiltingPlatforms[playerIndex].Initialize(players[playerIndex]);
        }
    }

    private void InitializePlayerWithEvents(int playerIndex, PlayerSlot slot) {
        PlayerProfile profile = GameSessionManager.Instance.PlayerSlots[playerIndex].Profile;
        int numberOfMagnetPowerups = 0;
        foreach (var itemDefinition in profile.Inventory.Items) {
            if (itemDefinition.Id == "magnet") {
                numberOfMagnetPowerups++;
            }
        }
        players[playerIndex].Initialize(playerIndex, slot.Navigator, slot.IsAI, numberOfMagnetPowerups);
        players[playerIndex].OnCoinCollected += HandleCoinCollected;
        players[playerIndex].OnFallOff += HandlePlayerFall;
    }

    private void InitializePlayerDisplays() {
        for (int i = 0; i < playerCornerDisplays.Length; i++) {
            if (!playerCornerDisplays[i]) {
                Debug.LogWarning($"PlayerCornerDisplay {i} not found!");
                continue;
            }
            
            var slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot?.Profile != null) {
                playerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Score);
            }
        }
    }

    private void StartPlayingPhase() {
        StartCountdown.OnTimerEnd -= StartPlayingPhase;
        gameTimer.StartTimer();

        EnablePlayerInput();
        DebugLogger.Log(LogChannel.Systems, "Movement enabled.");
        StartCoinSpawning();
        DebugLogger.Log(LogChannel.Systems, "Game started!");
    }

    private void StartCoinSpawning() {
        for (int i = 0; i < coinSpawners.Length; i++) {
            if (coinSpawners[i]) {
                coinSpawners[i].StartSpawning(gameDurationInSeconds);
            }
        }
    }
    
    private void EnablePlayerInput() {
        foreach (var player in players) {
            if (player) {
                player.EnableInput();
            }
        }
    }


    private void HandleCoinCollected(int playerIndex, int coinValue) {
        playerScores[playerIndex] += coinValue;
        playerCornerDisplays[playerIndex].UpdateScore(playerScores[playerIndex]);
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} collected a coin. New score: {playerScores[playerIndex]}");
    }

    private void HandlePlayerFall(int playerIndex) {
        DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} fell!");
    }

    private void EndGame() {
        gameTimer.OnTimerEnd -= EndGame;
        DisablePlayerControlsAndMovement();
        FinalizeCoinSpawnerOperations();
        
        DebugLogger.Log(LogChannel.Systems, "Game ended. Calculating results.");
        StartCoroutine(DisplayResultsAndFinish());
    }

    private void DisablePlayerControlsAndMovement() {
        foreach (var player in players) {
            if (player) {
                player.DisableInput();
                player.Freeze();
            }
        }
    }

    private void FinalizeCoinSpawnerOperations() {
        for (int i = 0; i < coinSpawners.Length; i++) {
            if (coinSpawners[i]) {
                coinSpawners[i].StopSpawning();
                coinSpawners[i].DestroyAll();
            }
        }
    }

    private IEnumerator DisplayResultsAndFinish() {
        var results = CalculateResults();
        
        // short delay so that the players have a moment to breathe
        yield return new WaitForSeconds(0.75f);
        
        string[] resultsText = new string[4];
        for (int i = 0; i < results.Length; i++) {
            int fundsEarned = results[i].BaseFundsEarned;
            int currentFunds = GameSessionManager.Instance.PlayerSlots[i].Profile.Wallet.GetCurrentFunds();
            int newFunds = currentFunds + fundsEarned;
            int place = results[i].PlayerPlace;
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Score {playerScores[i]}, Rank {place}");
            resultsText[i] += GetPlaceText(place);
            resultsText[i] += "\n";
            resultsText[i] += "<size=50>+" + fundsEarned + " funds</size>";
            resultsText[i] += "\n";
            resultsText[i] += "<size=30>New funds: " + newFunds + "</size>";
        }
        placesDisplay.UpdateTextObjects(resultsText);
        placesDisplay.Show();
        yield return new WaitForSeconds(resultsDisplayDurationInSeconds);
        OnMinigameFinished?.Invoke(results);
    }

    private string GetPlaceText(int place) {
        return place switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            4 => "4th",
            _ => "ERR"
        };
    }

    private PlayerMinigameResult[] CalculateResults() {
        var results = new PlayerMinigameResult[4];

        var playerRankings = new List<(int index, int score)>();
        for (int i = 0; i < 4; i++) {
            playerRankings.Add((i, playerScores[i]));
        }
        
        playerRankings.Sort((a, b) => b.score.CompareTo(a.score));

        int[] ranks = new int[4];
        int currentRank = 0;
        for (int i = 0; i < playerRankings.Count; i++) {
            if (i > 0 && playerRankings[i].score == playerRankings[i - 1].score) {
                ranks[playerRankings[i].index] = ranks[playerRankings[i-1].index];
            }
            else {
                ranks[playerRankings[i].index] = currentRank;
                currentRank++;
            }

        }

        if (IsDoubleRound) {
            for (int i = 0; i < fundsPerRank.Length; i++) {
                fundsPerRank[i] = fundsPerRank[i] * 2;
            }
        }

        for (int i = 0; i < 4; i++) {
            results[i] = new PlayerMinigameResult(i, ranks[i], fundsPerRank[i]);
        }

        return results;
    }
    
    private void OnDestroy() {
        foreach(var player in players) {
            player.OnCoinCollected -= HandleCoinCollected;
            player.OnFallOff -= HandlePlayerFall;
        }
    }
}
