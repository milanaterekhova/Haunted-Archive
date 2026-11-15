using UnityEngine;
using UnityEngine.EventSystems;

public class OracleTargetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryGrid inventoryGrid;
    private InventoryItem oracle;
    public InventoryItem Target { get; private set; }

    public void Setup(InventoryGrid grid, InventoryItem oracleItem, InventoryItem targetItem)
    {
        inventoryGrid = grid;
        oracle = oracleItem;
        Target = targetItem;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryGrid != null)
            inventoryGrid.OnOracleButtonPointerEnter(oracle);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryGrid != null)
            inventoryGrid.OnOracleButtonPointerExit(oracle);
    }
}
