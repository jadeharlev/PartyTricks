using UnityEngine;

public class ShopDiscountEffect : PowerUpEffect {
    private const int DiscountAmount = 50;
    public override int ApplyCostDiscount(int baseCost) {
        return Mathf.Max(0, baseCost - DiscountAmount);
    }
}