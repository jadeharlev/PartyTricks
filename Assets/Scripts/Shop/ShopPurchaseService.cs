using System.Collections.Generic;
using UnityEngine;

public class ShopPurchaseService {
    public void ResolvePurchases(List<ShopSlotSelector> shopSlotSelectors, ShopItemUI[] shopItems) {
        Debug.Log("Shop closed! Resolving purchases.");
        foreach (var playerSelector in shopSlotSelectors) {
            int index = playerSelector.CurrentShopItemIndex;
            playerSelector.Lock();
            playerSelector.CanAct = false;
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " buys " + shopItems[index].ToString());
        }
    }
}