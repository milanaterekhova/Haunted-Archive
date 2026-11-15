
using UnityEngine;

public enum ItemType
{
    Permanent,
    Consumable
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public Vector2Int size;
    public Color color;
    public Sprite icon;
    public ItemType itemType;

    [Header("Custom Shape")]
    public Vector2Int[] occupiedCells;

    [Header("Base Effects")]
    public int scorePerWave;
    public int darknessPerWave;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;

    public ItemData(
        string name,
        Vector2Int itemSize,
        Color itemColor,
        ItemType type,
        int score = 0,
        int darkness = 0,
        string itemDescription = "",
        Vector2Int[] shape = null,
        Sprite itemSprite = null)
    {
        itemName = name;
        size = itemSize;
        color = itemColor;
        itemType = type;
        scorePerWave = score;
        darknessPerWave = darkness;
        description = itemDescription;
        occupiedCells = shape;
        icon = itemSprite;
    }
}
