using UnityEngine;

public static class BetInputService
{
    public static int GetDelta(Vector2Int direction) {
        var xDirection = direction.x;
        var yDirection = direction.y;
        switch (xDirection) {
            case > 0:
                return 10;
            case < 0:
                return -10;
        }

        switch (yDirection) {
            case > 0:
                return 1;
            case < 0:
                return -1;
        }

        return 0;
    }
}
