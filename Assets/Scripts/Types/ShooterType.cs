using UnityEngine;

[System.Serializable]
public enum ShooterSpecialType
{
    Normal,
    Secret,      // Shooter bị che giấu màu và ammo
    Double,      // Shooter có ammo gấp đôi, không merge được
    // Có thể thêm các loại khác trong tương lai
}

[CreateAssetMenu(fileName = "New Shooter Type", menuName = "Shooter Type")]
public class ShooterType : ScriptableObject
{
    [Header("Basic Properties")]
    public string typeName;
    public Color color = Color.white;
    public GameObject prefab;
    public int baseAmmo = 20;

    [Header("Special Mechanics")]
    public ShooterSpecialType specialType = ShooterSpecialType.Normal;

    [Header("Visual Settings for Special Types")]
    public Color hiddenColor = Color.gray;      // Màu che giấu cho Secret Shooter
    public Sprite hiddenIcon;                   // Icon che giấu cho Secret Shooter
    public Color doubleShooterColor = Color.black; // Màu đặc biệt cho Double Shooter

    [Header("Ammo Settings")]
    public bool useCustomAmmo = false;
    public int customAmmo = 20;

    public int GetActualAmmo()
    {
        if (useCustomAmmo) return customAmmo;

        switch (specialType)
        {
            case ShooterSpecialType.Double:
                return baseAmmo * 2;
            default:
                return baseAmmo;
        }
    }

    public bool CanMerge()
    {
        return specialType != ShooterSpecialType.Double;
    }

    public Color GetDisplayColor(bool isRevealed = false)
    {
        if (specialType == ShooterSpecialType.Secret && !isRevealed)
            return hiddenColor;

        if (specialType == ShooterSpecialType.Double)
            return doubleShooterColor;

        return color;
    }
}
