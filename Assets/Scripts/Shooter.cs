using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class Shooter : MonoBehaviour
{
    public Color shooterColor;
    public ShooterType shooterType;
    public int ammo = 20;
    public float shotDelay = 0.05f; // Delay rất ngắn giữa các viên trong burst
    public float burstDelay = 0.3f; // Delay giữa các burst
    private bool isFiring = false;
    public GameController gameController;
    [SerializeField] public TextMeshProUGUI ammoText;
    public GameObject bulletPrefab;
    public Transform firePoint;

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
        if (!isFiring && ammo > 0)
        {
            StartCoroutine(BurstFireCoroutine(blockGrid));
        }
    }

    private IEnumerator BurstFireCoroutine(BlockGridManager blockGrid)
    {
        isFiring = true;
        Debug.Log($"Burst fire started! Ammo: {ammo}, Shooter Color: {shooterColor}");

        while (ammo > 0)
        {
            // Tìm block đầu tiên từ trái qua phải
            Block currentTarget = FindNextTarget(blockGrid);

            if (currentTarget == null)
            {
                Debug.Log("No more valid targets found");
                break;
            }

            Debug.Log($"Found target block with {currentTarget.floor} floors. Firing burst!");

            // Tính số đạn cần bắn cho block này
            int bulletsNeeded = currentTarget.floor;
            int bulletsToFire = Mathf.Min(bulletsNeeded, ammo);

            // **BẮN BURST - tất cả đạn gần như cùng lúc**
            yield return StartCoroutine(FireBurst(currentTarget, bulletsToFire));

            // Chờ để tất cả đạn trong burst hit target
            yield return new WaitForSeconds(burstDelay);

            Debug.Log($"Burst completed. Ammo left: {ammo}");
        }

        isFiring = false;
        Debug.Log($"Burst fire finished! Ammo left: {ammo}");
    }

    private IEnumerator FireBurst(Block target, int bulletCount)
    {
        Debug.Log($"Firing burst of {bulletCount} bullets!");

        // Bắn tất cả đạn với delay rất ngắn
        for (int i = 0; i < bulletCount; i++)
        {
            if (target == null || ammo <= 0 || target.floor <= 0) break;

            FireBulletAtTarget(target);

            // Delay rất ngắn giữa các viên đạn (tạo hiệu ứng burst)
            if (i < bulletCount - 1) // Không delay sau viên đạn cuối
            {
                yield return new WaitForSeconds(shotDelay);
            }
        }
    }

    private Block FindNextTarget(BlockGridManager blockGrid)
    {
        int width = blockGrid.width;
        int height = blockGrid.height;

        // Quét từ trái qua phải (x = 0 -> width-1)
        for (int x = 0; x < width; x++)
        {
            // Trong mỗi cột, tìm block đầu tiên từ dưới lên trên (y = 0 -> height-1)
            for (int y = 0; y < height; y++)
            {
                Transform targetTransform = blockGrid.GetObjectAt(x, y);
                if (targetTransform == null) continue;

                Block block = targetTransform.GetComponent<Block>();
                if (block != null && block.floor > 0)
                {
                    // Kiểm tra màu sắc có khớp không
                    if (ColorsMatch(block.blockColor, shooterColor))
                    {
                        Debug.Log($"Found target block at column {x}, row {y} with {block.floor} floors");
                        return block; // Trả về block đầu tiên tìm thấy
                    }
                    else
                    {
                        // Nếu gặp block khác màu, dừng tìm kiếm trong cột này
                        break;
                    }
                }
            }
        }

        return null; // Không tìm thấy target nào
    }

    private void FireBulletAtTarget(Block target)
    {
        if (target == null || target.floor <= 0 || ammo <= 0) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(target, this);
            Debug.Log($"Fired bullet at target with floor: {target.floor}");
        }
        else
        {
            // Nếu không có Bullet component, destroy object
            Destroy(bulletObj);
        }
    }

    private bool ColorsMatch(Color color1, Color color2)
    {
        float tolerance = 0.01f;
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
               Mathf.Abs(color1.g - color2.g) < tolerance &&
               Mathf.Abs(color1.b - color2.b) < tolerance;
    }

    private void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = ammo.ToString();
        }
    }

    public void ReduceAmmo()
    {
        ammo--;
        UpdateAmmoText();

        if (ammo <= 0)
        {
            Debug.Log("Shooter hết đạn!");
        }
    }

    public bool IsOutOfAmmo => ammo <= 0;
    public bool IsCurrentlyFiring => isFiring;
}