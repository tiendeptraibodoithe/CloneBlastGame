using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterSelector : MonoBehaviour
{
    public int gridX;
    public int gridY;
    private GameController gameController;
    private bool isSelected = false;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    private void OnMouseDown()
    {
        // Chỉ cho phép chọn shooter ở hàng trên cùng
        if (gridY != gameController.shooterGrid.height - 1)
        {
            Debug.Log("Chỉ chọn shooter hàng đầu.");
            return;
        }

        if (isSelected) return; // Ngăn chọn lại nếu đã bị chọn
        isSelected = true;

        GameController controller = GameObject.FindObjectOfType<GameController>();

        gameController.OnShooterClicked(this);
    }
}

