using UnityEngine;

public interface IDirectionalTwoButtonInputHandler {
    Vector2 GetNavigate();
    bool SelectIsPressed();
    bool CancelIsPressed();
    bool IsActive();
}
