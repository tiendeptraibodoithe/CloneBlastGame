using UnityEngine;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    public Color blockColor;
    public int floor = 3;
    public GameObject subBlockPrefab;
    private List<GameObject> subBlocks = new List<GameObject>();
    public float spacing = 0.2f;
    private bool isBeingHit = false;

    [Header("Freeze System")]
    public bool isFrozen = false;
    public int blocksRequiredToUnfreeze = 0;
    private int currentDestroyedCount = 0;
    public static int totalBlocksDestroyed = 0;
    private BlockType blockType;

    void Start()
    {
    }

    public void Initialize(BlockType type)
    {
        blockType = type;
        blockColor = type.color;
        subBlockPrefab = type.subBlockPrefab;
        floor = type.startingFloor;
        isFrozen = type.startsFrozen;
        blocksRequiredToUnfreeze = type.blocksToUnfreeze;

        Debug.Log($"Init {gameObject.name} | startsFrozen={isFrozen} | blocksRequiredToUnfreeze={blocksRequiredToUnfreeze}");

        GenerateSubBlocks();
        UpdateFrozenVisual();

        if (isFrozen)
        {
            Debug.Log($"Block {gameObject.name} starts frozen. Need {blocksRequiredToUnfreeze} blocks destroyed to unfreeze.");
        }
    }

    void GenerateSubBlocks()
    {
        float currentHeight = 0f;
        for (int i = 0; i < floor; i++)
        {
            GameObject sub = Instantiate(subBlockPrefab, transform);
            sub.transform.localPosition = new Vector3(0, currentHeight, 0);
            currentHeight += 0.5f + spacing;
            Renderer r = sub.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = blockColor;
            }
            subBlocks.Add(sub);
        }

        UpdateFrozenVisual();
    }

    public void Hit()
    {
        if (isFrozen)
        {
            Debug.Log("Block is frozen, cannot be hit!");
            return;
        }

        floor--;
        Debug.Log($"Block hit! Floor remaining: {floor}");
        UpdateVisual();

        if (IsDestroyed)
        {
            OnBlockDestroyed();
        }
    }

    public bool TryHit()
    {
        if (isBeingHit || floor <= 0 || isFrozen) return false;

        isBeingHit = true;
        if (floor > 0)
        {
            floor--;
            Debug.Log($"Block hit safely! Floor remaining: {floor}");
            UpdateVisual();

            if (IsDestroyed)
            {
                OnBlockDestroyed();
            }

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
            GameObject top = subBlocks[floor];
            if (top != null)
            {
                Destroy(top);
            }
        }
    }

    private void UpdateFrozenVisual()
    {
        Debug.Log($"UpdateFrozenVisual called for {gameObject.name}: isFrozen={isFrozen}, subBlocks count={subBlocks.Count}");

        if (isFrozen)
        {
            // Làm mờ màu để hiển thị frozen - thử nhiều cách
            foreach (GameObject subBlock in subBlocks)
            {
                if (subBlock != null)
                {
                    Renderer r = subBlock.GetComponent<Renderer>();
                    if (r != null)
                    {
                        // Cách 1: Thay đổi alpha (nếu shader hỗ trợ)
                        Color frozenColor = blockColor;
                        frozenColor.a = 0.5f;
                        r.material.color = frozenColor;

                        // Cách 2: Làm tối màu thay vì làm mờ
                        Color darkColor = blockColor * 0.5f; // Làm tối 50%
                        darkColor.a = 1f; // Giữ alpha = 1
                        r.material.color = darkColor;

                        // Cách 3: Thêm tint màu xanh lạnh
                        Color frozenTint = Color.Lerp(blockColor, Color.cyan, 0.3f);
                        frozenTint = frozenTint * 0.7f; // Làm tối một chút
                        frozenTint.a = 1f;
                        r.material.color = frozenTint;

                        Debug.Log($"Applied frozen visual to {subBlock.name}: {r.material.color}");
                    }
                }
            }
        }
        else
        {
            // Khôi phục màu gốc
            foreach (GameObject subBlock in subBlocks)
            {
                if (subBlock != null)
                {
                    Renderer r = subBlock.GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.material.color = blockColor;
                        Debug.Log($"Restored original color to {subBlock.name}: {blockColor}");
                    }
                }
            }
        }
    }

    private void OnBlockDestroyed()
    {
        // QUAN TRỌNG: Thực hiện logic unfreeze TRƯỚC khi destroy
        totalBlocksDestroyed++;
        Debug.Log($"Total blocks destroyed: {totalBlocksDestroyed}");

        // Kiểm tra tất cả block frozen TRƯỚC KHI destroy block này
        foreach (Block block in FindObjectsOfType<Block>())
        {
            if (block != this && block.isFrozen) // Không check chính nó
            {
                block.currentDestroyedCount++;

                Debug.Log($"[Freeze] {block.name} progress: {block.currentDestroyedCount}/{block.blocksRequiredToUnfreeze} (Total: {totalBlocksDestroyed})");

                if (block.currentDestroyedCount >= block.blocksRequiredToUnfreeze ||
                    totalBlocksDestroyed >= block.blocksRequiredToUnfreeze)
                {
                    block.Unfreeze();
                }
            }
        }

        // SAU ĐÓ mới destroy
        Destroy(gameObject);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;
        currentDestroyedCount = 0;
        UpdateFrozenVisual();
        Debug.Log($"UNFROZEN: {gameObject.name} is now unfrozen!", this);
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        UpdateFrozenVisual();
        Debug.Log($"Block {gameObject.name} frozen state: {frozen}");
    }

    public bool IsDestroyed => floor <= 0;
    public bool IsBeingProcessed => isBeingHit;
    public bool ShouldDestroy() => floor <= 0;
    public bool CanBeHit => !isFrozen && floor > 0;
}