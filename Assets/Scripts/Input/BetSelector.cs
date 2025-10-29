using System;
using UnityEngine;

public class BetSelector : MonoBehaviour
{
    public int PlayerIndex;
    public IDirectionalTwoButtonInputHandler Navigator { get; private set; }
    public bool IsLocked { get; private set; }
    public event Action<BetSelector, int> OnSelectionChanged;
    public event Action<BetSelector, bool> OnLockChanged;
    private Vector2 lastNavigateDirection;
    public PlayerProfile Profile { get; private set; }
    public bool CanAct;
    
    private float inputTimer = 0f;
    private float inputInterval = 0.10f;
    
    public void Initialize(int index, IDirectionalTwoButtonInputHandler navigator, PlayerProfile profile) {
        PlayerIndex = index;
        Navigator = navigator;
        IsLocked = false;
        CanAct = true;
        Profile = profile;
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
            inputTimer = 0f;
            return;
        }
        
        if(IsLocked) return;
        
        bool directionChanged = (discreteDirection != lastNavigateDirection);
        inputTimer += Time.deltaTime;
    
        if (directionChanged || inputTimer >= inputInterval) {
            lastNavigateDirection = discreteDirection;
            inputTimer = 0f;
            int delta = BetInputService.GetDelta(discreteDirection);
            OnSelectionChanged?.Invoke(this, delta);
        }
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
