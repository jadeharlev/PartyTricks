using System.Collections.Generic;
using CoreData;
using Services;
using UnityEngine;

public class ShopPurchaseService {
    public void ResolvePurchases(List<ShopSlotSelector> shopSlotSelectors, ShopItemUI[] shopItems) {
        foreach (var playerSelector in shopSlotSelectors) {
            ProcessPurchase(shopItems, playerSelector);
        }
    }

    private static void ProcessPurchase(ShopItemUI[] shopItems, ShopSlotSelector playerSelector) {
        int index = playerSelector.CurrentShopItemIndex;
        IPlayerService playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        playerSelector.Lock();
        playerSelector.CanAct = false;

        PlayerProfile profile = playerService.GetPlayerProfile(playerSelector.PlayerIndex);
        ShopItemUI item = shopItems[index];
        
        int baseCost = item.GetItemCost();
        var powerUpService = ServiceLocatorAccessor.GetService<IPowerUpService>();
        ShopModifiers shopModifiers = powerUpService.GetShopModifiers(profile);
        int finalCost = shopModifiers.ApplyDiscount(baseCost);
        bool purchaseSuccess = profile.Wallet.Buy(finalCost);
        if (purchaseSuccess) {
            profile.Inventory.AddItem(item.GetItem().ToDefinition());
        }
        else {
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " could not afford " + shopItems[index].ToString());
        }
    }
}