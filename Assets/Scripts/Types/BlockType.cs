using UnityEngine;

[CreateAssetMenu(menuName = "Types/BlockType")]
public class BlockType : ScriptableObject
{
    public string typeName;
    public GameObject subBlockPrefab;
    public Color color;
    public GameObject prefab;
    public int startingFloor = 3;
}