using UnityEngine;
using System.Collections.Generic;

public class ShooterGridManager : GridManager
{
    [Header("Shooter Types")]
    public List<ShooterType> shooterTypes;

    public void SpawnShooter(int x, int y, ShooterType type)
    {
        GameObject shooter = Instantiate(type.prefab, GetWorldPosition(x, y), Quaternion.identity);
        var selector = shooter.GetComponent<ShooterSelector>();
        if (selector != null)
        {
            selector.gridX = x;
            selector.gridY = y;
        }

        Shooter shooterComponent = shooter.GetComponent<Shooter>();
        if (shooterComponent != null)
        {
            shooterComponent.SetType(type);

            // Reveal ngay nếu ở hàng đầu tiên
            if (y == 0 && type.specialType == ShooterSpecialType.Secret)
            {
                shooterComponent.RevealSecretShooter();
            }
        }

        PlaceObject(x, y, shooter.transform);
    }

    public void ClearCell(int x, int y)
    {
        grid[x, y] = null;
    }

    // Move shooter object from one cell to another
    public void MoveObject(int fromX, int fromY, int toX, int toY)
    {
        Transform obj = GetObjectAt(fromX, fromY);
        if (obj != null)
        {
            grid[toX, toY] = obj;
            grid[fromX, fromY] = null;
            obj.position = GetWorldPosition(toX, toY);

            // Cập nhật thông tin vị trí
            var selector = obj.GetComponent<ShooterSelector>();
            if (selector != null)
            {
                selector.gridX = toX;
                selector.gridY = toY;
            }

            // **QUAN TRỌNG: Reveal secret shooter khi di chuyển lên hàng đầu tiên**
            if (toY == 0)
            {
                Shooter shooter = obj.GetComponent<Shooter>();
                if (shooter != null && shooter.IsSecretShooter && !shooter.isRevealed)
                {
                    shooter.RevealSecretShooter();
                    Debug.Log($"Secret shooter revealed when moved to front row at column {toX}");
                }
            }
        }
    }

    public override Vector3 GetWorldPosition(int x, int y)
    {
        float xOffset = -(width * cellSize) / 2f + cellSize / 2f;  // Căn giữa theo X
        float zStart = -10f; // Đặt shooter nằm dưới block
        float z = zStart - y * cellSize; // Mở rộng xuống dưới
        return new Vector3(x * cellSize + xOffset, 0, z);
    }

    
    public void RevealFrontRowSecretShooters()
    {
        for (int x = 0; x < width; x++)
        {
            Transform shooterTransform = GetObjectAt(x, 0);
            if (shooterTransform != null)
            {
                Shooter shooter = shooterTransform.GetComponent<Shooter>();
                if (shooter != null && shooter.IsSecretShooter && !shooter.isRevealed)
                {
                    shooter.RevealSecretShooter();
                }
            }
        }
    }
}