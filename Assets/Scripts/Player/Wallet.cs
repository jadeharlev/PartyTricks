using System;
using UnityEngine;

public class Wallet {
    private int currentFunds;
    public event Action<int> OnFundsChanged;
    public Wallet(int startingFunds) {
        this.currentFunds = startingFunds;
    }

    public int GetCurrentFunds() {
        return this.currentFunds;
    }

    public bool Buy(int itemCost) {
        if (CanPurchase(itemCost)) {
            currentFunds -= itemCost;
            OnFundsChanged?.Invoke(currentFunds);
            return true;
        }

        return false;
    }
    public bool CanPurchase(int cost) {
        return currentFunds >= cost;
    }

    public void AddFunds(int amount) {
        currentFunds += amount;
        OnFundsChanged?.Invoke(currentFunds);
    }
}
