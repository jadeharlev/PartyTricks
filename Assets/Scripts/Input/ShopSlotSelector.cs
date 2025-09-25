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

    public int GridColumns = 2;
    public int GridRows = 2;
    private Vector2 lastNavigateDirection;
    public bool CanAct;

    public void Initialize(int index, IShopNavigator navigator) {
        PlayerIndex = index;
        Navigator = navigator;
        CurrentShopItemIndex = 0;
        IsLocked = false;
        CanAct = true;
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
        var (x, y) = CalculateShopGridPosition(discreteDirection);
        CurrentShopItemIndex = y * GridColumns + x;
        OnSelectionChanged?.Invoke(this, CurrentShopItemIndex);
    }

    private (int x, int y) CalculateShopGridPosition(Vector2 discreteDirection) {
        int x = CurrentShopItemIndex % GridColumns;
        int y = CurrentShopItemIndex / GridColumns;
        if (discreteDirection.x < 0) x = Mathf.Clamp(x - 1, 0, GridColumns - 1);
        if (discreteDirection.x > 0) x = Mathf.Clamp(x + 1, 0, GridColumns - 1);
        if (discreteDirection.y > 0) y = Mathf.Clamp(y - 1, 0, GridRows - 1);
        if (discreteDirection.y < 0) y = Mathf.Clamp(y + 1, 0, GridRows - 1);
        return (x, y);
    }

    private static Vector2 CalculateDiscreteDirection(Vector2 navigateDirection) {
        Vector2 discreteDirection = Vector2.zero;
        if(navigateDirection.x != 0) discreteDirection.x = Mathf.Sign(navigateDirection.x);
        if(navigateDirection.y != 0) discreteDirection.y = Mathf.Sign(navigateDirection.y);
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

    private void Unlock() {
        IsLocked = false;
        OnLockChanged?.Invoke(this, false);
    }

}
