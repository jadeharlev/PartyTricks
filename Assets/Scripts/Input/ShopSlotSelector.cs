using System;
using UnityEngine;

public class ShopSlotSelector : SelectionController {
    public event Action<ShopSlotSelector, int> OnSelectionChanged;
    public int CurrentShopItemIndex { get; private set; }
    private ShopNavigationService shopNavigationService;

    public void Initialize(int index, IDirectionalTwoButtonInputHandler navigator, ShopNavigationService shopNavigationService, PlayerProfile profile, int currentShopIndex = 0) {
        PlayerIndex = index;
        Navigator = navigator;
        CurrentShopItemIndex = currentShopIndex;
        Profile = profile;
        this.shopNavigationService = shopNavigationService;
    }
    
    protected override void HandleNavigation() {
        var direction = Navigator.GetNavigate();
        var discreteDirection = CalculateDiscreteDirection(direction);
        
        bool moveDirectionIsNew = (discreteDirection != lastNavigateDirection);
        if (discreteDirection != Vector2.zero && moveDirectionIsNew) {
            CurrentShopItemIndex = shopNavigationService.Move(CurrentShopItemIndex, discreteDirection);
            OnSelectionChanged?.Invoke(this, CurrentShopItemIndex);
        }
        
        lastNavigateDirection = discreteDirection;
    }

}
