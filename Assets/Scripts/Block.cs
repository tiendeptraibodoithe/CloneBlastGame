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

    public void Hit()
    {
        floor--;
        Debug.Log($"Block hit! Floor remaining: {floor}");
        UpdateVisual();
    }

    public bool TryHit()
    {
        if (isBeingHit || floor <= 0) return false;

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
