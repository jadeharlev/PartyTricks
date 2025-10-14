// WIP: Not in use yet.
using UnityEngine;

public abstract class PowerUpEffect {
    public virtual void Use() {
        throw new System.NotImplementedException();
    }

    public virtual void OnPreShopStart(PlayerProfile profile) { }

    public virtual int ApplyCostDiscount(int baseCost) {
        return baseCost;
    }
    
    public virtual void OnMinigameStart(PlayerProfile profile) { }
}
