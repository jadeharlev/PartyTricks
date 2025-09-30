using UnityEngine;

public class ShopNavigationService {
    private readonly int gridRows;
    private readonly int gridColumns;

    public ShopNavigationService(int gridRows, int gridColumns) {
        this.gridRows = gridRows;
        this.gridColumns = gridColumns;
    }

    public int Move(int currentIndex, Vector2Int direction) {
        int row = currentIndex / gridColumns;
        int column = currentIndex % gridColumns;
        row = Mathf.Clamp(row-direction.y, 0, gridRows - 1);
        column = Mathf.Clamp(column+direction.x, 0, gridColumns - 1);
        return row*gridColumns + column;
    }
}