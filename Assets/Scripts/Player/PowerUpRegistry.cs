using System;
using System.Collections.Generic;
using UnityEngine;

public static class PowerUpRegistry {
    private static readonly Dictionary<string, Type> effectMap = new Dictionary<string, Type>
    {
        { "shopDiscount", typeof(ShopDiscountEffect) },
        { "increaseBettingAmounts", typeof(NoEffect) },
        { "increaseBettingOdds", typeof(NoEffect) },
        { "magnet", typeof(NoEffect) },
        { "speedBoost", typeof(NoEffect) },
        { "emptyItem", typeof(NoEffect) },
    };

    public static PowerUpEffect CreateEffect(string id) {
        if (effectMap.TryGetValue(id, out Type effectType)) {
            try {
                // Make sure that the type inherits from PowerUpEffect, and has an empty constructor
                return (PowerUpEffect)Activator.CreateInstance(effectType);
            } catch (Exception e) {
                Debug.LogError($"PowerupRegistry: Failed to instantiate effect {id} of type {effectType.Name}. Exception: {e.Message}");
                return null;
            }
        }
        return new NullPowerUpEffect();
    }
    private class NullPowerUpEffect : PowerUpEffect {}
}
