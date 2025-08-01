using UnityEngine;

[CreateAssetMenu(menuName = "Types/ShooterType")]
public class ShooterType : ScriptableObject
{
    public string typeName;
    public Color color;
    public GameObject prefab;
}
