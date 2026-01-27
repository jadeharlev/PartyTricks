using System;
using System.Collections;
using Input;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIBetInputHandler : AIInputHandlerBase
{
    private void Awake() {
        minNavigationDuration = 0.5f;
        maxNavigationDuration = 2f;
        minIdleDuration = 0.5f;
        maxIdleDuration = 1.5f;
        selectionProbability = 0.01f;
    }

    protected override Vector2 GetRandomNavigationVector() {
        int randomDirection = Random.Range(0, 4);
        
        if (randomDirection == 3 || randomDirection == 0) {
            // AI should bet more because it's more interesting
            // Random chance to reroll if decreasing bet
            randomDirection = Random.Range(0, 4);
            if (randomDirection > 2) return Vector2.right;
        }
        
        switch (randomDirection) {
            case 0: return Vector2.left;
            case 1: return Vector2.right;
            case 2: return Vector2.up;
            default: return Vector2.down;
        }
    }
}
