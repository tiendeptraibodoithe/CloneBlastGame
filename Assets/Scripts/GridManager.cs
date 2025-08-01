using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;  // Số ô theo chiều ngang
    public int height = 10; // Số ô theo chiều dọc
    public float cellSize = 1f; // Kích thước mỗi ô
    public Transform[,] grid; // Mảng 2D lưu trữ vật thể
    

    [Header("Debug")]
    public bool showGrid = true; // Hiển thị lưới trong Scene View
    public Color gridColor = Color.white;

    [Header("Grid Offset")]
    public float zOffset = 0f;

    protected virtual void Awake()
    {
        grid = new Transform[width, height];
        GenerateGrid();
    }

    protected virtual void Start() { }

    // Tạo lưới trong Scene (chỉ để debug)
    void GenerateGrid()
    {
        if (!showGrid) return;

        for (int x = 0; x <= width; x++)
        {
            Debug.DrawLine(
                new Vector3(x * cellSize - (width * cellSize * 0.5f), 0, -(height * cellSize * 0.5f) + zOffset),
                new Vector3(x * cellSize - (width * cellSize * 0.5f), 0, (height * cellSize * 0.5f) + zOffset),
                gridColor, 100f
            );
        }

        for (int y = 0; y <= height; y++)
        {
            Debug.DrawLine(
                new Vector3(-(width * cellSize * 0.5f), 0, y * cellSize - (height * cellSize * 0.5f) + zOffset),
                new Vector3((width * cellSize * 0.5f), 0, y * cellSize - (height * cellSize * 0.5f) + zOffset),
                gridColor, 100f
            );
        }
    }

    // Chuyển tọa độ lưới sang vị trí thế giới (World Position)
    public virtual Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(
            x * cellSize - (width * cellSize * 0.5f) + cellSize * 0.5f,
            0,
            y * cellSize - (height * cellSize * 0.5f) + cellSize * 0.5f + zOffset
        );
    }

    // Đặt vật thể vào lưới
    public void PlaceObject(int x, int y, Transform objectToPlace)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            grid[x, y] = objectToPlace;
            objectToPlace.position = GetWorldPosition(x, y); // Đã bao gồm zOffset
        }
    }

    public Transform GetObjectAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y];
        }
        return null;
    }
}