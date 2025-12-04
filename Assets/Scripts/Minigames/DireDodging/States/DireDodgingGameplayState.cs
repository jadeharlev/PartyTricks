using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DireDodgingGameplayState : IDireDodgingState {
    private int numberOfAlivePlayers;
    private int[] playerPlaces;
    private int[] playerKills;
    private int[] deadPlayers;
    private MinigameTimer timer;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private Camera gameCamera;
    private bool gameShouldEnd => (numberOfAlivePlayers <= 1);
    public DireDodgingGameplayState(MinigameTimer timer, PlayerCornerDisplay[] playerCornerDisplays, Camera camera) {
        this.timer = timer;
        gameCamera = camera;
        timer.OnTimerEnd += OnGameplayEnd;
        timer.OnHalfwayPointReached += OnHalfwayPointReached;
        this.playerCornerDisplays = playerCornerDisplays;
    }

    private void OnHalfwayPointReached(int remainingTimeInSeconds) {
        timer.OnHalfwayPointReached -= OnHalfwayPointReached;
        DireDodgingMinigameManager.Instance.StartIncreasingIntensity(remainingTimeInSeconds);
    }
    

    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Gameplay State.", LogLevel.Verbose);
        DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
        DireDodgingMinigameManager.Instance.StartPlayerShooting();
        timer.StartTimer();
        numberOfAlivePlayers = 4;
        playerPlaces = new[] { 1, 1, 1, 1 };
        playerKills = new[] { 0, 0, 0, 0 };
        deadPlayers = new[] { 0, 0, 0, 0 };
    }

    public void OnUpdate() {
        
    }

    public void HandlePlayerKill(int playerIndex) {
        playerKills[playerIndex]++;
        if (PlayerIsDead(playerIndex)) {
            Debug.Log("Updating kills for dead player with index " + playerIndex);
            playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex], playerPlaces[playerIndex]);
        }
        else {
            playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex]);
        }
    }

    private bool PlayerIsDead(int playerIndex) {
        return deadPlayers[playerIndex] == 1;
    }

    public void HandlePlayerDeath(int playerIndex) {
        playerPlaces[playerIndex] = numberOfAlivePlayers;
        deadPlayers[playerIndex] = 1;
        numberOfAlivePlayers--;
        UpdateEliminations(playerIndex);
        gameCamera.DOShakePosition(duration: 0.1f, strength: 0.4f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
        PauseManager.Instance.DoTimedPause(0.3f, CheckForEndOfGame);
    }

    private void UpdateEliminations(int playerIndex) {
        playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex], playerPlaces[playerIndex]);
    }

    private void CheckForEndOfGame() {
        if (gameShouldEnd) {
            OnGameplayEnd();
        }
    }

    private void OnGameplayEnd() {
        timer.OnTimerEnd -= OnGameplayEnd;
        timer.StopIfRunning();
        UpdateAllDisplays();
        DireDodgingMinigameManager.Instance.FreezeAllPlayers();
        DireDodgingMinigameManager.Instance.ReturnAllProjectiles();
        PauseManager.Instance.DoTimedPause(1f, () =>
        {
            DireDodgingMinigameManager.Instance.TransitionToResults(playerPlaces, playerKills); 
        });
    }

    private void UpdateAllDisplays() {
        for (int i = 0; i < 4; i++) {
            playerCornerDisplays[i].UpdateEliminations(playerKills[i], playerPlaces[i]);
        }
    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Gameplay State.", LogLevel.Verbose);
    }
}