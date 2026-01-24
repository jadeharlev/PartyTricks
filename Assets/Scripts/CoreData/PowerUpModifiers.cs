using UnityEngine;

namespace CoreData {
    public struct GamblingModifiers {
        public int BettingAmountBoostCount;
        public int BettingOddsBoostCount;
    }
    
    public struct MovementModifiers {
        public int MagnetCount;
        public int MoveBoostCount;
        public int CoinSpawnRateBoostCount;
        public int SpecialCoinRateBoostCount;
    }

    public struct CombatModifiers {
        public int IncreasedHPCount;
        public int IncreasedAttackSpeedCount;
    }

    public struct ShopModifiers {
        public readonly int ShopDiscountCount;
        private const int DISCOUNT_PER_ITEM = 50;

        public ShopModifiers(int shopDiscountCount) {
            ShopDiscountCount = shopDiscountCount;
        }

        public int ApplyDiscount(int baseCost) {
            return Mathf.Max(0, baseCost - (ShopDiscountCount*DISCOUNT_PER_ITEM));
        }
    }
}