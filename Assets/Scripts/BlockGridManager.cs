using UnityEngine;
using System.Collections.Generic;

public class BlockGridManager : GridManager
{
    [Header("Block Types")]
    public List<BlockType> blockTypes;
    public Transform[,] grid;

    private void Awake()
    {
        grid = new Transform[width, height]; // Khởi tạo lưới
    }

    public void ClearCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            grid[x, y] = null;
        }
    }

    public void MoveObject(int fromX, int fromY, int toX, int toY)
    {
        if (!IsValid(fromX, fromY) || !IsValid(toX, toY)) return;
        Transform obj = grid[fromX, fromY];
        if (obj != null)
        {
            grid[toX, toY] = obj;
            grid[fromX, fromY] = null;
            obj.transform.position = GetWorldPosition(toX, toY);
        }
    }

    public Transform GetObjectAt(int x, int y)
    {
        if (IsValid(x, y))
            return grid[x, y];
        return null;
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public override Vector3 GetWorldPosition(int x, int y)
    {
        float xOffset = -(width * cellSize) / 2f + cellSize / 2f; // Căn giữa theo X
        return new Vector3(x * cellSize + xOffset, 0, y * cellSize + zOffset);
    }


    public Block SpawnBlock(int x, int y, BlockType type)
    {
        GameObject block = Instantiate(type.prefab, GetWorldPosition(x, y), Quaternion.identity);
        Block blockComponent = block.GetComponent<Block>();
        if (blockComponent == null)
        {
            Debug.LogError("Prefab không có component Block.");
            return null;
        }

        // Gọi hàm khởi tạo đầy đủ, sẽ set cả freeze + subBlock + floor
        blockComponent.Initialize(type);

        // Nếu blockType bị đóng băng, khóa block lại
        if (type.isFrozen)
        {
            blockComponent.LockBlock(type.unlockAfterShooterCount);
        }

        PlaceObject(x, y, block.transform);
        return blockComponent;
    }


    public void PlaceObject(int x, int y, Transform obj)
    {
        if (IsValid(x, y))
        {
            grid[x, y] = obj;
        }
    }

    public void DropColumnDown(int x, int destroyedY)
    {
        // Di chuyển tất cả blocks phía trên vị trí bị phá huỷ xuống dưới
        for (int y = destroyedY + 1; y < height; y++)
        {
            Transform above = GetObjectAt(x, y);
            if (above != null)
            {
                // Di chuyển block xuống vị trí trống
                MoveObject(x, y, x, y - 1);
            }
        }

        // Tiếp tục kiểm tra và di chuyển cho đến khi không còn khoảng trống
        bool hasGaps = true;
        while (hasGaps)
        {
            hasGaps = false;
            for (int y = 0; y < height - 1; y++)
            {
                if (GetObjectAt(x, y) == null && GetObjectAt(x, y + 1) != null)
                {
                    // Có khoảng trống, di chuyển block xuống
                    MoveObject(x, y + 1, x, y);
                    hasGaps = true;
                }
            }
        }
    }

}