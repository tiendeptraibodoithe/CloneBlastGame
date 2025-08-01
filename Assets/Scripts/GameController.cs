using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Grid Managers")]
    public BlockGridManager blockGrid;
    public ShooterGridManager shooterGrid;

    [Header("Block Type Mapping (index = ID in file)")]
    public List<BlockType> blockTypeMapping;

    [Header("Shooter Type Mapping (index = ID in file)")]
    public List<ShooterType> shooterTypeMapping;

    [Header("Shooter Selection Settings")]
    public Transform[] shooterSelectionSlots = new Transform[5]; // Gán vị trí ô trên cùng trong Editor
    private List<GameObject> selectedShooters = new List<GameObject>();

    void Start()
    {
        LoadLevelFromText("Level1");
        LoadShooterFromText("Shooter1");
    }

    void Update()
    {
        // Dọn dẹp những shooter đã bị huỷ hoặc set inactive
        selectedShooters.RemoveAll(s => s == null || !s.activeSelf);

        for (int i = selectedShooters.Count - 1; i >= 0; i--)
        {
            var shooterObj = selectedShooters[i];
            Shooter shooter = shooterObj.GetComponent<Shooter>();

            if (shooter != null && shooter.ammo <= 0)
            {
                Destroy(shooterObj); // Huỷ shooter
                selectedShooters.RemoveAt(i);
            }
        }
    }


    public void LoadLevelFromText(string fileName)
    {
        TextAsset levelData = Resources.Load<TextAsset>("Levels/" + fileName);
        if (levelData == null)
        {
            Debug.LogError("Không tìm thấy file level: " + fileName);
            return;
        }

        string[] lines = levelData.text
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        for (int y = 0; y < lines.Length; y++)
        {
            string[] tokens = lines[y].Trim().Split(' ');
            for (int x = 0; x < tokens.Length; x++)
            {
                if (int.TryParse(tokens[x], out int id) && id >= 0 && id < blockTypeMapping.Count)
                {
                    int gridY = (lines.Length - 1) - y; // đảo chiều nếu grid của bạn vẽ từ dưới lên
                    BlockType type = blockTypeMapping[id];
                    if (type != null)
                    {
                        blockGrid.SpawnBlock(x, gridY, type);
                    }
                }
            }
        }
    }

    public void LoadShooterFromText(string fileName)
    {
        TextAsset shooterData = Resources.Load<TextAsset>("Levels/" + fileName);
        if (shooterData == null)
        {
            Debug.LogError("Không tìm thấy file shooter: " + fileName);
            return;
        }

        string[] lines = shooterData.text
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        for (int y = 0; y < lines.Length; y++)
        {
            string[] tokens = lines[y].Trim().Split(' ');
            for (int x = 0; x < tokens.Length; x++)
            {
                if (int.TryParse(tokens[x], out int id) && id >= 0 && id < shooterTypeMapping.Count)
                {
                    int gridY = (lines.Length - 1) - y; // y đảo nếu shooterGrid vẽ từ dưới lên
                    ShooterType type = shooterTypeMapping[id];
                    if (type != null)
                    {
                        shooterGrid.SpawnShooter(x, gridY, type);
                    }
                }
            }
        }
    }

    public void OnShooterClicked(ShooterSelector selector)
    {
        // Làm sạch các shooter đã null hoặc bị destroy khỏi danh sách
        selectedShooters.RemoveAll(s => s == null || !s.activeSelf);

        // Tìm slot đầu tiên còn trống (chưa có child)
        int availableSlotIndex = -1;
        for (int i = 0; i < shooterSelectionSlots.Length; i++)
        {
            if (shooterSelectionSlots[i].childCount == 0)
            {
                availableSlotIndex = i;
                break;
            }
        }

        if (availableSlotIndex == -1)
        {
            Debug.Log("Đã đầy slot chọn.");
            return;
        }

        // Lấy thông tin shooter gốc
        Shooter originalShooter = selector.GetComponent<Shooter>();
        if (originalShooter == null)
        {
            Debug.LogError("Không tìm thấy Shooter component!");
            return;
        }

        Debug.Log($"Selecting shooter with color: {originalShooter.shooterColor}, type: {originalShooter.shooterType}, ammo: {originalShooter.ammo}");

        // Tạo shooter clone tại slot trống
        GameObject shooterClone = Instantiate(selector.gameObject);
        shooterClone.transform.SetParent(shooterSelectionSlots[availableSlotIndex], worldPositionStays: false);
        shooterClone.transform.localPosition = Vector3.zero;

        // Tắt collider và xóa selector component của clone
        Collider cloneCollider = shooterClone.GetComponent<Collider>();
        if (cloneCollider != null) cloneCollider.enabled = false;

        ShooterSelector cloneSelector = shooterClone.GetComponent<ShooterSelector>();
        if (cloneSelector != null) Destroy(cloneSelector);

        // Gán thông tin cho shooter clone
        Shooter cloneShooter = shooterClone.GetComponent<Shooter>();
        if (cloneShooter != null)
        {
            cloneShooter.shooterColor = originalShooter.shooterColor;
            cloneShooter.shooterType = originalShooter.shooterType;
            cloneShooter.ammo = originalShooter.ammo;
            cloneShooter.gameController = this;

            MeshRenderer mr = cloneShooter.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = cloneShooter.shooterColor;
            }

            Debug.Log($"Clone shooter created with color: {cloneShooter.shooterColor}, ammo: {cloneShooter.ammo}");

            // Bắt đầu bắn
            Debug.Log("Starting AutoFire...");
            cloneShooter.AutoFire(blockGrid);
        }

        // Thêm clone vào danh sách
        selectedShooters.Add(shooterClone);

        // Xóa shooter gốc khỏi shooter grid
        shooterGrid.ClearCell(selector.gridX, selector.gridY);
        Destroy(selector.gameObject);

        // Đẩy các shooter bên dưới lên
        for (int y = selector.gridY - 1; y >= 0; y--)
        {
            Transform shooterBelow = shooterGrid.GetObjectAt(selector.gridX, y);
            if (shooterBelow != null)
            {
                shooterGrid.MoveObject(selector.gridX, y, selector.gridX, y + 1);
            }
        }
    }


    public void RecheckAllSelectedShooters()
    {
        foreach (GameObject shooterObj in selectedShooters.ToList()) // dùng ToList() để tránh lỗi nếu shooter bị destroy
        {
            if (shooterObj == null) continue;

            Shooter shooter = shooterObj.GetComponent<Shooter>();
            if (shooter != null && shooter.ammo > 0)
            {
                shooter.AutoFire(blockGrid);
            }
        }
    }

    public void RemoveShooterReference(GameObject shooterObj)
    {
        selectedShooters.Remove(shooterObj);
    }
}