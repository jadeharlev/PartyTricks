public class DireDodgingGameplayState : IDireDodgingState {
    private int alivePlayers;
    private int[] playerPlaces;
    private int[] playerKills;
    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Gameplay State.", LogLevel.Verbose);
        DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
        DireDodgingMinigameManager.Instance.StartPlayerShooting();
        alivePlayers = 4;
        playerPlaces = new[] { 1, 1, 1, 1 };
        playerKills = new[] { 0, 0, 0, 0 };
    }

    public void OnUpdate() {
        
    }

    public void HandlePlayerKill(int playerIndex) {
        playerKills[playerIndex]++;
    }

    public void HandlePlayerDeath(int playerIndex) {
        playerPlaces[playerIndex] = alivePlayers;
        alivePlayers--;
        if (alivePlayers == 1) {
            OnGameplayEnd();
        }
    }

    private void OnGameplayEnd() {
        DireDodgingMinigameManager.Instance.FreezeAllPlayers();
        DireDodgingMinigameManager.Instance.TransitionToResults(playerPlaces, playerKills);
    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Gameplay State.", LogLevel.Verbose);
    }
}