using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoringAnimationManager : MonoBehaviour
{
    [Header("UI")]
    public Canvas targetCanvas;
    public TMP_Text popupText;

    [Header("Timing")]
    [Min(0.05f)] public float eventDuration = 0.78f;
    [Min(0.01f)] public float pulseScale = 1.035f;
    public float popupVerticalOffset = 42f;

    [Header("Popup Background")]
    public bool usePopupBackground = true;
    public Color popupBackgroundColor = new Color(0.055f, 0.045f, 0.075f, 0.90f);
    public Vector2 popupPadding = new Vector2(22f, 14f);
    [Min(80f)] public float popupMinWidth = 180f;
    [Min(120f)] public float popupMaxWidth = 360f;

    private Image popupBackground;
    private RectTransform popupBackgroundRect;

    private readonly List<InventoryItem> highlightedItems = new List<InventoryItem>();
    private readonly List<Vector3> originalScales = new List<Vector3>();
    private readonly List<Image> highlightedImages = new List<Image>();
    private readonly List<Color> originalColors = new List<Color>();

    private RectTransform canvasRect;

    private void Awake()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvas == null && popupText != null)
            targetCanvas = popupText.GetComponentInParent<Canvas>();

        if (targetCanvas != null)
            canvasRect = targetCanvas.transform as RectTransform;

        if (popupText != null)
        {
            CreatePopupBackground();
            popupText.gameObject.SetActive(false);
            if (popupBackground != null)
    popupBackground.gameObject.SetActive(false);
        }
    }

    public IEnumerator PlayEvents(List<WaveReportEntry> events)
    {
        if (popupText == null)
        {
            Debug.LogWarning("ScoringAnimationManager: assign Popup Text in the Inspector.");
            yield break;
        }

        if (targetCanvas == null)
            targetCanvas = popupText.GetComponentInParent<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogWarning("ScoringAnimationManager: Popup Text must be inside a Canvas.");
            yield break;
        }

        canvasRect = targetCanvas.transform as RectTransform;

        foreach (WaveReportEntry entry in events)
        {
            if (entry == null)
                continue;

            ClearHighlight();
            Highlight(entry.involvedItems);
            ShowPopup(entry);

            float elapsed = 0f;
            while (elapsed < eventDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                AnimateHighlight(Mathf.Clamp01(elapsed / eventDuration));
                yield return null;
            }

            ClearHighlight();
            popupText.gameObject.SetActive(false);
            if (popupBackground != null)
    popupBackground.gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(0.12f);
        }
    }

    private void ShowPopup(WaveReportEntry entry)
    {
        StringBuilder message = new StringBuilder();

        message.Append("<b>");
        message.Append(entry.description);
        message.Append("</b>");

        if (entry.scoreChange != 0)
        {
            message.Append("\n<color=#FFD36A>");
            if (entry.scoreChange > 0)
                message.Append("+");
            message.Append(entry.scoreChange);
            message.Append(" SCORE</color>");
        }

        if (entry.darknessChange != 0)
        {
            message.Append("\n<color=#B99AFF>");
            if (entry.darknessChange > 0)
                message.Append("+");
            message.Append(entry.darknessChange);
            message.Append(" DARKNESS</color>");
        }

        if (entry.scoreChange == 0 && entry.darknessChange == 0)
            message.Append("\n<color=#FF8B8B>BLOCKED</color>");

        popupText.text = message.ToString();
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.gameObject.SetActive(true);

        // Keep the text comfortably readable regardless of the item artwork behind it.
        popupText.ForceMeshUpdate();
        if (popupBackgroundRect != null)
        {
            Vector2 preferred = popupText.GetPreferredValues(
                popupText.text,
                popupMaxWidth - popupPadding.x * 2f,
                0f
            );

            float width = Mathf.Clamp(
                preferred.x + popupPadding.x * 2f,
                popupMinWidth,
                popupMaxWidth
            );
            float height = preferred.y + popupPadding.y * 2f;

            popupBackgroundRect.anchorMin = popupText.rectTransform.anchorMin;
            popupBackgroundRect.anchorMax = popupText.rectTransform.anchorMax;
            popupBackgroundRect.pivot = popupText.rectTransform.pivot;
            popupBackgroundRect.anchoredPosition = popupText.rectTransform.anchoredPosition;
            popupBackgroundRect.sizeDelta = new Vector2(width, height);
            popupBackground.color = popupBackgroundColor;
            popupBackground.gameObject.SetActive(usePopupBackground);

            popupText.rectTransform.sizeDelta = new Vector2(
                width - popupPadding.x * 2f,
                height - popupPadding.y * 2f
            );
        }

        popupText.transform.SetAsLastSibling();

        Vector3 worldCenter = GetPopupAnchor(entry.involvedItems);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            targetCanvas.worldCamera,
            worldCenter
        );

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : targetCanvas.worldCamera,
            out Vector2 localPoint))
        {
            popupText.rectTransform.anchoredPosition =
                localPoint + Vector2.up * popupVerticalOffset;

            if (popupBackgroundRect != null)
                popupBackgroundRect.anchoredPosition = popupText.rectTransform.anchoredPosition;
        }
    }

private void CreatePopupBackground()
{
    if (popupText == null || popupBackground != null)
        return;

    Transform parent = popupText.transform.parent;
    if (parent == null)
        return;

    GameObject backgroundObject = new GameObject(
        "ScoringPopupBackground",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image)
    );

    popupBackgroundRect = backgroundObject.GetComponent<RectTransform>();
    popupBackground = backgroundObject.GetComponent<Image>();
    popupBackground.raycastTarget = false;
    popupBackground.color = popupBackgroundColor;

    popupBackgroundRect.SetParent(parent, false);
    popupBackgroundRect.SetSiblingIndex(popupText.transform.GetSiblingIndex());
    popupBackgroundRect.anchorMin = popupText.rectTransform.anchorMin;
    popupBackgroundRect.anchorMax = popupText.rectTransform.anchorMax;
    popupBackgroundRect.pivot = popupText.rectTransform.pivot;
    popupBackgroundRect.anchoredPosition = popupText.rectTransform.anchoredPosition;
    popupBackgroundRect.sizeDelta = popupText.rectTransform.sizeDelta;
    popupBackground.gameObject.SetActive(false);
}
    private Vector3 GetPopupAnchor(List<InventoryItem> items)
    {
        float totalCenterX = 0f;
        float highestY = float.MinValue;
        int count = 0;

        if (items != null)
        {
            foreach (InventoryItem item in items)
            {
                if (item == null || item.visual == null)
                    continue;

                Vector3[] corners = new Vector3[4];
                item.visual.GetWorldCorners(corners);
                totalCenterX += (corners[0].x + corners[2].x) * 0.5f;
                highestY = Mathf.Max(highestY, corners[1].y, corners[2].y);
                count++;
            }
        }

        if (count > 0)
            return new Vector3(totalCenterX / count, highestY, targetCanvas.transform.position.z);

        return targetCanvas.transform.position;
    }

    private void Highlight(List<InventoryItem> items)
    {
        if (items == null)
            return;

        foreach (InventoryItem item in items)
        {
            if (item == null || item.visual == null)
                continue;

            highlightedItems.Add(item);
            originalScales.Add(item.visual.localScale);
            item.visual.localScale = originalScales[originalScales.Count - 1];

            Image[] images = item.visual.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image == null)
                    continue;

                highlightedImages.Add(image);
                originalColors.Add(image.color);
                // A restrained warm tint keeps the item readable beneath the popup.
                Color softHighlight = new Color(1f, 0.96f, 0.86f, image.color.a);
                image.color = Color.Lerp(image.color, softHighlight, 0.10f);
            }
        }
    }

    private void AnimateHighlight(float progress)
    {
        float pulse = Mathf.Sin(progress * Mathf.PI);
        for (int i = 0; i < highlightedItems.Count; i++)
        {
            InventoryItem item = highlightedItems[i];
            if (item == null || item.visual == null)
                continue;

            item.visual.localScale =
                originalScales[i] * Mathf.Lerp(1f, pulseScale, pulse);
        }
    }

    private void ClearHighlight()
    {
        for (int i = 0; i < highlightedItems.Count; i++)
        {
            InventoryItem item = highlightedItems[i];
            if (item != null && item.visual != null)
                item.visual.localScale = originalScales[i];
        }

        for (int i = 0; i < highlightedImages.Count; i++)
        {
            if (highlightedImages[i] != null)
                highlightedImages[i].color = originalColors[i];
        }

        highlightedItems.Clear();
        originalScales.Clear();
        highlightedImages.Clear();
        originalColors.Clear();
    }

    private void OnDisable()
    {
        ClearHighlight();
        if (popupText != null)
            popupText.gameObject.SetActive(false);
            if (popupBackground != null)
    popupBackground.gameObject.SetActive(false);
    }
}
