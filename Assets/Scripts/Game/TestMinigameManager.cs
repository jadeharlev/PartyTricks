// test minigame: gives each player a random place.

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TestMinigameManager : MonoBehaviour, IMinigameManager {
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    [SerializeField] private bool isDoubleRound = false;
    public bool IsDoubleRound => isDoubleRound;

    public bool HasBeenInitialized { get; private set; } = false;

    private void Start() {
        StartCoroutine(SimulateGameFinish());
    }
    
    public void Initialize(bool isDoubleRound) {
        this.isDoubleRound = isDoubleRound;
        HasBeenInitialized = true;
    }

    private IEnumerator SimulateGameFinish() {
        float delayBeforeResultsShown = 5f;
        if (isDoubleRound) delayBeforeResultsShown *= 2;
        yield return new WaitForSeconds(delayBeforeResultsShown);
        DebugLogger.Log(LogChannel.Systems, $"Simulating game finish. IsDoubleRound: {isDoubleRound}.");

        var results = new PlayerMinigameResult[4];
        int fundsPerRank = 25;
        var ranks = Enumerable.Range(0, 4).OrderBy(x => Guid.NewGuid()).ToArray();
        for (int i = 0; i < 4; i++) {
            int playerIndex = i;
            int rank = ranks[i];
            int baseFunds = fundsPerRank * (4 - rank);
            int finalFunds = baseFunds;
            if (IsDoubleRound) {
                finalFunds *= 2;
                DebugLogger.Log(LogChannel.Systems, $"Double funds for player {playerIndex} to {finalFunds}.");
            }
            results[i] = new PlayerMinigameResult(playerIndex, rank, finalFunds);
        }
        OnMinigameFinished?.Invoke(results);
    }
}