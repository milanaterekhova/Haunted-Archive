
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Stats")]
    public int score = 0;
    public int darkness = 0;
    public int currentWave = 1;
    public int maxWaves = 5;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text darknessText;
    public TMP_Text waveText;
    public TMP_Text scoreTargetText;
    public TMP_Text waveScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
        UpdateWaveScoreUI(0);
    }

    public int GetScoreTarget(int wave)
    {
        switch (wave)
        {
            case 1: return 8;
            case 2: return 15;
            case 3: return 25;
            case 4: return 35;
            case 5: return 45;
            default: return 45;
        }
    }

    public int GetScoreTarget()
    {
        return GetScoreTarget(currentWave);
    }

    public int GetWaveDarknessBonus(int wave)
    {
        switch (wave)
        {
            case 1: return 3;
            case 2: return 5;
            case 3: return 7;
            case 4: return 9;
            case 5: return 12;
            default: return 12;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void AddDarkness(int amount)
    {
        darkness = Mathf.Max(0, darkness + amount);
        UpdateUI();
    }

    public void UpdateWaveScoreUI(int currentScore)
    {
        if (waveScoreText != null)
        {
            waveScoreText.text = "Wave Score: " + currentScore;
        }

        if (scoreTargetText != null)
        {
            scoreTargetText.text = "Target Score: " + GetScoreTarget();
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Total Score: " + score;

        if (darknessText != null)
            darknessText.text = "Darkness: " + darkness + " / 100";

        if (waveText != null)
            waveText.text = "Wave: " + currentWave + " / " + maxWaves;

        if (scoreTargetText != null)
            scoreTargetText.text = "Target Score: " + GetScoreTarget();
    }

    public void RestartGame()
    {
        score = 0;
        darkness = 0;
        currentWave = 1;

        UpdateUI();
        UpdateWaveScoreUI(0);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
