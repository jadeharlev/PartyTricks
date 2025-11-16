using System;
using UnityEngine;

public class PlayerProfile {
    public Inventory Inventory;
    public Wallet Wallet;
    public PlayerProfile(int startingFunds) {
        this.Wallet = new Wallet(startingFunds);
        this.Inventory = new Inventory();
    }

    public void Reset() {
        Wallet.Reset();
        Inventory.Reset();
    }
}
