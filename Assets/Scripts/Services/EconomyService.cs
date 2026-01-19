using UnityEngine;

namespace Services {
    public class EconomyService : MonoBehaviour, IEconomyService {
        [SerializeField] private int[] placeRewards = { 150, 100, 50, 25 };
        private IPlayerService playerService;

        private void Awake() {
            playerService = GetComponent<IPlayerService>();
        }
        
        public void ApplyRewards(PlayerMinigameResult[] results) {
            if (playerService == null) {
                playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            }

            foreach (var result in results) {
                var profile = playerService.GetPlayerProfile(result.PlayerIndex);
                if (profile == null) {
                    DebugLogger.Log(LogChannel.Systems, "No profile found for player " + result.PlayerIndex, LogLevel.Warning);
                    continue;
                }

                int fundsEarned = 0;
                bool wasGamblingMinigame = result.AmountBet > 0;
                if (wasGamblingMinigame || result.BaseFundsEarned > 0) {
                    fundsEarned = result.BaseFundsEarned;
                } else if (result.PlayerPlace <= placeRewards.Length) {
                    fundsEarned += placeRewards[result.PlayerPlace-1];
                }
                
                profile.Wallet.AddFunds(fundsEarned);
                
                DebugLogger.Log(LogChannel.Systems, $"Player {result.PlayerIndex} received {fundsEarned} funds (Place: {result.PlayerPlace})");
            }
        }

        public int GetRewardForPlace(int place) {
            if (place < 1 || place > placeRewards.Length) return 0;
            return placeRewards[place-1];
        }
    }
}