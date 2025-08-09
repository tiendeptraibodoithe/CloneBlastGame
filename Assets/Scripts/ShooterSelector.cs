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
        if (gridY != 0)
        {
            Debug.Log("Chỉ chọn shooter hàng đầu.");
            return;
        }

        if (isSelected) return;

        // Gọi controller và chỉ đánh dấu selected nếu chọn thành công
        if (gameController.OnShooterClicked(this))
        {
            isSelected = true;
        }
    }
}

