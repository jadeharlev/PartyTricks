using UnityEngine;

public class EconomyService : MonoBehaviour {
    public static EconomyService Instance { get; private set; }

    [Header("Reward Configuration")] 
    [SerializeField]
    private int baseRewardFirstPlace = 100;
    [SerializeField]
    private int baseRewardSecondPlace = 80;
    [SerializeField]
    private int baseRewardThirdPlace = 60;
    [SerializeField]
    private int baseRewardFourthPlace = 50;

    [Tooltip("Use these numbers if the minigame doesn't report funds.")]
    [SerializeField] private bool useRankFallback = true;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyRewards(PlayerMinigameResult[] results) {
        if (GameSessionManager.Instance == null) {
            Debug.LogError("EconomyService: GameSessionManager not found.");
            return;
        }

        foreach (var result in results) {
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[result.PlayerIndex];
            if (slot == null || slot.Profile == null) {
                Debug.LogError($"Error: Slot or profile missing for player {result.PlayerIndex}.");
                continue;
            }

            int fundsEarned = 0;
            bool wasGamblingMinigame = (result.AmountBet > 0);
            if (wasGamblingMinigame) {
                fundsEarned = result.BaseFundsEarned;
                int netFundsEarned = result.NetFundsEarned;
                string resultText = netFundsEarned.ToString();
                if (netFundsEarned >= 0) {
                    resultText = $"-{netFundsEarned}";
                }
                
                DebugLogger.Log(LogChannel.Systems, $"Player {result.PlayerIndex} - Bet {result.AmountBet}. Payout: " 
                                                    + $"{fundsEarned}, Net: {resultText}");
            } else if (result.BaseFundsEarned > 0) {
                fundsEarned = result.BaseFundsEarned;
            } else if (useRankFallback) {
                fundsEarned = GetFundsByRank(result.PlayerPlace);
            }

            bool isDoubleRound = GameFlowManager.Instance.GetCurrentRoundDefinition().IsDouble;
            if (fundsEarned > 0) {
                slot.Profile.Wallet.AddFunds(fundsEarned);
                DebugLogger.Log(LogChannel.Systems, $"Player {result.PlayerIndex} earned {fundsEarned} ({result.PlayerPlace} place). Double round: {isDoubleRound}");
            }
        }
    }

    private int GetFundsByRank(int resultPlayerPlace) {
        return resultPlayerPlace switch
        {
            1 => baseRewardFirstPlace,
            2 => baseRewardSecondPlace,
            3 => baseRewardThirdPlace,
            4 => baseRewardFourthPlace,
            _ => 0,
        };
    }
}