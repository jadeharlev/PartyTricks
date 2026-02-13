using System.Collections.Generic;

namespace VineSwinging.Core {
    public static class ResultsCalculator {
        public static int[] CalculateRanks(int[] scores) {
            var playerRankings = new List<(int index, int score)>();
            for (int i = 0; i < scores.Length; i++) {
                playerRankings.Add((i, scores[i]));
            }
            playerRankings.Sort((a, b) => b.score.CompareTo(a.score));
            int[] ranks = new int[scores.Length];
            for (int i = 0; i < playerRankings.Count; i++) {
                if (i > 0 && playerRankings[i].score == playerRankings[i - 1].score) {
                    ranks[playerRankings[i].index] = ranks[playerRankings[i - 1].index];
                }
                else {
                    ranks[playerRankings[i].index] = i;
                }
            }
            return ranks;
        }
    }
}