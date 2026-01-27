using System;
using UnityEngine;

public abstract class SelectionController : MonoBehaviour
{
    public int PlayerIndex;
    public IDirectionalTwoButtonInputHandler Navigator { get; protected set; }
    public bool IsLocked { get; protected set; }
    public bool CanAct { get; set; } = true;
    public PlayerProfile Profile { get; protected set; }
    
    protected Vector2 lastNavigateDirection;
    
    public event Action<SelectionController, bool> OnLockChanged;
    private void Update() {
        if(Navigator == null || !Navigator.IsActive() || !CanAct) return;

        if (Navigator.CancelIsPressed() && IsLocked) {
            Unlock();
            return;
        }
        
        if (IsLocked) return;
        
        HandleNavigation();
        HandleSelection();
    }

    protected abstract void HandleNavigation();

    protected static Vector2Int CalculateDiscreteDirection(Vector2 navigateDirection) {
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
