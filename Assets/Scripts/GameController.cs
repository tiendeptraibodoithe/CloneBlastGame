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
        selectedShooters.RemoveAll(s => s == null || !s.activeSelf);

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

        Shooter originalShooter = selector.GetComponent<Shooter>();
        if (originalShooter == null)
        {
            Debug.LogError("Không tìm thấy Shooter component!");
            return false;
        }

        // Kiểm tra shooter này có phải ở hàng cuối cùng không
        bool isBottomRow = true;
        for (int y = selector.gridY - 1; y >= 0; y--)
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

        Debug.Log($"Selecting shooter - Type: {originalShooter.shooterType.specialType}, Color: {originalShooter.shooterColor}, Ammo: {originalShooter.ammo}");

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
            // Copy đầy đủ thông tin từ original
            cloneShooter.shooterColor = originalShooter.shooterColor;
            cloneShooter.shooterType = originalShooter.shooterType;
            cloneShooter.ammo = originalShooter.ammo;
            cloneShooter.isRevealed = originalShooter.isRevealed;
            cloneShooter.gameController = this;

            // QUAN TRỌNG: Reveal secret shooter TRƯỚC khi làm bất cứ việc gì khác
            if (cloneShooter.IsSecretShooter && !cloneShooter.isRevealed)
            {
                cloneShooter.RevealSecretShooter();
                // Đảm bảo isRevealed được set sau khi reveal
                cloneShooter.isRevealed = true;

                Debug.Log($"Secret shooter revealed - New Color: {cloneShooter.shooterColor}");
            }

            // Khởi tạo lại visuals cho clone SAU KHI reveal
            cloneShooter.SetType(cloneShooter.shooterType);

            Debug.Log($"Clone shooter created - Type: {cloneShooter.shooterType.specialType}, Color: {cloneShooter.shooterColor}, Ammo: {cloneShooter.ammo}, IsRevealed: {cloneShooter.isRevealed}");

            // Auto fire SAU KHI đã reveal hoàn toàn
            cloneShooter.AutoFire(blockGrid);
        }

        selectedShooters.Add(shooterClone);

        // Thử merge SAU KHI shooter đã được xử lý hoàn toàn
        TryMergeSelectedShooters();

        // Xóa shooter khỏi grid và di chuyển các shooter phía dưới lên
        shooterGrid.ClearCell(selector.gridX, selector.gridY);
        Destroy(selector.gameObject);

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
        if (selectedShooters.Count < 3)
            return;

        // Lọc ra những shooter có thể merge và đã được reveal
        var mergeableShooters = selectedShooters
            .Where(s => s != null)
            .Select(s => s.GetComponent<Shooter>())
            .Where(shooter => shooter != null && shooter.shooterType.CanMerge())
            .Where(shooter => !(shooter.IsSecretShooter && !shooter.isRevealed)) // Secret shooter phải được reveal
            .ToList();

        if (mergeableShooters.Count < 3)
        {
            Debug.Log("Không đủ shooter có thể merge hoặc chưa reveal");
            return;
        }

        // Tạo dictionary để nhóm shooter theo màu sắc (không phân biệt type)
        Dictionary<Color, List<Shooter>> colorGroups = new Dictionary<Color, List<Shooter>>();

        foreach (var shooter in mergeableShooters)
        {
            Color shooterColor = shooter.shooterColor;

            // Tìm xem có màu nào tương tự không (dùng tolerance)
            Color matchingColor = Color.clear;
            foreach (var existingColor in colorGroups.Keys)
            {
                if (ColorsMatch(existingColor, shooterColor))
                {
                    matchingColor = existingColor;
                    break;
                }
            }

            if (matchingColor != Color.clear)
            {
                // Thêm vào nhóm màu đã có
                colorGroups[matchingColor].Add(shooter);
            }
            else
            {
                // Tạo nhóm màu mới
                colorGroups[shooterColor] = new List<Shooter> { shooter };
            }
        }

        foreach (var group in colorGroups)
        {
            var shootersOfColor = group.Value;
            Color groupColor = group.Key;

            Debug.Log($"Color group {groupColor} has {shootersOfColor.Count} shooters:");
            foreach (var shooter in shootersOfColor)
            {
                Debug.Log($"  - {shooter.shooterType.name} (Type: {shooter.shooterType.specialType}, Color: {shooter.shooterColor})");
            }

            if (shootersOfColor.Count >= 3)
            {
                // Lấy 3 shooter đầu tiên cùng màu (có thể khác type)
                Shooter s1 = shootersOfColor[0];
                Shooter s2 = shootersOfColor[1];
                Shooter s3 = shootersOfColor[2];

                // Kiểm tra tất cả đều có thể merge (dựa trên màu, không cần cùng type)
                if (!CanMergeByColor(s1, s2) || !CanMergeByColor(s2, s3) || !CanMergeByColor(s1, s3))
                {
                    Debug.Log("Một số shooter không thể merge với nhau");
                    continue;
                }

                // Tính tổng ammo
                int totalAmmo = s1.ammo + s2.ammo + s3.ammo;

                Debug.Log($"Trying to merge color group: {groupColor}");

                // Tìm ShooterType hợp nhất tương ứng bằng màu sắc
                ShooterType mergedType = mergedShooterTypes
                    .FirstOrDefault(t => ColorsMatch(t.color, groupColor));

                if (mergedType == null)
                {
                    Debug.LogError($"Không tìm thấy merged type cho màu: {groupColor}");

                    // Debug thêm thông tin về các merged types có sẵn
                    Debug.Log("Available merged types:");
                    for (int i = 0; i < mergedShooterTypes.Count; i++)
                    {
                        if (mergedShooterTypes[i] != null)
                        {
                            Debug.Log($"  [{i}]: {mergedShooterTypes[i].name} - Color: {mergedShooterTypes[i].color}");
                        }
                    }

                    continue;
                }

                // Tìm vị trí slot của shooter giữa
                int middleIndex = -1;
                for (int i = 0; i < shooterSelectionSlots.Length; i++)
                {
                    if (shooterSelectionSlots[i].childCount > 0 &&
                        shooterSelectionSlots[i].GetChild(0).gameObject == s2.gameObject)
                    {
                        middleIndex = i;
                        break;
                    }
                }

                if (middleIndex == -1)
                {
                    Debug.LogError("Không tìm thấy vị trí giữa để đặt shooter hợp nhất.");
                    continue;
                }

                Debug.Log($"Merging 3 shooters: {s1.shooterType.name}, {s2.shooterType.name}, {s3.shooterType.name}");

                // Xóa 3 shooter cũ
                Destroy(s1.gameObject);
                Destroy(s2.gameObject);
                Destroy(s3.gameObject);
                selectedShooters.Remove(s1.gameObject);
                selectedShooters.Remove(s2.gameObject);
                selectedShooters.Remove(s3.gameObject);

                // Spawn shooter mới
                GameObject mergedShooter = Instantiate(mergedType.prefab, shooterSelectionSlots[middleIndex]);
                mergedShooter.transform.localPosition = Vector3.zero;

                Shooter shooterComponent = mergedShooter.GetComponent<Shooter>();
                shooterComponent.shooterColor = groupColor;
                shooterComponent.shooterType = mergedType;
                shooterComponent.ammo = totalAmmo;
                shooterComponent.gameController = this;
                shooterComponent.isRevealed = true; // Merged shooter luôn được reveal

                // Set type để cập nhật visuals
                shooterComponent.SetType(mergedType);

                // Auto fire
                shooterComponent.AutoFire(blockGrid);

                // Thêm vào danh sách
                selectedShooters.Add(mergedShooter);

                Debug.Log($"Đã hợp nhất 3 shooter cùng màu {groupColor} thành {mergedType.name} với ammo {totalAmmo}");

                break; // Chỉ merge 1 lần mỗi lần gọi
            }
        }
    }

    private bool CanMergeByColor(Shooter s1, Shooter s2)
    {
        if (s1.shooterType == null || s2.shooterType == null)
            return false;

        // Kiểm tra xem có thể merge không
        if (!s1.shooterType.CanMerge() || !s2.shooterType.CanMerge())
            return false;

        // Kiểm tra secret shooter đã reveal chưa
        if (s1.IsSecretShooter && !s1.isRevealed)
            return false;

        if (s2.IsSecretShooter && !s2.isRevealed)
            return false;

        // So sánh màu sắc
        return ColorsMatch(s1.shooterColor, s2.shooterColor);
    }

    // Helper method so sánh màu với tolerance
    private bool ColorsMatch(Color color1, Color color2)
    {
        float tolerance = 0.01f;
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
               Mathf.Abs(color1.g - color2.g) < tolerance &&
               Mathf.Abs(color1.b - color2.b) < tolerance;
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