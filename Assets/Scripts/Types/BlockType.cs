using UnityEngine;

[CreateAssetMenu(menuName = "Types/BlockType")]
public class BlockType : ScriptableObject
{
    public string typeName;
    public GameObject subBlockPrefab;
    public Color color;
    public GameObject prefab;
    public int startingFloor = 3;

    public bool isFrozen = false; // Block này có bị đóng băng không
    public int unlockAfterShooterCount = 12; // Số block shooter cần ăn để mở 
}