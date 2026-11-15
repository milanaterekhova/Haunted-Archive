using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public WaveSummaryUI waveSummaryUI;
    public EndTurnButton endTurnButton;
    public InventoryGrid incomingGrid;
    public GameObject victoryPanel;
    public TMP_Text finalScoreText;

    private void Start()
    {
        Debug.Log("Current Wave: " + GameManager.Instance.currentWave);
    }

    public void StartNextWave()
    {
        if (GameManager.Instance.currentWave < GameManager.Instance.maxWaves)
        {
            GameManager.Instance.currentWave++;

endTurnButton.ResetWaveData();

incomingGrid.GenerateRandomItems(5);

waveSummaryUI.HideSummary();

Debug.Log("Wave " + GameManager.Instance.currentWave + " started!");
            Debug.Log("Wave " + GameManager.Instance.currentWave + " started!");
        }
        else
{
    waveSummaryUI.HideSummary();

    victoryPanel.SetActive(true);

    finalScoreText.text =
        "FINAL SCORE: " + GameManager.Instance.score;

    Debug.Log("You survived all waves! You win!");
}
    }
}