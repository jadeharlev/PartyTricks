using System;
using UnityEngine;

public class DireDodgingResultsState : IDireDodgingState {
    private Action<PlayerMinigameResult[]> OnMinigameFinished;
    private int[] playerPlaces;
    private int[] playerKills;
    private int[] baseFundsPerRank;
    private int fundsPerKill;
    public DireDodgingResultsState(int[] playerPlaces, int[] playerKills,
        Action<PlayerMinigameResult[]> OnMinigameFinished, int[] baseFundsPerRank, int fundsPerKill) {
        this.OnMinigameFinished = OnMinigameFinished;
        this.playerPlaces = playerPlaces;
        this.playerKills = playerKills;
        this.baseFundsPerRank = baseFundsPerRank;
        this.fundsPerKill = fundsPerKill;
    }

    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Results State.", LogLevel.Verbose);
        CalculateMinigameResults();
    }

    private void CalculateMinigameResults() {
        PlayerMinigameResult[] playerResults = new PlayerMinigameResult[4];
        for (int i = 0; i < playerResults.Length; i++) {
            Debug.Log("Calculating results for player " + (i + 1) +":");
            int playerRank = playerPlaces[i]-1;
            int baseFundsEarned = baseFundsPerRank[playerRank];
            Debug.Log("Rank was " + (playerPlaces[i]+1) +" and base funds earned were " + baseFundsEarned);
            baseFundsEarned += playerKills[i] * fundsPerKill;
            Debug.Log("Final funds earned were " + baseFundsEarned);
            playerResults[i] = new PlayerMinigameResult(i, playerRank, baseFundsEarned);
        }
    }

    public void OnUpdate() {

    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Results State.", LogLevel.Verbose);
    }
}