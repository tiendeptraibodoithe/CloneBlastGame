using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Thêm namespace này để sử dụng IEnumerator
using TMPro;

public class Shooter : MonoBehaviour
{
    public Color shooterColor; // Màu sắc của shooter
    public ShooterType shooterType;
    public int ammo = 20;
    public float shotDelay = 0.2f; // Thêm biến delay giữa các phát đạn
    private bool isFiring = false;
    public GameController gameController;
    [SerializeField] public TextMeshProUGUI ammoText;
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Thay đổi màu sắc (tuỳ chọn)
    void Start()
    {
        GetComponent<Renderer>().material.color = shooterColor;
        Transform textObj = transform.Find("Canvas/AmmoText");
        if (textObj != null)
        {
            ammoText = textObj.GetComponent<TextMeshProUGUI>();
        }

        UpdateAmmoText();
    }

    public void SetType(ShooterType type)
    {
        shooterType = type;
        shooterColor = type.color;
        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material.color = type.color;
        }
    }

    public void AutoFire(BlockGridManager blockGrid)
    {
        if (!isFiring)
        {
            StartCoroutine(AutoFireCoroutine(blockGrid));
        }
    }

    private IEnumerator AutoFireCoroutine(BlockGridManager blockGrid)
    {
        isFiring = true;
        Debug.Log($"AutoFire started! Ammo: {ammo}, Shooter Color: {shooterColor}");

        int width = blockGrid.width;
        int height = blockGrid.height;

        // Tìm tất cả các block có cùng màu với shooter
        List<Vector2Int> targetPositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            // Tìm block đầu tiên (gần nhất với shooter) từ phía dưới lên trên
            for (int y = 0; y < height; y++)
            {
                Transform target = blockGrid.GetObjectAt(x, y);
                if (target == null) continue;

                Block block = target.GetComponent<Block>();
                if (block != null)
                {
                    Debug.Log($"Found block at ({x},{y}) - Block Color: {block.blockColor}, Shooter Color: {shooterColor}");

                    // Kiểm tra màu sắc có khớp không (với tolerance để tránh lỗi floating point)
                    if (ColorsMatch(block.blockColor, shooterColor))
                    {
                        Debug.Log($"Color match! Adding target at ({x},{y})");
                        targetPositions.Add(new Vector2Int(x, y));
                        break; // Chỉ lấy block đầu tiên trong cột này
                    }
                    else
                    {
                        // Nếu gặp block khác màu, dừng tìm kiếm trong cột này
                        break;
                    }
                }
            }
        }

        Debug.Log($"Found {targetPositions.Count} target blocks with matching color");

        // Bắn vào các block đã tìm thấy
        foreach (Vector2Int pos in targetPositions)
        {
            if (ammo <= 0) break;

            Transform target = blockGrid.GetObjectAt(pos.x, pos.y);
            if (target == null) continue;

            Block block = target.GetComponent<Block>();
            if (block == null) continue;

            Debug.Log($"Shooting at block ({pos.x},{pos.y})");

            // Bắn vào block này cho đến khi hết đạn hoặc block bị phá huỷ
            while (ammo > 0 && block != null && block.floor > 0)
            {
                if (!ColorsMatch(block.blockColor, shooterColor))
                {
                    Debug.LogWarning($"Block at ({pos.x},{pos.y}) is different color! Skipping.");
                    break;
                }
                Debug.Log($"Hitting block! Floor before: {block.floor}");
                if (bulletPrefab != null)
                {
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                    StartCoroutine(MoveBulletToTarget(bullet.transform, block.transform.position));
                }
                block.Hit();
                ammo--;
                UpdateAmmoText();
                Debug.Log($"Floor after: {block.floor}, Ammo left: {ammo}");

                if (block != null && block.gameObject != null && block.floor <= 0)
                {
                    Debug.Log($"Block destroyed at ({pos.x},{pos.y})!");

                    blockGrid.ClearCell(pos.x, pos.y);
                    Destroy(block.gameObject);

                    // Cho các block phía trên rơi xuống
                    blockGrid.DropColumnDown(pos.x, pos.y);
                    isFiring = false;

                    // 🔁 Kiểm tra lại tất cả các shooter đã chọn
                    GameObject.FindObjectOfType<GameController>().RecheckAllSelectedShooters();

                    // 🔄 Nếu còn đạn, tiếp tục bắn
                    if (ammo > 0)
                    {
                        StartCoroutine(AutoFireCoroutine(blockGrid));
                    }

                    yield break; // Kết thúc coroutine hiện tại
                }

                // Thêm delay giữa các phát đạn
                yield return new WaitForSeconds(shotDelay);

                if (ammo <= 0)
                {
                    Debug.Log("Shooter has no ammo left. Destroying...");
                    Destroy(gameObject);
                    yield break;
                }
            }
        }
        isFiring = false;
        Debug.Log($"AutoFire finished! Ammo left: {ammo}");
    }

    private bool ColorsMatch(Color color1, Color color2)
    {
        // So sánh màu với tolerance để tránh lỗi floating point
        float tolerance = 0.01f;
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
               Mathf.Abs(color1.g - color2.g) < tolerance &&
               Mathf.Abs(color1.b - color2.b) < tolerance;
    }

    public void HitBlock()
    {
        ammo--;
        UpdateAmmoText();

        if (ammo <= 0)
        {
            Debug.Log("Shooter hết đạn!");
            Destroy(gameObject);
        }
    }

    private void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = ammo.ToString();
        }
    }

    private IEnumerator MoveBulletToTarget(Transform bullet, Vector3 targetPos)
    {
        float speed = 10f; // chỉnh tốc độ theo ý bạn
        while (bullet != null && Vector3.Distance(bullet.position, targetPos) > 0.1f)
        {
            bullet.position = Vector3.MoveTowards(bullet.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        // Khi tới nơi, có thể phá huỷ đạn
        if (bullet != null) Destroy(bullet.gameObject);
    }
}