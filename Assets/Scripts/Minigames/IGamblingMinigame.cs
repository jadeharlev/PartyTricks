using System;
using System.Collections.Generic;

public interface IGamblingMinigame : IMinigameManager {
    void SetPlayerBets(int[] bets);
    public Action<Dictionary<int, int>> OnBetTimerEnd { get; set; }
}