using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bets : MonoBehaviour {
    [SerializeField] 
    private BetCard[] betCards;
    private BetPlayerManager playerManager;
    public int AllowedBettingDurationInSeconds = 10;
    [SerializeField] 
    private MinigameTimer MinigameTimer;
    [FormerlySerializedAs("playerCornerDisplays")] [SerializeField]
    private PlayerCornerDisplay[] PlayerCornerDisplays;
    private IGamblingMinigame gamblingMinigameManager;

    private void Awake() {
        gamblingMinigameManager = GetComponent<IGamblingMinigame>();
        if (gamblingMinigameManager == null) {
            Debug.LogError("Error: IGamblingMinigame not found on Bets GameObject");
        }

        if (MinigameTimer == null) {
            Debug.LogError("Error: MinigameTimer not found on Bets GameObject");
        }
        MinigameTimer.Initialize(AllowedBettingDurationInSeconds);
    }

    private void Start() {
        StartCoroutine(WaitForManagerAndStart());
    }

    private IEnumerator WaitForManagerAndStart() {
        yield return new WaitUntil(() => gamblingMinigameManager != null);
        InitializeComponents();
        StartBets();
    }

    private void InitializeComponents() {
        playerManager = new BetPlayerManager(betCards, PlayerCornerDisplays);
        MinigameTimer.OnTimerEnd += OnBetTimerEnd;
    }

    private void StartBets() {
        playerManager.InitializePlayers();
        MinigameTimer.StartTimer();
    }

    private void OnBetTimerEnd() {
        Debug.Log("Timer over!");
        playerManager.LockAllSelectors();

        Dictionary<int, int> bets = playerManager.GetPlayerBets();

        StartCoroutine(WaitAndContinue(bets));
    }

    private IEnumerator WaitAndContinue(Dictionary<int, int> bets) {
        yield return new WaitForSeconds(3);
        if (gamblingMinigameManager != null && gamblingMinigameManager.OnBetTimerEnd != null) {
            gamblingMinigameManager.OnBetTimerEnd?.Invoke(bets);
        }
        else {
            Debug.LogError("OnBetTimerEnd event is not set up correctly.");
        }
    }

    public void Reset() {
        MinigameTimer.StopIfRunning();
        MinigameTimer.StartTimer();
        playerManager.EnableAllSelectors();
    }
    
    public void UnlockAISelectors() {
        playerManager.UnlockAISelectors();
    }
    

    private void OnDestroy() {
        if (MinigameTimer != null) {
            MinigameTimer.OnTimerEnd -= OnBetTimerEnd;
        }
        playerManager?.Cleanup();
    }
}
