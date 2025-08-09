using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Grid Managers")]
    public BlockGridManager blockGrid;
    public ShooterGridManager shooterGrid;

    [Header("Block Type Mapping (index = ID in file)")]
    public List<BlockType> blockTypeMapping;

    [Header("Shooter Type Mapping (index = ID in file)")]
    public List<ShooterType> shooterTypeMapping;

    [Header("Merged Shooter Types")]
    public List<ShooterType> mergedShooterTypes;

    [Header("Shooter Selection Settings")]
    public Transform[] shooterSelectionSlots = new Transform[5]; // Gán vị trí ô trên cùng trong Editor
    private List<GameObject> selectedShooters = new List<GameObject>();

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Level"))
        {
            string levelNumber = sceneName.Replace("Level", "");

            LoadLevelFromText("Level" + levelNumber);
            LoadShooterFromText("Shooter" + levelNumber);
        }
        else
        {
            Debug.LogWarning("Tên scene không bắt đầu bằng 'Level', không load dữ liệu!");
        }
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
                    // Đảo thứ tự hàng: hàng đầu tiên trong file sẽ là hàng trên cùng
                    int gridY = y;
                    ShooterType type = shooterTypeMapping[id];
                    if (type != null)
                    {
                        shooterGrid.SpawnShooter(x, gridY, type);
                    }
                }
            }
        }
    }

    public bool OnShooterClicked(ShooterSelector selector)
    {
        // Làm sạch các shooter đã null hoặc bị destroy khỏi danh sách
        selectedShooters.RemoveAll(s => s == null || !s.activeSelf);

        // Tìm slot đầu tiên còn trống
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
            return false;
        }

        // Lấy thông tin shooter gốc
        Shooter originalShooter = selector.GetComponent<Shooter>();
        if (originalShooter == null)
        {
            Debug.LogError("Không tìm thấy Shooter component!");
            return false;
        }

        // Kiểm tra shooter này có phải ở hàng cuối cùng không
        // Xác định hàng "dưới cùng" dựa trên hướng sắp xếp thực tế
        bool isBottomRow = true;
        for (int y = selector.gridY - 1; y >= 0; y--) // quét xuống thay vì lên
        {
            if (shooterGrid.GetObjectAt(selector.gridX, y) != null)
            {
                isBottomRow = false;
                break;
            }
        }

        if (!isBottomRow)
        {
            Debug.Log("Chỉ có thể chọn shooter ở hàng cuối cùng");
            return false;
        }

        Debug.Log($"Selecting shooter with color: {originalShooter.shooterColor}, type: {originalShooter.shooterType}, ammo: {originalShooter.ammo}");

        GameObject shooterClone = Instantiate(selector.gameObject);
        shooterClone.transform.SetParent(shooterSelectionSlots[availableSlotIndex], worldPositionStays: false);
        shooterClone.transform.localPosition = Vector3.zero;

        Collider cloneCollider = shooterClone.GetComponent<Collider>();
        if (cloneCollider != null) cloneCollider.enabled = false;

        ShooterSelector cloneSelector = shooterClone.GetComponent<ShooterSelector>();
        if (cloneSelector != null) Destroy(cloneSelector);

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
            cloneShooter.AutoFire(blockGrid);
        }

        selectedShooters.Add(shooterClone);

        TryMergeSelectedShooters();

        // Xóa shooter khỏi grid
        shooterGrid.ClearCell(selector.gridX, selector.gridY);
        Destroy(selector.gameObject);

        // Di chuyển các shooter phía dưới lên
        for (int y = selector.gridY + 1; y < shooterGrid.height; y++)
        {
            Transform shooterBelow = shooterGrid.GetObjectAt(selector.gridX, y);
            if (shooterBelow != null)
            {
                shooterGrid.MoveObject(selector.gridX, y, selector.gridX, y - 1);
            }
        }


        return true;
    }

    private void TryMergeSelectedShooters()
    {
        // Cần ít nhất 3 shooter để kiểm tra
        if (selectedShooters.Count < 3)
            return;

        // Nhóm các shooter theo màu
        var colorGroups = selectedShooters
            .Where(s => s != null)
            .GroupBy(s => s.GetComponent<Shooter>().shooterColor);

        foreach (var group in colorGroups)
        {
            var shootersOfColor = group.ToList();

            if (shootersOfColor.Count >= 3)
            {
                // Lấy 3 shooter đầu tiên cùng màu
                GameObject s1 = shootersOfColor[0];
                GameObject s2 = shootersOfColor[1];
                GameObject s3 = shootersOfColor[2];

                // Tính tổng ammo
                int totalAmmo = s1.GetComponent<Shooter>().ammo +
                                s2.GetComponent<Shooter>().ammo +
                                s3.GetComponent<Shooter>().ammo;

                Color mergedColor = group.Key;

                // Tìm ShooterType hợp nhất tương ứng
                ShooterType mergedType = mergedShooterTypes
                    .FirstOrDefault(t => t.color == mergedColor);

                if (mergedType == null)
                {
                    Debug.LogError($"Không tìm thấy merged type cho màu: {mergedColor}");
                    return;
                }

                // Lấy vị trí slot của shooter thứ 2 để spawn shooter hợp nhất tại đó
                int middleIndex = shooterSelectionSlots
                    .ToList().FindIndex(slot => slot.childCount > 0 && slot.GetChild(0).gameObject == s2);

                if (middleIndex == -1)
                {
                    Debug.LogError("Không tìm thấy vị trí giữa để đặt shooter hợp nhất.");
                    return;
                }

                // Xoá 3 shooter cũ
                Destroy(s1);
                Destroy(s2);
                Destroy(s3);
                selectedShooters.Remove(s1);
                selectedShooters.Remove(s2);
                selectedShooters.Remove(s3);

                // Spawn shooter mới
                GameObject mergedShooter = Instantiate(mergedType.prefab, shooterSelectionSlots[middleIndex]);
                mergedShooter.transform.localPosition = Vector3.zero;

                Shooter shooterComponent = mergedShooter.GetComponent<Shooter>();
                shooterComponent.shooterColor = mergedColor;
                shooterComponent.shooterType = mergedType;
                shooterComponent.ammo = totalAmmo;
                shooterComponent.gameController = this;

                // Đặt màu nếu cần
                MeshRenderer mr = mergedShooter.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.material.color = mergedColor;
                }

                // Bắn
                shooterComponent.AutoFire(blockGrid);

                // Thêm vào danh sách
                selectedShooters.Add(mergedShooter);

                Debug.Log($"Đã hợp nhất shooter màu {mergedColor} với ammo {totalAmmo}");

                break; // Chỉ merge 1 lần mỗi lần gọi
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