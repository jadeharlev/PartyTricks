using Services;
using VineSwinging.Core;

namespace Minigames.Swinging.States {
    public class VineSwingingResultsState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;

        public VineSwingingResultsState(VineSwingingMinigameManager minigameManager) {
            this.minigameManager = minigameManager;
        }
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Results State.");
            PlayerMinigameResult[] results = CalculateResults();
            ShowResultsDisplay(results);
            minigameManager.OnGameEnd(results);
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

        private PlayerMinigameResult[] CalculateResults() {
            int[] scores = new int[4];
            for (int i = 0; i < 4; i++) {
                scores[i] = minigameManager.PlayerStateMachines[i].PlayerContext.FurthestVineIndex;
            }

            int[] ranks = ResultsCalculator.CalculateRanks(scores);
            int[] fundsPerRank = minigameManager.FundsPerRank;
            if (minigameManager.IsDoubleRound) {
                fundsPerRank = (int[])fundsPerRank.Clone();
                for (int i = 0; i < fundsPerRank.Length; i++) {
                    fundsPerRank[i] *= 2;
                }
            }

            var results = new PlayerMinigameResult[4];
            for (int i = 0; i < 4; i++) {
                results[i] = new PlayerMinigameResult(i, ranks[i], fundsPerRank[ranks[i]]);
            }
            return results;
        }

        private void ShowResultsDisplay(PlayerMinigameResult[] results) {
            IPlayerService playerService = minigameManager.PlayerService;
            string[] resultsText = new string[4];
            for (int i = 0; i < results.Length; i++) {
                int fundsEarned = results[i].BaseFundsEarned;
                int currentFunds = playerService.PlayerSlots[i].Profile.Wallet.GetCurrentFunds();
                int newFunds = currentFunds + fundsEarned;
                resultsText[i] = GetPlaceAsText(results[i].PlayerPlace)
                    + "\n<size=50>+" + fundsEarned + " funds </size>"
                    + "\n<size=30>New funds: " + newFunds + "</size>";
            }
            minigameManager.PlacesDisplay.UpdateTextObjects(resultsText);
            minigameManager.PlacesDisplay.Show();
        }

        public void OnUpdate() {

        }

        public void Exit() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Exited Results State.");
        }
    }
}