using CoreData;
using UnityEngine;

namespace Services {
    public class PowerUpService : MonoBehaviour, IPowerUpService {
        public GamblingModifiers GetGamblingModifiers(PlayerProfile playerProfile) {
            GamblingModifiers gamblingModifiers = new GamblingModifiers();
            foreach (var item in playerProfile.Inventory.Items) {
                switch (item.Id) {
                    case "increaseBettingAmounts":
                        gamblingModifiers.BettingAmountBoostCount++;
                        break;
                    case "increaseBettingOdds":
                        gamblingModifiers.BettingOddsBoostCount++;
                        break;
                }
            }
            return gamblingModifiers;
        }

        public MovementModifiers GetMovementModifiers(PlayerProfile playerProfile) {
            MovementModifiers movementModifiers = new MovementModifiers();
            foreach (var item in playerProfile.Inventory.Items) {
                switch (item.Id) {
                    case "magnet":
                        movementModifiers.MagnetCount++;
                        break;
                    case "moveBoost":
                        movementModifiers.MoveBoostCount++;
                        break;
                    case "increasedCoinSpawns":
                        movementModifiers.CoinSpawnRateBoostCount++;
                        break;
                    case "increasedSpecialCoinRate":
                        movementModifiers.SpecialCoinRateBoostCount++;
                        break;
                }
            }
            return movementModifiers;
        }

        public CombatModifiers GetCombatModifiers(PlayerProfile playerProfile) {
            CombatModifiers combatModifiers = new CombatModifiers();
            foreach (var item in playerProfile.Inventory.Items) {
                switch (item.Id) {
                    case "increasedHP":
                        combatModifiers.IncreasedHPCount++;
                        break;
                    case "increasedAttackSpeed":
                        combatModifiers.IncreasedAttackSpeedCount++;
                        break;
                }
            }
            return combatModifiers;
        }
    }
}