using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class Shooter : MonoBehaviour
{
    [Header("Basic Properties")]
    public Color shooterColor;
    public ShooterType shooterType;
    public int ammo = 20;
    public float shotDelay = 0.05f;
    public float burstDelay = 0.3f;

    [Header("Special Mechanics")]
    public bool isRevealed = false; // Cho Secret Shooter

    private bool isFiring = false;
    public GameController gameController;
    [SerializeField] public TextMeshProUGUI ammoText;
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Visual components
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material hiddenMaterial;

    void Start()
    {
        InitializeVisuals();
        CheckIfShouldReveal();
        UpdateAmmoText();
    }

    private void InitializeVisuals()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null && shooterType != null)
        {
            // Tạo materials
            originalMaterial = new Material(meshRenderer.material);
            originalMaterial.color = shooterType.color;

            hiddenMaterial = new Material(meshRenderer.material);
            hiddenMaterial.color = shooterType.hiddenColor;

            UpdateVisualAppearance();
        }

        // Find ammo text
        Transform textObj = transform.Find("Canvas/AmmoText");
        if (textObj != null)
        {
            ammoText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void CheckIfShouldReveal()
    {
        // Reveal secret shooter nếu ở hàng đầu tiên (y = 0)
        if (shooterType != null && shooterType.specialType == ShooterSpecialType.Secret)
        {
            ShooterSelector selector = GetComponent<ShooterSelector>();
            if (selector != null && selector.gridY == 0)
            {
                RevealSecretShooter();
            }
        }
    }

    public void SetType(ShooterType type)
    {
        shooterType = type;
        shooterColor = type.color;
        ammo = type.GetActualAmmo();

        // Khởi tạo lại materials với type mới
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = new Material(meshRenderer.material);
            originalMaterial.color = type.color;

            hiddenMaterial = new Material(meshRenderer.material);
            hiddenMaterial.color = type.hiddenColor;

            UpdateVisualAppearance();
        }

        // Kiểm tra xem có cần reveal không (cho trường hợp spawn)
        CheckIfShouldReveal();
        UpdateAmmoText();
    }

    private void UpdateVisualAppearance()
    {
        if (shooterType == null || meshRenderer == null) return;

        Color displayColor = shooterType.GetDisplayColor(isRevealed);

        // Cập nhật màu sắc
        if (shooterType.specialType == ShooterSpecialType.Secret && !isRevealed)
        {
            meshRenderer.material = hiddenMaterial;
        }
        else
        {
            meshRenderer.material = originalMaterial;
            meshRenderer.material.color = displayColor;
        }

        // Thêm visual effects cho Double Shooter
        if (shooterType.specialType == ShooterSpecialType.Double)
        {
            AddDoubleShooterEffects();
        }
    }

    private void AddDoubleShooterEffects()
    {

        // Thêm glow effect nếu có shader hỗ trợ
        if (meshRenderer.material.HasProperty("_EmissionColor"))
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", shooterType.doubleShooterColor * 0.3f);
        }
    }

    public void RevealSecretShooter()
    {
        if (shooterType.specialType == ShooterSpecialType.Secret && !isRevealed)
        {
            isRevealed = true;
            UpdateVisualAppearance();
            UpdateAmmoText();

            // Hiệu ứng reveal
            StartCoroutine(RevealEffect());
        }
    }

    private IEnumerator RevealEffect()
    {
        // Kiểm tra meshRenderer trước khi sử dụng
        if (meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer is null, skipping reveal effect");
            yield break;
        }

        // Hiệu ứng flash khi reveal
        float flashDuration = 0.3f;
        Color originalColor = meshRenderer.material.color;

        for (int i = 0; i < 3; i++)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = Color.white;
                yield return new WaitForSeconds(flashDuration / 6);
                meshRenderer.material.color = originalColor;
                yield return new WaitForSeconds(flashDuration / 6);
            }
        }
    }

    public bool CanMergeWith(Shooter other)
    {
        if (shooterType == null || other.shooterType == null)
            return false;

        // Kiểm tra xem có thể merge không
        if (!shooterType.CanMerge() || !other.shooterType.CanMerge())
            return false;

        // Kiểm tra màu sắc (chỉ merge khi đã reveal)
        if (shooterType.specialType == ShooterSpecialType.Secret && !isRevealed)
            return false;

        if (other.shooterType.specialType == ShooterSpecialType.Secret && !other.isRevealed)
            return false;

        return ColorsMatch(shooterColor, other.shooterColor);
    }

    public void AutoFire(BlockGridManager blockGrid)
    {
        // Không cần reveal ở đây nữa vì đã reveal khi ở hàng đầu tiên
        if (!isFiring && ammo > 0)
        {
            StartCoroutine(BurstFireCoroutine(blockGrid));
        }
    }

    private IEnumerator BurstFireCoroutine(BlockGridManager blockGrid)
    {
        isFiring = true;
        Debug.Log($"Burst fire started! Ammo: {ammo}, Shooter Color: {shooterColor}, Type: {shooterType.specialType}");

        while (ammo > 0)
        {
            Block currentTarget = FindNextTarget(blockGrid);

            if (currentTarget == null)
            {
                Debug.Log("No more valid targets found");
                break;
            }

            Debug.Log($"Found target block with {currentTarget.floor} floors. Firing burst!");

            int bulletsNeeded = currentTarget.floor;
            int bulletsToFire = Mathf.Min(bulletsNeeded, ammo);

            yield return StartCoroutine(FireBurst(currentTarget, bulletsToFire));
            yield return new WaitForSeconds(burstDelay);

            Debug.Log($"Burst completed. Ammo left: {ammo}");
        }

        isFiring = false;
        Debug.Log($"Burst fire finished! Ammo left: {ammo}");
    }

    private IEnumerator FireBurst(Block target, int bulletCount)
    {
        Debug.Log($"Firing burst of {bulletCount} bullets!");

        for (int i = 0; i < bulletCount; i++)
        {
            if (target == null || ammo <= 0 || target.floor <= 0) break;

            FireBulletAtTarget(target);

            if (i < bulletCount - 1)
            {
                yield return new WaitForSeconds(shotDelay);
            }
        }
    }

    private Block FindNextTarget(BlockGridManager blockGrid)
    {
        int width = blockGrid.width;
        int height = blockGrid.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Transform targetTransform = blockGrid.GetObjectAt(x, y);
                if (targetTransform == null) continue;

                Block block = targetTransform.GetComponent<Block>();
                if (block != null && block.floor > 0)
                {
                    // 🚫 Bỏ qua block bị khóa
                    if (block.isLocked)
                    {
                        Debug.Log($"Skipping locked block at {x},{y}");
                        continue;
                    }

                    if (ColorsMatch(block.blockColor, shooterColor))
                    {
                        Debug.Log($"Found target block at column {x}, row {y} with {block.floor} floors");
                        return block;
                    }
                    else
                    {
                        break; // dừng tìm ở cột này nếu màu không khớp
                    }
                }
            }
        }

        return null;
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
            // Ẩn ammo text cho Secret Shooter chưa reveal
            if (shooterType != null && shooterType.specialType == ShooterSpecialType.Secret && !isRevealed)
            {
                ammoText.text = "?";
            }
            else
            {
                ammoText.text = ammo.ToString();
            }
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

    // Properties để truy cập thông tin special type
    public bool IsSecretShooter => shooterType != null && shooterType.specialType == ShooterSpecialType.Secret;
    public bool IsDoubleShooter => shooterType != null && shooterType.specialType == ShooterSpecialType.Double;
    public bool IsNormalShooter => shooterType != null && shooterType.specialType == ShooterSpecialType.Normal;
}