using UnityEngine;

public interface IShopNavigator {
    Vector2 GetNavigate();
    bool SelectIsPressed();
    bool CancelIsPressed();
    bool IsActive();
}
