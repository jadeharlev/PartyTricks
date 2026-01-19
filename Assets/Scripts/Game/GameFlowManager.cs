using System;
using System.Collections.Generic;
using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour, IGameFlowService {
    [SerializeField] private MinigameConfigSO config;
    [SerializeField] private int initialGameLength = 5;
    [SerializeField] private GameObject gameBoardDisplayPrefab;
    
    private GameBoardGenerator boardGenerator;
    private GameBoardDisplay currentBoardDisplay;
    private IEconomyService economyService;
    private int currentRoundIndex = -1;
    private List<(MinigameType minigameType, bool IsDouble)> gameBoard;
    private bool GameIsOver => currentRoundIndex >= gameBoard.Count;
    
    private void Awake() {
        economyService = ServiceLocatorAccessor.GetService<IEconomyService>();
        boardGenerator = new GameBoardGenerator(initialGameLength);
    }

    public void StartGame() {
        boardGenerator.GenerateRandomBoard();
        gameBoard = boardGenerator.GameBoard;
        currentRoundIndex = 0;
        DebugLogger.Log(LogChannel.Systems, $"Game started. Board generated with {gameBoard.Count} rounds.");
        ShowGameBoardDisplay();
    }

    private void ShowGameBoardDisplay() {
        if (gameBoardDisplayPrefab == null) {
            Debug.LogError("No game board display prefab assigned");
            return;
        }
        GameObject displayObject = Instantiate(gameBoardDisplayPrefab);
        currentBoardDisplay = displayObject.GetComponent<GameBoardDisplay>();
        if (currentBoardDisplay != null) {
            currentBoardDisplay.OnContinue += HandleBoardDisplayFinished;
            currentBoardDisplay.ShowBoard(gameBoard);
        }
        else {
            Debug.LogError("GameBoardDisplay component missing from prefab.");
        }
    }

    private void HandleBoardDisplayFinished() {
        if (currentBoardDisplay != null) {
            currentBoardDisplay.OnContinue -= HandleBoardDisplayFinished;
            currentBoardDisplay = null;
        }
        TransitionToShop();
    }

    public void OnShopEnd() {
        DebugLogger.Log(LogChannel.Systems, "Shop phase over. Transitioning to next minigame.");
        StartNextRound();
    }

    private void StartNextRound() {
        TransitionToMinigame();
    }

    private void TransitionToShop() {
        SceneManager.LoadScene("Shop");
    }

    private void TransitionToMinigame() {
        var nextRound = gameBoard[currentRoundIndex];
        MinigameType minigameType = nextRound.minigameType;
        string sceneName = config.GetRandomSceneName(minigameType);
        if (string.IsNullOrEmpty(sceneName)) {
            Debug.LogError($"Failed to load scene for type {minigameType}. Stopping.");
            return;
        }

        SceneManager.LoadScene(sceneName);
        DebugLogger.Log(LogChannel.Systems, $"Loading minigame: {minigameType} (Scene: {sceneName}). Double Round: {nextRound.IsDouble}");
        SceneManager.sceneLoaded += OnMinigameSceneLoaded;
    }

    private void OnMinigameSceneLoaded(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnMinigameSceneLoaded;
        var minigameManager = FindMinigameManager();
        if (minigameManager != null) {
            minigameManager.OnMinigameFinished += ProcessMinigameResults;
            var currentRoundDefinition = GetCurrentRoundDefinition();
            // TODO clean up
            switch (minigameManager) {
                case TestMinigameManager testManager:
                    testManager.Initialize(currentRoundDefinition.IsDouble);
                    break;
                case BlackjackMinigameManager blackjackManager:
                    blackjackManager.Initialize(currentRoundDefinition.IsDouble);
                    break;
                case CoinTiltMinigameManager coinTiltMinigameManager:
                    coinTiltMinigameManager.Initialize(currentRoundDefinition.IsDouble);
                    break;
                case DireDodgingMinigameManager direDodgingMinigameManager:
                    direDodgingMinigameManager.Initialize(currentRoundDefinition.IsDouble);
                    break;
            }
        }
        else {
            Debug.LogError($"GameFlowManager: Couldn't find minigame manager for scene {scene.name}");
        }
    }

    private static IMinigameManager FindMinigameManager() {
        IMinigameManager minigameManager = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IMinigameManager>().FirstOrDefault();
        return minigameManager;
    }

    public void ProcessMinigameResults(PlayerMinigameResult[] results) {
        var minigameManager = FindMinigameManager();
        if (minigameManager != null) {
            minigameManager.OnMinigameFinished -= ProcessMinigameResults;
        }
        
        economyService.ApplyRewards(results);

        DebugLogger.Log(LogChannel.Systems, "Minigame finished, results processed. Transitioning back to shop.");
        currentRoundIndex++;
        if (GameIsOver) {
            EndGame();
        } else TransitionToShop();
    }

    private void EndGame() {
        DebugLogger.Log(LogChannel.Systems, "Ending game.");
        SceneManager.LoadScene("Results");
    }

    public (MinigameType minigameType, bool IsDouble) GetCurrentRoundDefinition() {
        if (currentRoundIndex >= 0 && currentRoundIndex < gameBoard.Count) {
            return gameBoard[currentRoundIndex];
        }

        return (MinigameType.Unknown, false);
    }

    public List<(MinigameType minigameType, bool isDouble)> GetCompletedMinigameList() {
        List<(MinigameType, bool)> completedMinigames = new();
        for (int i = 0; i < currentRoundIndex; i++) {
            completedMinigames.Add(gameBoard[i]);
        }
        return completedMinigames;
    }

    public List<(MinigameType minigameType, bool isDouble)> GetUpcomingMinigameList() {
        List<(MinigameType, bool)> upcomingMinigames = new();
        for (int i = currentRoundIndex; i < gameBoard.Count; i++) {
            if (i == -1) continue;
            upcomingMinigames.Add(gameBoard[i]);
        }
        return upcomingMinigames;
    }
}