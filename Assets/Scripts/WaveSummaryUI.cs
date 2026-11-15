using UnityEngine;
using TMPro;

public class WaveSummaryUI : MonoBehaviour
{
    public GameObject panel;

    public TMP_Text titleText;
    public TMP_Text scoreSummaryText;
    public TMP_Text darknessSummaryText;
    public TMP_Text itemBreakdownText;

    public void ShowSummary(
        int wave,
        int score,
        int darkness,
        System.Collections.Generic.List<WaveReportEntry> entries)
    {
        panel.SetActive(true);

        titleText.text = "WAVE " + wave + " COMPLETED";
        scoreSummaryText.text = "Score: +" + score;
        darknessSummaryText.text = "Darkness: " + darkness;

        // Individual effects and combinations are now shown directly on the grid.
        // Keep the old breakdown field empty so the Summary panel stays compact.
        if (itemBreakdownText != null)
            itemBreakdownText.text = "";
    }

    public void HideSummary()
    {
        panel.SetActive(false);
    }
}