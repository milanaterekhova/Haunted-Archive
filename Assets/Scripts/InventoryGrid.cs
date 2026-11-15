
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    public int columns = 5;
    public int rows = 5;
    public float cellSize = 90f;
    public float spacing = 3f;
    [Header("Grid Settings")]
public bool createPalette = true;

    private InventoryItem[,] occupied;
    private List<InventoryItem> placedItems = new List<InventoryItem>();
    private List<OracleSelectionUI> oracleSelectionUIs = new List<OracleSelectionUI>();
    
    public List<InventoryItem> GetPlacedItems()
{
    return placedItems;
}


public List<InventoryItem> GetAdjacentItems(InventoryItem item)
{
    List<InventoryItem> neighbours = new List<InventoryItem>();

    if (item == null || occupied == null)
        return neighbours;

    foreach (Vector2Int cell in item.occupiedCells)
    {
        int x = item.x + cell.x;
        int y = item.y + cell.y;

        CheckNeighbour(x - 1, y, item, neighbours);
        CheckNeighbour(x + 1, y, item, neighbours);
        CheckNeighbour(x, y - 1, item, neighbours);
        CheckNeighbour(x, y + 1, item, neighbours);
    }

    return neighbours;
}

private void CheckNeighbour(
    int x,
    int y,
    InventoryItem currentItem,
    List<InventoryItem> neighbours)
{
    if (x < 0 || y < 0 || x >= columns || y >= rows)
        return;

    InventoryItem neighbour = occupied[x, y];

    if (neighbour != null &&
        neighbour != currentItem &&
        !neighbours.Contains(neighbour))
    {
        neighbours.Add(neighbour);
    }
}

    private RectTransform gridArea;
    private RectTransform canvasArea;
    [Header("Grid Connection")]
public InventoryGrid otherGrid;
public bool isIncomingGrid = false;

    private InventoryItem heldItem;
private static InventoryGrid activeHoldingGrid;

private RectTransform preview;
private Image previewImage;

    private float Step => cellSize + spacing;

    private void Start()
    {
        gridArea = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("InventoryGrid must be inside a Canvas!");
            return;
        }

        canvasArea = canvas.GetComponent<RectTransform>();
        occupied = new InventoryItem[columns, rows];

        CreateGrid();

if (createPalette)
{
    CreatePalette();
}

if (isIncomingGrid)
{
    GenerateRandomItems(5);
}
    }

    private void CreateGrid()
    {
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject cell = new GameObject(
                    $"Cell_{x}_{y}",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button)
                );

                cell.transform.SetParent(gridArea, false);

                RectTransform rect = cell.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = new Vector2(x * Step, -y * Step);

                Image image = cell.GetComponent<Image>();
                image.color = new Color(0.18f, 0.18f, 0.23f);

                int cellX = x;
                int cellY = y;

                cell.GetComponent<Button>().onClick.AddListener(
                    () => PlaceHeldItem(cellX, cellY)
                );
            }
        }
    }

    
private void CreatePalette()
{
    List<ItemData> items = ItemDatabase.CreateItems();

    for (int i = 0; i < items.Count; i++)
    {
        ItemData item = items[i];

        CreatePaletteButton(
            item.itemName,
            item.size,
            item.color,
            i,
            item
        );
    }
}




public void GenerateRandomItems(int count)
{
    List<InventoryItem> oldItems =
        new List<InventoryItem>(placedItems);

    foreach (InventoryItem item in oldItems)
    {
        RemovePlacedItem(item);
    }

    List<ItemData> allItems = ItemDatabase.CreateItems();

    Dictionary<string, int> itemLimits = new Dictionary<string, int>()
    {
        { "Candle", 2 },
        { "Cursed Book", 2 },
        { "Black Raven Feather", 2 },
        { "Rose in Glass", 1 },
        { "Tarot Cards", 2 },
        { "Crystal Ball", 2 },
        { "Oracle Eye", 2 },
        { "Gold Potion", 2 },
        { "Purification Potion", 2 }
    };

    Dictionary<string, int> itemCounts =
        new Dictionary<string, int>();

    List<ItemData> generatedItems = new List<ItemData>();

    for (int i = 0; i < count; i++)
    {
        ItemData selected = GetRandomAvailableItem(
            allItems,
            new List<string>(itemLimits.Keys),
            itemLimits,
            itemCounts
        );

        if (selected != null)
        {
            generatedItems.Add(selected);
            AddGeneratedItemCount(selected, itemCounts);
        }
    }

    foreach (ItemData item in generatedItems)
    {
        AddItemToGrid(item);
    }

    Debug.Log("Generated " + generatedItems.Count + " items!");
}


private ItemData GetRandomAvailableItem(
    List<ItemData> allItems,
    List<string> allowedNames,
    Dictionary<string, int> limits,
    Dictionary<string, int> counts)
{
    List<ItemData> available = allItems.FindAll(item =>
    {
        if (!allowedNames.Contains(item.itemName))
            return false;

        int limit = limits[item.itemName];

        int currentCount = counts.ContainsKey(item.itemName)
            ? counts[item.itemName]
            : 0;

        return currentCount < limit;
    });

    if (available.Count == 0)
        return null;

    return available[Random.Range(0, available.Count)];
}

private void AddGeneratedItemCount(
    ItemData item,
    Dictionary<string, int> counts)
{
    if (!counts.ContainsKey(item.itemName))
        counts[item.itemName] = 0;

    counts[item.itemName]++;
}

    
    private void AddItemToGrid(ItemData data)
{
    if (data == null)
    {
        Debug.LogError("ItemData not found!");
        return;
    }

    InventoryItem placed = new InventoryItem(data);

    // Find the first available position
    bool foundPosition = false;

    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < columns; x++)
        {
            if (CanPlace(placed, x, y))
            {
                placed.x = x;
                placed.y = y;
                foundPosition = true;
                break;
            }
        }

        if (foundPosition)
            break;
    }

    if (!foundPosition)
    {
        Debug.LogWarning("No space for " + data.itemName);
        return;
    }

    placedItems.Add(placed);

    GameObject obj = new GameObject(
        placed.itemName,
        typeof(RectTransform),
        typeof(Image),
        typeof(Button));

    obj.transform.SetParent(gridArea, false);

    placed.visual = obj.GetComponent<RectTransform>();
    Image itemImage = obj.GetComponent<Image>();

if (data.icon != null)
{
    itemImage.sprite = data.icon;
    itemImage.color = Color.white;
    itemImage.preserveAspect = true;
}
else
{
    itemImage.color = placed.color;
}

    ItemHoverHandler hover = obj.AddComponent<ItemHoverHandler>();
    hover.itemData = data;
    hover.inventoryGrid = this;
    hover.inventoryItem = placed;

    obj.GetComponent<Button>().onClick.AddListener(() =>
{
    if (activeHoldingGrid == null)
    {
        PickUpPlacedItem(placed);
    }
    else
    {
        PlaceHeldItem(placed.x, placed.y);
    }
});
    UpdatePlacedVisual(placed);
    FillCells(placed, true);
    RefreshOracleSelectionUIs();
}
private void CreatePaletteButton(
    string itemName,
    Vector2Int size,
    Color color,
    int index,
    ItemData itemData)
{
    GameObject buttonObject = new GameObject(
        itemName,
        typeof(RectTransform),
        typeof(Image),
        typeof(Button));

        ItemHoverHandler hover = buttonObject.AddComponent<ItemHoverHandler>();
hover.itemData = itemData;

    buttonObject.transform.SetParent(canvasArea, false);

    RectTransform rect = buttonObject.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0, 1);
    rect.anchorMax = new Vector2(0, 1);
    rect.pivot = new Vector2(0, 1);
    rect.sizeDelta = new Vector2(180, 42);
    rect.anchoredPosition = new Vector2(520, -180 - index * 55);

    Image paletteImage = buttonObject.GetComponent<Image>();

if (itemData.icon != null)
{
    paletteImage.sprite = itemData.icon;
    paletteImage.color = Color.white;
    paletteImage.preserveAspect = true;
}
else
{
    paletteImage.color = color;
}

    GameObject labelObject = new GameObject(
        "Label", typeof(RectTransform), typeof(Text));

    labelObject.transform.SetParent(buttonObject.transform, false);

    RectTransform labelRect = labelObject.GetComponent<RectTransform>();
    labelRect.anchorMin = Vector2.zero;
    labelRect.anchorMax = Vector2.one;
    labelRect.offsetMin = Vector2.zero;
    labelRect.offsetMax = Vector2.zero;

    Text label = labelObject.GetComponent<Text>();
    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    label.text = itemName;
    label.fontSize = 16;
    label.color = Color.white;
    label.alignment = TextAnchor.MiddleCenter;
    label.raycastTarget = false;

    buttonObject.GetComponent<Button>().onClick.AddListener(
        () => PickUpNewItem(itemData));
}

    
private void PickUpNewItem(ItemData itemData)
{
    CancelHeldItem();

    heldItem = new InventoryItem(itemData);
    activeHoldingGrid = this;

    CreatePreview();
    UpdatePreview();
}


   private void CreatePreview()
{
    GameObject obj = new GameObject(
        "HeldItemPreview",
        typeof(RectTransform)
    );

    obj.transform.SetParent(canvasArea, false);

    preview = obj.GetComponent<RectTransform>();
    preview.anchorMin = new Vector2(0.5f, 0.5f);
    preview.anchorMax = new Vector2(0.5f, 0.5f);
    preview.pivot = new Vector2(0, 1);

    // The preview footprint stays aligned to the grid. Only this child image rotates.
    GameObject iconObject = new GameObject(
        "PreviewIcon",
        typeof(RectTransform),
        typeof(Image)
    );
    iconObject.transform.SetParent(preview, false);

    RectTransform iconRect = iconObject.GetComponent<RectTransform>();
    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
    iconRect.pivot = new Vector2(0.5f, 0.5f);
    iconRect.anchoredPosition = Vector2.zero;

    previewImage = iconObject.GetComponent<Image>();
    previewImage.raycastTarget = false;

    if (heldItem.itemData.icon != null)
    {
        previewImage.sprite = heldItem.itemData.icon;
        previewImage.color = new Color(1f, 1f, 1f, 0.75f);
        previewImage.preserveAspect = true;
    }
    else
    {
        previewImage.color = new Color(
            heldItem.color.r,
            heldItem.color.g,
            heldItem.color.b,
            0.75f
        );
    }

    bool customShape =
        heldItem.itemData.occupiedCells != null &&
        heldItem.itemData.occupiedCells.Length > 0;

    if (customShape && heldItem.itemData.icon == null)
    {
        previewImage.enabled = false;

        foreach (Vector2Int cell in heldItem.occupiedCells)
        {
            GameObject part = new GameObject(
                "PreviewShapePart",
                typeof(RectTransform),
                typeof(Image)
            );

            part.transform.SetParent(preview, false);

            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(Step, Step);
            rect.anchoredPosition = new Vector2(
                cell.x * Step,
                -cell.y * Step
            );

            Image image = part.GetComponent<Image>();
            image.color = new Color(
                heldItem.color.r,
                heldItem.color.g,
                heldItem.color.b,
                0.75f
            );
            image.raycastTarget = false;
        }
    }

    UpdatePreviewIconRotation();
}

    private OracleSelectionUI FindOracleSelectionUI(InventoryItem oracle)
    {
        return oracleSelectionUIs.Find(state => state.oracle == oracle);
    }

    private void RemoveOracleSelectionUI(InventoryItem oracle)
    {
        OracleSelectionUI state = FindOracleSelectionUI(oracle);
        if (state == null)
            return;

        foreach (GameObject button in state.buttons)
        {
            if (button != null)
                Destroy(button);
        }

        oracleSelectionUIs.Remove(state);
    }

    private void ClearOracleTargetsReferencing(InventoryItem target)
    {
        foreach (InventoryItem item in placedItems)
        {
            if (item != null && item.oracleTarget == target)
                item.oracleTarget = null;
        }
    }

    
private void RefreshOracleSelectionUIs()
{
    if (isIncomingGrid || occupied == null)
        return;

    // Remove UI for Oracle Eyes that are no longer in this inventory.
    for (int i = oracleSelectionUIs.Count - 1; i >= 0; i--)
    {
        if (!placedItems.Contains(oracleSelectionUIs[i].oracle))
            RemoveOracleSelectionUI(oracleSelectionUIs[i].oracle);
    }

    foreach (InventoryItem oracle in placedItems)
    {
        if (oracle.itemName != "Oracle Eye")
            continue;

        List<InventoryItem> neighbours = GetAdjacentItems(oracle);
        neighbours.RemoveAll(item => item.itemName == "Oracle Eye");

        OracleSelectionUI state = FindOracleSelectionUI(oracle);

        if (state == null)
        {
            state = new OracleSelectionUI();
            state.oracle = oracle;
            oracleSelectionUIs.Add(state);
        }

        if (neighbours.Count == 0)
        {
            oracle.oracleTarget = null;
        }

        else if (neighbours.Count == 1)
        {
            oracle.oracleTarget = neighbours[0];
        }
        // Multiple neighbours: keep the current selection if valid.
        else if (oracle.oracleTarget != null &&
                 !neighbours.Contains(oracle.oracleTarget))
        {
            oracle.oracleTarget = null;
        }

        List<InventoryItem> buttonTargets =
            neighbours.Count > 1
                ? neighbours
                : new List<InventoryItem>();

        RebuildOracleButtons(state, buttonTargets, true);
    }
}


    private void RebuildOracleButtons(
        OracleSelectionUI state,
        List<InventoryItem> neighbours,
        bool visible)
    {
        foreach (GameObject button in state.buttons)
        {
            if (button != null)
                Destroy(button);
        }
        state.buttons.Clear();

        foreach (InventoryItem target in neighbours)
        {
            GameObject buttonObject = new GameObject(
                "OracleTargetButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            buttonObject.transform.SetParent(gridArea, false);
            buttonObject.transform.SetAsLastSibling();

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(35f, 35f);
            rect.anchoredPosition = GetOracleButtonPosition(state.oracle, target);

            Image image = buttonObject.GetComponent<Image>();
            image.color = state.oracle.oracleTarget == target
                ? new Color(0.95f, 0.78f, 0.25f, 1f)
                : new Color(0.55f, 0.42f, 0.78f, 0.42f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;

            OracleTargetButton targetButton = buttonObject.AddComponent<OracleTargetButton>();
            targetButton.Setup(this, state.oracle, target);
            button.onClick.AddListener(() => SelectOracleTarget(state.oracle, target));

            buttonObject.SetActive(visible);
            state.buttons.Add(buttonObject);
        }
    }

    private Vector2 GetOracleButtonPosition(InventoryItem oracle, InventoryItem target)
    {
        Vector2 total = Vector2.zero;
        int contacts = 0;

        foreach (Vector2Int targetCell in target.occupiedCells)
        {
            int tx = target.x + targetCell.x;
            int ty = target.y + targetCell.y;

            foreach (Vector2Int oracleCell in oracle.occupiedCells)
            {
                int ox = oracle.x + oracleCell.x;
                int oy = oracle.y + oracleCell.y;

                if (ox == tx + 1 && oy == ty)
                    total += new Vector2((tx + 1f) * Step - spacing * 0.5f, -(ty + 0.5f) * Step);
                else if (ox == tx - 1 && oy == ty)
                    total += new Vector2(tx * Step + spacing * 0.5f, -(ty + 0.5f) * Step);
                else if (ox == tx && oy == ty + 1)
                    total += new Vector2((tx + 0.5f) * Step, -((ty + 1f) * Step - spacing * 0.5f));
                else if (ox == tx && oy == ty - 1)
                    total += new Vector2((tx + 0.5f) * Step, -(ty * Step + spacing * 0.5f));
                else
                    continue;

                contacts++;
            }
        }

        if (contacts == 0)
            return new Vector2((target.x + target.size.x * 0.5f) * Step,
                               -(target.y + target.size.y * 0.5f) * Step);

        return total / contacts;
    }

    public void SelectOracleTarget(InventoryItem oracle, InventoryItem target)
    {
        if (oracle == null || target == null || target.itemName == "Oracle Eye")
            return;
        if (!placedItems.Contains(oracle) || !placedItems.Contains(target))
            return;
        if (!GetAdjacentItems(oracle).Contains(target))
            return;

        // A new click replaces the previous target, it never adds another copy.
        oracle.oracleTarget = target;

        OracleSelectionUI state = FindOracleSelectionUI(oracle);
        if (state != null)
        {
            state.initiallyVisible = false;
            state.oracleHovered = false;
            state.buttonHovered = false;
            state.hideAt = -1f;
            SetOracleButtonsVisible(state, true);
            UpdateOracleButtonHighlights(state);
        }
    }

    public void OnPlacedItemPointerEnter(InventoryItem item)
    {
        if (item == null || item.itemName != "Oracle Eye")
            return;

        OracleSelectionUI state = FindOracleSelectionUI(item);
        if (state == null)
        {
            RefreshOracleSelectionUIs();
            state = FindOracleSelectionUI(item);
        }

        if (state == null)
            return;

        state.oracleHovered = true;
        state.hideAt = -1f;
        SetOracleButtonsVisible(state, true);
        UpdateOracleButtonHighlights(state);
    }

    public void OnPlacedItemPointerExit(InventoryItem item)
    {
        OracleSelectionUI state = FindOracleSelectionUI(item);
        if (state == null)
            return;

        state.oracleHovered = false;
        state.hideAt = Time.unscaledTime + 0.2f;
    }

    public void OnOracleButtonPointerEnter(InventoryItem oracle)
    {
        OracleSelectionUI state = FindOracleSelectionUI(oracle);
        if (state == null)
            return;

        state.buttonHovered = true;
        state.hideAt = -1f;
        SetOracleButtonsVisible(state, true);
    }

    public void OnOracleButtonPointerExit(InventoryItem oracle)
    {
        OracleSelectionUI state = FindOracleSelectionUI(oracle);
        if (state == null)
            return;

        state.buttonHovered = false;
        state.hideAt = Time.unscaledTime + 0.2f;
    }

    private void UpdateOracleSelectionUI()
    {
        for (int i = 0; i < oracleSelectionUIs.Count; i++)
        {
            OracleSelectionUI state = oracleSelectionUIs[i];
            if (!state.oracleHovered && !state.buttonHovered &&
                state.hideAt >= 0f && Time.unscaledTime >= state.hideAt)
            {
                state.hideAt = -1f;
                state.initiallyVisible = false;
                SetOracleButtonsVisible(state, false);
            }
        }
    }

    private void SetOracleButtonsVisible(OracleSelectionUI state, bool visible)
    {
        foreach (GameObject button in state.buttons)
        {
            if (button != null)
                button.SetActive(visible);
        }
    }

    private void UpdateOracleButtonHighlights(OracleSelectionUI state)
    {
        foreach (GameObject buttonObject in state.buttons)
        {
            if (buttonObject == null)
                continue;

            OracleTargetButton targetButton = buttonObject.GetComponent<OracleTargetButton>();
            Image image = buttonObject.GetComponent<Image>();
            if (targetButton == null || image == null)
                continue;

            bool selected = state.oracle.oracleTarget == targetButton.Target;
            image.color = selected
                ? new Color(0.95f, 0.78f, 0.25f, 1f)
                : new Color(0.55f, 0.42f, 0.78f, 0.42f);
        }
    }

    private void Update()
{
    UpdateOracleSelectionUI();

    if (heldItem == null)
        return;

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        CancelHeldItem();
        return;
    }

    if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
{
    heldItem.Rotate();
    UpdatePreview();
}
    UpdatePreview();

    if (Input.GetMouseButtonDown(0))
    {
        if (!IsPointerInsideGrid(this) &&
            !IsPointerInsideGrid(otherGrid))
        {
            ReturnHeldItemToIncoming();
        }
    }
}

private bool IsPointerInsideGrid(InventoryGrid grid)
{
    if (grid == null || grid.gridArea == null)
        return false;

    Camera cam = null;
    Canvas canvas = GetComponentInParent<Canvas>();

    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        cam = canvas.worldCamera;

    Vector2 localPoint;

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        grid.gridArea,
        Input.mousePosition,
        cam,
        out localPoint);

    return grid.gridArea.rect.Contains(localPoint);
}

private void ReturnHeldItemToIncoming()
{
    InventoryGrid incomingGrid = isIncomingGrid ? this : otherGrid;

    if (incomingGrid == null)
    {
        Debug.LogError("Incoming Grid is not assigned!");
        return;
    }

    // Find the first available position
    for (int y = 0; y < incomingGrid.rows; y++)
    {
        for (int x = 0; x < incomingGrid.columns; x++)
        {
            if (incomingGrid.CanPlace(heldItem, x, y))
            {
                incomingGrid.PlaceHeldItem(x, y);
                return;
            }
        }
    }

    Debug.Log("Incoming Items is full! Item remains in hands.");
}

    private void UpdatePreviewIconRotation()
{
    if (previewImage == null || heldItem == null)
        return;

    RectTransform iconRect = previewImage.rectTransform;
    iconRect.sizeDelta = new Vector2(
        heldItem.itemData.size.x * Step,
        heldItem.itemData.size.y * Step
    );
    iconRect.localEulerAngles = new Vector3(
        0f,
        0f,
        -heldItem.rotationDegrees
    );
}

    private void UpdatePreview()
{
    if (preview == null || heldItem == null)
        return;

    preview.sizeDelta = new Vector2(
        heldItem.size.x * Step,
        heldItem.size.y * Step
    );

    UpdatePreviewIconRotation();

    for (int i = 0; i < heldItem.occupiedCells.Length; i++)
{
    // Child 0 is the icon; custom shape cells start at child 1.
    if (i + 1 >= preview.childCount)
        break;

    RectTransform part = preview.GetChild(i + 1).GetComponent<RectTransform>();

    Vector2Int cell = heldItem.occupiedCells[i];

    part.anchoredPosition = new Vector2(
        cell.x * Step,
        -cell.y * Step
    );
}

    Camera cam = null;
    Canvas canvas = GetComponentInParent<Canvas>();

    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        cam = canvas.worldCamera;

    Vector2 mouseLocal;

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvasArea, Input.mousePosition, cam, out mouseLocal);

    preview.anchoredPosition = mouseLocal;

    InventoryGrid targetGrid = null;
    Vector2 gridLocal;

    bool overCurrentGrid =
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridArea, Input.mousePosition, cam, out gridLocal);

    if (overCurrentGrid && gridArea.rect.Contains(gridLocal))
    {
        targetGrid = this;
    }
    else if (otherGrid != null)
    {
        bool overOtherGrid =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                otherGrid.gridArea, Input.mousePosition, cam, out gridLocal);

        if (overOtherGrid && otherGrid.gridArea.rect.Contains(gridLocal))
            targetGrid = otherGrid;
    }

    if (targetGrid != null)
    {
        Rect rect = targetGrid.gridArea.rect;

        int x = Mathf.FloorToInt(
            (gridLocal.x - rect.xMin) / targetGrid.Step);

        int y = Mathf.FloorToInt(
            (rect.yMax - gridLocal.y) / targetGrid.Step);

        if (x >= 0 && y >= 0 &&
            x < targetGrid.columns && y < targetGrid.rows)
        {
            Vector3 snappedPosition = targetGrid.gridArea.TransformPoint(
    new Vector3(
        rect.xMin + (x - heldItem.grabOffset.x) * targetGrid.Step,
        rect.yMax - (y - heldItem.grabOffset.y) * targetGrid.Step,
        0));

            preview.position = snappedPosition;

            bool valid = targetGrid.CanPlace(heldItem, x, y);

            previewImage.color = valid
                ? new Color(0.25f, 0.9f, 0.4f, 0.75f)
                : new Color(1f, 0.2f, 0.2f, 0.75f);

            return;
        }
    }

    previewImage.color = new Color(
        heldItem.color.r,
        heldItem.color.g,
        heldItem.color.b,
        0.75f);
}

    private void PlaceHeldItem(int x, int y)
{
    if (activeHoldingGrid == null)
        return;

    InventoryGrid sourceGrid = activeHoldingGrid;
    InventoryItem movingItem = sourceGrid.heldItem;

    if (movingItem == null)
        return;

        x -= movingItem.grabOffset.x;
        y -= movingItem.grabOffset.y;

    List<InventoryItem> overlappingItems = new List<InventoryItem>();

foreach (Vector2Int cell in movingItem.occupiedCells)
{
    int gridX = x + cell.x;
    int gridY = y + cell.y;

    if (gridX < 0 || gridY < 0 ||
        gridX >= columns || gridY >= rows)
    {
        Debug.Log("There is not enough space!");
        return;
    }

    InventoryItem occupant = occupied[gridX, gridY];

    if (occupant != null &&
        !overlappingItems.Contains(occupant))
    {
        overlappingItems.Add(occupant);
    }
}

    if (overlappingItems.Count > 1)
    {
        Debug.Log("You can only replace one item at a time!");
        return;
    }

    InventoryItem displacedItem = null;

    if (overlappingItems.Count == 1)
    {
        displacedItem = overlappingItems[0];

        if (!CanPlaceIgnoring(movingItem, x, y, displacedItem))
        {
            Debug.Log("The new item does not fit!");
            return;
        }

        ClearOracleTargetsReferencing(displacedItem);
        RemoveOracleSelectionUI(displacedItem);
        placedItems.Remove(displacedItem);
        FillCells(displacedItem, false);

        if (displacedItem.visual != null)
            Destroy(displacedItem.visual.gameObject);

        displacedItem.visual = null;
    }
    else
    {
        if (!CanPlace(movingItem, x, y))
        {
            Debug.Log("There is not enough free space!");
            return;
        }
    }

    InventoryItem placed = new InventoryItem(movingItem.itemData);

placed.size = movingItem.size;
placed.occupiedCells = (Vector2Int[])movingItem.occupiedCells.Clone();
placed.tarotChoice = movingItem.tarotChoice;
placed.rotationDegrees = movingItem.rotationDegrees;

placed.x = x;
placed.y = y;

    placedItems.Add(placed);

    GameObject obj = new GameObject(
        placed.itemName,
        typeof(RectTransform),
        typeof(Image),
        typeof(Button));

    obj.transform.SetParent(gridArea, false);

    placed.visual = obj.GetComponent<RectTransform>();
    Image itemImage = obj.GetComponent<Image>();

if (placed.itemData.icon != null)
{
    itemImage.sprite = placed.itemData.icon;
    itemImage.color = Color.white;
    itemImage.preserveAspect = true;
}
else
{
    itemImage.color = placed.color;
}

    ItemHoverHandler hover = obj.AddComponent<ItemHoverHandler>();
    hover.itemData = placed.itemData;
    hover.inventoryGrid = this;
    hover.inventoryItem = placed;

    obj.GetComponent<Button>().onClick.AddListener(() =>
{
    if (activeHoldingGrid == null)
    {
        PickUpPlacedItem(placed);
    }
    else
    {
        PlaceHeldItem(placed.x, placed.y);
    }
});

    UpdatePlacedVisual(placed);
    FillCells(placed, true);

    // If we replaced an item, pick up the displaced item visually as well.
    // Rebuild the preview because the previous preview still contains the
    // sprite and child shape parts of the item that was just placed.
    if (displacedItem != null)
    {
        sourceGrid.heldItem = displacedItem;

        if (sourceGrid.preview != null)
            Destroy(sourceGrid.preview.gameObject);

        sourceGrid.preview = null;
        sourceGrid.previewImage = null;

        sourceGrid.CreatePreview();
        sourceGrid.UpdatePreview();
    }
    else
    {
        if (sourceGrid.preview != null)
            Destroy(sourceGrid.preview.gameObject);

        sourceGrid.preview = null;
        sourceGrid.previewImage = null;
        sourceGrid.heldItem = null;

        activeHoldingGrid = null;
    }

    RefreshOracleSelectionUIs();
}

    private void PickUpPlacedItem(InventoryItem item)
{
    Vector2 localPoint;

    Canvas canvas = GetComponentInParent<Canvas>();
    Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
        ? null
        : canvas.worldCamera;

    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        gridArea,
        Input.mousePosition,
        cam,
        out localPoint))
    {
        Rect rect = gridArea.rect;

        int clickedX = Mathf.FloorToInt(
            (localPoint.x - rect.xMin) / Step);

        int clickedY = Mathf.FloorToInt(
            (rect.yMax - localPoint.y) / Step);

        item.grabOffset = new Vector2Int(
            clickedX - item.x,
            clickedY - item.y
        );
    }

    ClearOracleTargetsReferencing(item);
    RemoveOracleSelectionUI(item);
    placedItems.Remove(item);
    FillCells(item, false);
    RefreshOracleSelectionUIs();

    heldItem = item;
    activeHoldingGrid = this;

    if (item.visual != null)
        Destroy(item.visual.gameObject);

    item.visual = null;

    CreatePreview();
    UpdatePreview();
}

    

private void UpdatePlacedVisual(InventoryItem item)
{
    item.visual.anchorMin = new Vector2(0, 1);
    item.visual.anchorMax = new Vector2(0, 1);
    item.visual.pivot = new Vector2(0, 1);

    item.visual.anchoredPosition = new Vector2(
        item.x * Step,
        -item.y * Step
    );

    item.visual.sizeDelta = new Vector2(
        item.size.x * Step,
        item.size.y * Step
    );

    bool customShape =
        item.itemData.occupiedCells != null &&
        item.itemData.occupiedCells.Length > 0;

    Image rootImage = item.visual.GetComponent<Image>();

    // The root spans the full bounding rectangle, so it must not intercept clicks in empty shape cells.
    rootImage.raycastTarget = false;

    if (item.itemData.icon != null)
    {
        rootImage.sprite = null;
        rootImage.color = new Color(1f, 1f, 1f, 0f);

        GameObject iconObject = new GameObject(
            "RotatingItemIcon",
            typeof(RectTransform),
            typeof(Image)
        );
        iconObject.transform.SetParent(item.visual, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(
            item.itemData.size.x * Step,
            item.itemData.size.y * Step
        );
        iconRect.localEulerAngles = new Vector3(
            0f,
            0f,
            -item.rotationDegrees
        );

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = item.itemData.icon;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }
    else if (customShape)
    {
        rootImage.enabled = false;

        foreach (Vector2Int cell in item.occupiedCells)
        {
            GameObject part = new GameObject(
                "ShapePart",
                typeof(RectTransform),
                typeof(Image)
            );

            part.transform.SetParent(item.visual, false);

            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(Step, Step);
            rect.anchoredPosition = new Vector2(
                cell.x * Step,
                -cell.y * Step
            );

            Image image = part.GetComponent<Image>();
            image.color = item.color;
            image.raycastTarget = false;
        }
    }
    else
    {
        rootImage.enabled = true;
        rootImage.color = item.color;
    }

    // Add click areas only to genuinely occupied cells.
    // These child Images route clicks to the Button on the parent item,
    // while the empty part of an L-shape remains clickable as grid space.
    foreach (Vector2Int cell in item.occupiedCells)
    {
        GameObject hitArea = new GameObject(
            "OccupiedCellHitArea",
            typeof(RectTransform),
            typeof(Image)
        );

        hitArea.transform.SetParent(item.visual, false);

        RectTransform hitRect = hitArea.GetComponent<RectTransform>();
        hitRect.anchorMin = new Vector2(0, 1);
        hitRect.anchorMax = new Vector2(0, 1);
        hitRect.pivot = new Vector2(0, 1);
        hitRect.sizeDelta = new Vector2(Step, Step);
        hitRect.anchoredPosition = new Vector2(
            cell.x * Step,
            -cell.y * Step
        );

        Image hitImage = hitArea.GetComponent<Image>();
        hitImage.color = new Color(1f, 1f, 1f, 0f);
        hitImage.raycastTarget = true;
    }
}


    private bool CanPlace(InventoryItem item, int x, int y)
{
    foreach (Vector2Int cell in item.occupiedCells)
    {
        int gridX = x + cell.x;
        int gridY = y + cell.y;

        if (gridX < 0 || gridY < 0 ||
            gridX >= columns || gridY >= rows)
        {
            return false;
        }

        if (occupied[gridX, gridY] != null)
        {
            return false;
        }
    }

    return true;
}

private bool CanPlaceIgnoring(
    InventoryItem item,
    int x,
    int y,
    InventoryItem ignoredItem)
{
    foreach (Vector2Int cell in item.occupiedCells)
    {
        int gridX = x + cell.x;
        int gridY = y + cell.y;

        if (gridX < 0 || gridY < 0 ||
            gridX >= columns || gridY >= rows)
        {
            return false;
        }

        InventoryItem occupant = occupied[gridX, gridY];

        if (occupant != null && occupant != ignoredItem)
        {
            return false;
        }
    }

    return true;
}
    private void FillCells(InventoryItem item, bool fill)
{
    foreach (Vector2Int cell in item.occupiedCells)
    {
        int gridX = item.x + cell.x;
        int gridY = item.y + cell.y;

        occupied[gridX, gridY] = fill ? item : null;
    }
}

   
public void RemovePlacedItem(InventoryItem item)
{
    if (item == null || !placedItems.Contains(item))
        return;

    ClearOracleTargetsReferencing(item);
    RemoveOracleSelectionUI(item);
    FillCells(item, false);

    placedItems.Remove(item);

    if (item.visual != null)
    {
        Destroy(item.visual.gameObject);
    }

    RefreshOracleSelectionUIs();
}

private void CancelHeldItem()
{
    if (preview != null)
        Destroy(preview.gameObject);

    preview = null;
    previewImage = null;
    heldItem = null;
    if (activeHoldingGrid == this)
    activeHoldingGrid = null;
}
private class OracleSelectionUI
    {
        public InventoryItem oracle;
        public List<GameObject> buttons = new List<GameObject>();

        public bool initiallyVisible = true;
        public bool oracleHovered = false;
        public bool buttonHovered = false;

        public float hideAt = -1f;
    }
}

public class InventoryItem
{
    public string itemName;
    public int tarotChoice = 0;
    public Vector2Int grabOffset = Vector2Int.zero;
    public Vector2Int size;
    public Color color;
    public int x;
    public int y;
    public RectTransform visual;

    public ItemData itemData;
    public Vector2Int[] occupiedCells;
    public InventoryItem oracleTarget;
    public int rotationDegrees = 0;

    public InventoryItem(ItemData data)
{
    itemData = data;
    itemName = data.itemName;
    size = data.size;
    color = data.color;

    if (data.occupiedCells != null && data.occupiedCells.Length > 0)
    {
        occupiedCells = (Vector2Int[])data.occupiedCells.Clone();
    }
    else
    {
        occupiedCells = new Vector2Int[size.x * size.y];

        int index = 0;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupiedCells[index] = new Vector2Int(x, y);
                index++;
            }
        }
    }
}

public void Rotate()
{
    rotationDegrees = (rotationDegrees + 90) % 360;

    Vector2Int oldSize = size;
    grabOffset = new Vector2Int(
    oldSize.y - 1 - grabOffset.y,
    grabOffset.x
);

    for (int i = 0; i < occupiedCells.Length; i++)
    {
        Vector2Int cell = occupiedCells[i];

        occupiedCells[i] = new Vector2Int(
            oldSize.y - 1 - cell.y,
            cell.x
        );
    }

    size = new Vector2Int(oldSize.y, oldSize.x);
}

    public InventoryItem(string name, Vector2Int itemSize, Color itemColor)
    {
        itemName = name;
        size = itemSize;
        color = itemColor;
    }
}