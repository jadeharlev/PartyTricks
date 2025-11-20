using System;

public class DireDodgingResultsState : IDireDodgingState {
    private Action<PlayerMinigameResult[]> OnMinigameFinished;
    private int[] playerPlaces;
    private int[] playerKills;
    private int[] baseFundsPerRank;
    private int fundsPerKill;
    private PlacesDisplay placesDisplay;
    public DireDodgingResultsState(int[] playerPlaces, int[] playerKills, int[] baseFundsPerRank, int fundsPerKill,
        PlacesDisplay placesDisplay) {
        this.playerPlaces = playerPlaces;
        this.playerKills = playerKills;
        this.baseFundsPerRank = baseFundsPerRank;
        this.fundsPerKill = fundsPerKill;
        this.placesDisplay = placesDisplay;
    }

    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Results State.", LogLevel.Verbose);
        PlayerMinigameResult[] results = CalculateMinigameResults();
        UpdatePlaceDisplays(results);
        placesDisplay.Show();
        DireDodgingMinigameManager.Instance.OnGameEnd(results);
    }

    private void UpdatePlaceDisplays(PlayerMinigameResult[] results) {
        string[] playerPlaceInfo = new string[4];
        for (int i = 0; i<results.Length; i++) {
            PlayerMinigameResult result = results[i];
            playerPlaceInfo[i] = GetPlaceDisplayString(result);
        }
        placesDisplay.UpdateTextObjects(playerPlaceInfo);
    }

    private string GetPlaceDisplayString(PlayerMinigameResult result) {
        string placeDisplayString = "";
        int playerIndex = result.PlayerIndex;
        var placeAsText = GetPlaceAsText(result.PlayerPlace);
        placeDisplayString += placeAsText + "\n<size=50>";
        placeDisplayString += baseFundsPerRank[result.PlayerPlace - 1];
        var kills = playerKills[playerIndex];
        if (kills >= 1) {
            placeDisplayString += " + " + (kills * fundsPerKill);
        }

        placeDisplayString += " Funds";
        placeDisplayString += "</size>\n<size=30>(" + placeAsText + " Place";
        if (kills >= 1) {
            placeDisplayString += ", " + kills + " Elimination";
            if (kills > 1) {
                placeDisplayString += "s";
            }
        }
        placeDisplayString += ")</size>";
        return placeDisplayString;
    }

    private static string GetPlaceAsText(int place) {
        return place switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            4 => "4th",
            _ => "ERR"
        };
    }

    private PlayerMinigameResult[] CalculateMinigameResults() {
        PlayerMinigameResult[] playerResults = new PlayerMinigameResult[4];
        for (int playerIndex = 0; playerIndex < playerResults.Length; playerIndex++) {
            int playerRank = playerPlaces[playerIndex]-1;
            int baseFundsEarned = baseFundsPerRank[playerRank];
            int eliminationMoney = playerKills[playerIndex] * fundsPerKill;
            baseFundsEarned += eliminationMoney;
            if (DireDodgingMinigameManager.Instance.IsDoubleRound) baseFundsEarned *= 2;
            playerResults[playerIndex] = new PlayerMinigameResult(playerIndex, playerRank, baseFundsEarned, 0,playerKills[playerIndex]);
        }
        return playerResults;
    }

    public void OnUpdate() {

    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Results State.", LogLevel.Verbose);
    }
}