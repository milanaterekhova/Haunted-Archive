
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;

    public GameObject tooltipPanel;
    public TMP_Text nameText;
    public TMP_Text sizeText;
    public TMP_Text descriptionText;

    private RectTransform panelRect;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;

        panelRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = tooltipPanel.GetComponentInParent<Canvas>();

        tooltipPanel.SetActive(false);

        // Tooltip must never block clicks on inventory items or buttons.
        CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Position the tooltip using its top-left corner.
        panelRect.pivot = new Vector2(0f, 1f);
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
            UpdateTooltipPosition();
    }

    private void UpdateTooltipPosition()
    {
        if (panelRect == null || canvas == null)
            return;

        Canvas.ForceUpdateCanvases();

        float scale = canvas.scaleFactor;

        float panelWidth = panelRect.rect.width * scale;
        float panelHeight = panelRect.rect.height * scale;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 mouse = Input.mousePosition;

        const float offset = 18f;
        const float padding = 8f;

        // Choose left or right depending on available space.
        float x;

        if (mouse.x + offset + panelWidth <= screenWidth - padding)
            x = mouse.x + offset;
        else
            x = mouse.x - panelWidth - offset;

        // Choose above or below depending on available space.
        float y;

        if (mouse.y - offset - panelHeight >= padding)
            y = mouse.y - offset;
        else
            y = mouse.y + offset + panelHeight;

        // Keep the tooltip inside the screen.
        x = Mathf.Clamp(x, padding, screenWidth - panelWidth - padding);
        y = Mathf.Clamp(y, panelHeight + padding, screenHeight - padding);

        Vector2 screenPosition = new Vector2(x, y);

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            screenPosition,
            cam,
            out Vector3 worldPosition))
        {
            panelRect.position = worldPosition;
        }
    }

    public void Show(ItemData item)
    {
        if (item == null)
            return;

        nameText.text = item.itemName;
        sizeText.text = "Size: " + item.size.x + " x " + item.size.y;
        descriptionText.text = item.description;

        tooltipPanel.SetActive(true);

        UpdateTooltipPosition();
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
