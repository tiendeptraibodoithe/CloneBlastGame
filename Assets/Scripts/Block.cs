using UnityEngine;

public class Block : MonoBehaviour
{
    public Color blockColor; // Màu sắc của block
    public int floor = 3;

    // Thay đổi màu sắc (tuỳ chọn)
    void Start()
    {
        GetComponent<Renderer>().material.color = blockColor;
    }

    // Xử lý khi bị bắn trúng
    public void Hit()
    {
        floor--;
        Debug.Log($"Block hit! Floor remaining: {floor}");

        // QUAN TRỌNG: Không destroy object ở đây
        // Để AutoFire() tự xử lý việc destroy

        // Có thể thêm hiệu ứng visual khi floor giảm
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Tuỳ chọn: Thay đổi màu sắc dựa trên floor còn lại
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = blockColor;
            // Làm mờ dần khi floor giảm
            color.a = Mathf.Max(0.3f, (float)floor / 3f);
            renderer.material.color = color;
        }
    }

    public bool ShouldDestroy()
    {
        return floor <= 0;
    }
}