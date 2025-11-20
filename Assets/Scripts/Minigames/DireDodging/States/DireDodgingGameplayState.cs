using UnityEngine;

public class DireDodgingGameplayState : IDireDodgingState {
    private int alivePlayers;
    private int[] playerPlaces;
    private int[] playerKills;
    private int[] deadPlayers;
    private MinigameTimer timer;
    private PlayerCornerDisplay[] playerCornerDisplays;
    public DireDodgingGameplayState(MinigameTimer timer, PlayerCornerDisplay[] playerCornerDisplays) {
        this.timer = timer;
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
        alivePlayers = 4;
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
        playerPlaces[playerIndex] = alivePlayers;
        deadPlayers[playerIndex] = 1;
        alivePlayers--;
        playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex], playerPlaces[playerIndex]);
        if (alivePlayers == 1) {
            OnGameplayEnd();
        }
    }

    private void OnGameplayEnd() {
        timer.OnTimerEnd -= OnGameplayEnd;
        timer.StopIfRunning();
        UpdateAllDisplays();
        DireDodgingMinigameManager.Instance.FreezeAllPlayers();
        DireDodgingMinigameManager.Instance.ReturnAllProjectiles();
        DireDodgingMinigameManager.Instance.TransitionToResults(playerPlaces, playerKills);
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