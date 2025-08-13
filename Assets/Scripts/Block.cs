using UnityEngine;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    public Color blockColor;
    public int floor = 3;
    public GameObject subBlockPrefab; // Gán trong prefab
    private List<GameObject> subBlocks = new List<GameObject>();
    public float spacing = 0.2f;

    private bool isBeingHit = false;

    public bool isLocked = false; // Block có đang bị khóa không
    public int unlockAfterShooterCount = 12; // Số block cần ăn để mở khóa (có thể set khi spawn)
    private int shooterProgress = 0; // Số block đã ăn để mở khóa

    void Start()
    {
        GenerateSubBlocks();
    }

    void GenerateSubBlocks()
    {
        float currentHeight = 0f;
        for (int i = 0; i < floor; i++)
        {
            GameObject sub = Instantiate(subBlockPrefab, transform);
            sub.transform.localPosition = new Vector3(0, currentHeight, 0);
            currentHeight += 0.5f + spacing; // Chiều cao block + khoảng cách

            Renderer r = sub.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = blockColor;
            }
            subBlocks.Add(sub);
        }
    }

    // Khóa block, truyền vào số lượng block cần ăn để mở khóa
    public void LockBlock(int unlockCount)
    {
        isLocked = true;
        unlockAfterShooterCount = unlockCount;
        shooterProgress = 0;
    }

    // Gọi hàm này mỗi khi shooter ăn được 1 block (từ GameController)
    public void ShooterCollectedBlock()
    {
        if (!isLocked) return;

        shooterProgress++;
        if (shooterProgress >= unlockAfterShooterCount)
        {
            UnlockBlock();
        }
    }

    // Mở khóa block
    public void UnlockBlock()
    {
        isLocked = false;
        Debug.Log("Block đã được mở khóa!");
    }
    public void Hit()
    {
        if (isLocked) // Nếu block bị khóa thì không cho bắn
        {
            Debug.Log("Block đang bị đóng băng/khóa, không thể bắn!");
            return;
        }

        floor--;
        Debug.Log($"Block hit! Floor remaining: {floor}");
        UpdateVisual();
    }

    public bool TryHit()
    {
        if (isLocked || isBeingHit || floor <= 0) return false;

        isBeingHit = true;

        if (floor > 0)
        {
            floor--;
            Debug.Log($"Block hit safely! Floor remaining: {floor}");

            UpdateVisual();

            isBeingHit = false;
            return true;
        }

        isBeingHit = false;
        return false;
    }

    private void UpdateVisual()
    {
        if (floor >= 0 && floor < subBlocks.Count)
        {
            // Huỷ tầng trên cùng còn lại
            GameObject top = subBlocks[floor];
            if (top != null)
            {
                Destroy(top);
            }
        }
    }

    public bool IsDestroyed => floor <= 0;
    public bool IsBeingProcessed => isBeingHit;
    public bool ShouldDestroy() => floor <= 0;
}
