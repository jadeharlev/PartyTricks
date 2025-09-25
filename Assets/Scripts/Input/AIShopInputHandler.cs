using System.Collections;
using UnityEngine;

public class AIShopInputHandler : MonoBehaviour, IShopNavigator {
    private Vector2 currentNavigate;
    private bool selectIsPressed;

    private void OnEnable() {
        StartCoroutine(AIRoutine());
    }

    private IEnumerator AIRoutine() {
        while (true) {
            currentNavigate = GetRandomNavigationVector();
            yield return new WaitForSeconds(Random.Range(0.3f, 2f));
            
            currentNavigate = Vector2.zero;
            yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

            if (Random.value < 0.2f) {
                selectIsPressed = true;
                yield return null;
                selectIsPressed = false;
            }
        }
    }

    private Vector2 GetRandomNavigationVector() {
        int randomDirection = Random.Range(0, 4);
        switch (randomDirection) {
            case 0: return Vector2.left;
            case 1: return Vector2.right;
            case 2: return Vector2.up;
            default: return Vector2.down;
        }
    }

    public Vector2 GetNavigate() {
        return currentNavigate;
    }

    public bool SelectIsPressed() {
        return selectIsPressed;
    }

    public bool CancelIsPressed() {
        return false;
    }

    public bool IsActive() {
        return true;
    }
}
