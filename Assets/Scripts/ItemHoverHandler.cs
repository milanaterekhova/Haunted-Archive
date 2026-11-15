using UnityEngine;
using UnityEngine.EventSystems;

public class ItemHoverHandler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public ItemData itemData;
    public InventoryGrid inventoryGrid;
    public InventoryItem inventoryItem;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
            ItemTooltip.Instance.Show(itemData);

        if (inventoryGrid != null && inventoryItem != null)
            inventoryGrid.OnPlacedItemPointerEnter(inventoryItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
            ItemTooltip.Instance.Hide();

        if (inventoryGrid != null && inventoryItem != null)
            inventoryGrid.OnPlacedItemPointerExit(inventoryItem);
    }
}
