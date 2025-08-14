using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Types/BlockType")]
public class BlockType : ScriptableObject
{
    public string typeName;
    public GameObject subBlockPrefab;
    public Color color;
    public GameObject prefab;
    public int startingFloor = 3;
    [Header("Freeze Settings")]
    public bool startsFrozen = false; // Block này bắt đầu bị đóng băng
    public int blocksToUnfreeze = 5; // Số block cần phá để unlock block này
}