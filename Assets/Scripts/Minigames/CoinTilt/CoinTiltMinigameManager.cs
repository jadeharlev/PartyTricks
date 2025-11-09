using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO configure to userankfallback with economyservice

public class CoinTiltMinigameManager : MonoBehaviour, IMinigameManager
{
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }

    [Header("Minigame Settings")] 
    [SerializeField] private int gameDurationInSeconds = 30;
    [SerializeField] private int countdownDurationInSeconds = 5;
    [SerializeField] private float resultsDisplayDurationInSeconds = 5f;

    [Header("References")] 
    [SerializeField] private CoinTiltPlayer[] players = new CoinTiltPlayer[4];
    [SerializeField] private TiltingPlatform[] tiltingPlatforms = new TiltingPlatform[4];
    [SerializeField] private CoinSpawner[] coinSpawners = new CoinSpawner[4];
    [SerializeField] private PlayerCornerDisplay[] playerCornerDisplays = new PlayerCornerDisplay[4];
    [SerializeField] private CoinTiltMinigameCountdown countdown;
    [SerializeField] private CoinTiltGameTimer gameTimer;

    private GamePhase currentPhase = GamePhase.Countdown;
    private bool hasBeenInitialized;
    private readonly int[] playerScores = new int[4];

    private void Awake() {
        // InitializeVariables();
    }

    private void Start() {
        StartCoroutine(WaitForInitialization());
    }

    public void Initialize(bool isDoubleRound) {
        this.IsDoubleRound = isDoubleRound;
        InitializeVariables();
        hasBeenInitialized = true;
        DebugLogger.Log(LogChannel.Systems, $"CoinTiltMinigame initialized. Double round: {isDoubleRound}");
    }

    private void InitializeVariables() {

        if (IsDoubleRound) {
            gameDurationInSeconds *= 2;
        }
        
        if (players == null || players.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 players assigned!");
        }

        if (tiltingPlatforms == null || tiltingPlatforms.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 Tilting Platforms assigned!");
        }

        if (coinSpawners == null || coinSpawners.Length != 4) {
            Debug.LogError("CoinTiltMinigameManager does not have 4 Coin Spawners assigned!");
        }

        for (int i = 0; i < 4; i++) {
            playerScores[i] = 0;
        }

        if (countdown == null) {
            Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Countdown assigned!");
        }
        else {
            countdown.Initialize(countdownDurationInSeconds);
        }

        if (gameTimer == null) {
            Debug.LogError("CoinTiltMinigameManager does not have a Coin Tilt Minigame Timer assigned!");
        }
        else {
            gameTimer.Initialize(gameDurationInSeconds);
            gameTimer.OnTimerEnd += EndGame;
        }
        
    }

    private IEnumerator WaitForInitialization() {
        while (!hasBeenInitialized) {
            yield return null;
        }

        StartCountdownPhase();
    }

    private void StartCountdownPhase() {
        currentPhase = GamePhase.Countdown;
        InitializePlayers();
        InitializePlayerDisplays();
        
        DebugLogger.Log(LogChannel.Systems, "Starting countdown phase...");
        countdown.StartTimer();
        countdown.OnTimerEnd += StartPlayingPhase;
    }

    private void InitializePlayers() {
        for (int i = 0; i < players.Length; i++) {
            if (players[i] == null) {
                Debug.LogError($"CoinTiltMinigameManager does not have a player in slot {i}!");
                continue;
            }

            var slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot == null) {
                Debug.LogError($"PlayerSlot {i} not found!");
                continue;
            }
            
            players[i].Initialize(i, slot.Navigator, slot.IsAI, this);
            players[i].OnCoinCollected += HandleCoinCollected;
            players[i].OnFallOff += HandlePlayerFall;

            if (tiltingPlatforms[i] != null) {
                tiltingPlatforms[i].Initialize(players[i]);
            }
        }
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
        countdown.OnTimerEnd -= StartPlayingPhase;
        currentPhase = GamePhase.Playing;
        gameTimer.StartTimer();

        foreach (var player in players) {
            if (player) {
                player.EnableInput();
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, "Movement enabled.");

        for (int i = 0; i < coinSpawners.Length; i++) {
            if (coinSpawners[i]) {
                coinSpawners[i].StartSpawning(gameDurationInSeconds);
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, "Game started!");
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
        currentPhase = GamePhase.Results;

        foreach (var player in players) {
            if (player) {
                player.DisableInput();
                player.Freeze();
            }
        }
        
        
        for (int i = 0; i < coinSpawners.Length; i++) {
            if (coinSpawners[i]) {
                coinSpawners[i].StopSpawning();
                coinSpawners[i].DestroyAll();
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, "Game ended. Calculating results.");
        StartCoroutine(DisplayResultsAndFinish());
    }

    private IEnumerator DisplayResultsAndFinish() {
        var results = CalculateResults();
        for (int i = 0; i < results.Length; i++) {
            DebugLogger.Log(LogChannel.Systems, $"Player {i}: Score {playerScores[i]}, Rank {results[i].PlayerPlace}");
        }
        yield return new WaitForSeconds(resultsDisplayDurationInSeconds);
        currentPhase = GamePhase.Ended;
        OnMinigameFinished?.Invoke(results);
    }

    private PlayerMinigameResult[] CalculateResults() {
        var results = new PlayerMinigameResult[4];

        var playerRankings = new List<(int index, int score)>();
        for (int i = 0; i < 4; i++) {
            playerRankings.Add((i, playerScores[i]));
        }
        
        playerRankings.Sort((a, b) => b.score.CompareTo(a.score));

        int[] ranks = new int[4];
        for (int i = 0; i < playerRankings.Count; i++) {
            ranks[playerRankings[i].index] = i;
        }

        int[] fundsPerRank = new[] { 100, 80, 60, 50 };

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

    private enum GamePhase {
        Countdown,
        Playing,
        Results,
        Ended
    }
}
