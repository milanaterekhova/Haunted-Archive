using System.Collections.Generic;

[System.Serializable]
public class WaveReportEntry
{
    public string description;
    public int scoreChange;
    public int darknessChange;
    public List<InventoryItem> involvedItems;

    public WaveReportEntry(
        string description,
        int scoreChange,
        int darknessChange,
        params InventoryItem[] items)
    {
        this.description = description;
        this.scoreChange = scoreChange;
        this.darknessChange = darknessChange;
        involvedItems = items != null
            ? new List<InventoryItem>(items)
            : new List<InventoryItem>();
    }
}
