using System;
using UnityEngine;

public class DireDodgingResultsState : IDireDodgingState {
    private Action<PlayerMinigameResult[]> OnMinigameFinished;
    private int[] playerPlaces;
    private int[] playerKills;
    private int[] baseFundsPerRank;
    private int fundsPerKill;
    public DireDodgingResultsState(int[] playerPlaces, int[] playerKills, int[] baseFundsPerRank, int fundsPerKill) {
        this.playerPlaces = playerPlaces;
        this.playerKills = playerKills;
        this.baseFundsPerRank = baseFundsPerRank;
        this.fundsPerKill = fundsPerKill;
    }

    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Results State.", LogLevel.Verbose);
        PlayerMinigameResult[] results = CalculateMinigameResults();
        DireDodgingMinigameManager.Instance.OnGameEnd(results);
    }

    private PlayerMinigameResult[] CalculateMinigameResults() {
        PlayerMinigameResult[] playerResults = new PlayerMinigameResult[4];
        for (int playerIndex = 0; playerIndex < playerResults.Length; playerIndex++) {
            int playerRank = playerPlaces[playerIndex]-1;
            int baseFundsEarned = baseFundsPerRank[playerRank];
            int eliminationMoney = playerKills[playerIndex] * fundsPerKill;
            baseFundsEarned += eliminationMoney;
            if (DireDodgingMinigameManager.Instance.IsDoubleRound) baseFundsEarned *= 2;
            playerResults[playerIndex] = new PlayerMinigameResult(playerIndex, playerRank, baseFundsEarned);
        }
        return playerResults;
    }

    public void OnUpdate() {

    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Results State.", LogLevel.Verbose);
    }
}