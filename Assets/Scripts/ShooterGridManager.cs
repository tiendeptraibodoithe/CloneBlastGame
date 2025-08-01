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

            shooter.GetComponent<Shooter>().SetType(type);
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

                // Cập nhật thông tin vị trí nếu cần
                var selector = obj.GetComponent<ShooterSelector>();
                if (selector != null)
                {
                    selector.gridX = toX;
                    selector.gridY = toY;
                }
            }
        }
    }
