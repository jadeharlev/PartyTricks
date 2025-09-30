using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotSelector : MonoBehaviour {
    public int PlayerIndex;
    public IShopNavigator Navigator { get; private set; }
    public int CurrentShopItemIndex { get; private set; }
    public bool IsLocked { get; private set; }
    public event Action<ShopSlotSelector, int> OnSelectionChanged;
    public event Action<ShopSlotSelector, bool> OnLockChanged;
    private Vector2 lastNavigateDirection;
    private ShopNavigationService shopNavigationService;
    public PlayerProfile Profile { get; private set; }
    public bool CanAct;

    public void Initialize(int index, IShopNavigator navigator, ShopNavigationService shopNavigationService, PlayerProfile profile, int currentShopIndex = 0) {
        PlayerIndex = index;
        Navigator = navigator;
        CurrentShopItemIndex = currentShopIndex;
        IsLocked = false;
        CanAct = true;
        Profile = profile;
        this.shopNavigationService = shopNavigationService;
    }

    private void Update() {
        if (!CanAct) return;
        if (Navigator == null || !Navigator.IsActive()) return;
        Vector2 navigateDirection = Navigator.GetNavigate();
        if(navigateDirection != Vector2.zero) {
            HandleNavigation(navigateDirection);
        }
        HandleSelection();
    }

    private void HandleNavigation(Vector2 navigateDirection) {
        var discreteDirection = CalculateDiscreteDirection(navigateDirection);
        if(discreteDirection == Vector2.zero) {
            lastNavigateDirection = Vector2.zero;
            return;
        }
        bool moveDirectionIsNew = (discreteDirection != lastNavigateDirection);
        if (!moveDirectionIsNew || IsLocked) return;
        lastNavigateDirection = discreteDirection;
        CurrentShopItemIndex = shopNavigationService.Move(CurrentShopItemIndex, discreteDirection);
        OnSelectionChanged?.Invoke(this, CurrentShopItemIndex);
    }

    private static Vector2Int CalculateDiscreteDirection(Vector2 navigateDirection) {
        Vector2Int discreteDirection = Vector2Int.zero;
        if(navigateDirection.x != 0) discreteDirection.x = (int)Mathf.Sign(navigateDirection.x);
        if(navigateDirection.y != 0) discreteDirection.y = (int)Mathf.Sign(navigateDirection.y);
        return discreteDirection;
    }

    private void HandleSelection() {
        if (Navigator.SelectIsPressed()) {
            Lock();
        }

        if (Navigator.CancelIsPressed()) {
            Unlock();
        }
    }

    public void Lock() {
        IsLocked = true;
        OnLockChanged?.Invoke(this, true);
    }

    public void Unlock() {
        IsLocked = false;
        OnLockChanged?.Invoke(this, false);
    }

}
