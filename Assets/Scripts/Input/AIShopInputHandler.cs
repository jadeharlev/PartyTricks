using System.Collections;
using Input;
using UnityEngine;

public class AIShopInputHandler : AIInputHandlerBase {
    private void Awake() {
        minNavigationDuration = 0.3f;
        maxNavigationDuration = 2f;
        minIdleDuration = 0.2f;
        maxIdleDuration = 1.5f;
        selectionProbability = 0.2f;
    }

    protected override Vector2 GetRandomNavigationVector() {
        int randomDirection = Random.Range(0, 4);
        return randomDirection switch
        {
            0 => Vector2.left,
            1 => Vector2.right,
            2 => Vector2.up,
            _ => Vector2.down
        };
    }
}
