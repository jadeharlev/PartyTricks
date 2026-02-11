using UnityEngine;

public interface IDirectionalTwoButtonInputHandler {
    Vector2 GetNavigate();
    bool SelectIsPressed();
    bool CancelIsPressed();
    bool IsActive();
    
    // Add these for charged attack
    bool ChargeIsPressed();
    bool ChargeIsHeld();
    bool ChargeIsReleased();
}