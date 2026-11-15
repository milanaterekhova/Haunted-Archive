using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EndTurnButton : MonoBehaviour
{
    public InventoryGrid inventoryGrid;
    public GameObject tarotChoicePanel;
    public WaveSummaryUI waveSummaryUI;
    public GameObject gameOverPanel;
    public TMP_Text gameOverScoreText;
    public ScoringAnimationManager scoringAnimationManager;

    private bool isResolving = false;

    private readonly List<WaveReportEntry> waveEntries = new List<WaveReportEntry>();
    private int waveScore = 0;
    private int waveDarkness = 0;
    private List<InventoryItem> pendingTarotCards = new List<InventoryItem>();
    private int currentTarotIndex = 0;

    public void EndTurn()
{
    if (isResolving)
        return;

    if (inventoryGrid == null)
    {
        Debug.LogError("InventoryGrid is not assigned!");
        return;
    }

    pendingTarotCards = inventoryGrid.GetPlacedItems()
        .FindAll(item => item.itemName == "Tarot Cards");

    currentTarotIndex = 0;

    if (pendingTarotCards.Count > 0)
    {
        if (tarotChoicePanel == null)
        {
            Debug.LogError("Tarot Choice Panel is not assigned!");
            return;
        }

        tarotChoicePanel.SetActive(true);
        return;
    }

    ResolveTurn();
}

    public void ChooseTarotScore()
{
    SetCurrentTarotChoice(1);
}

public void ChooseTarotDarkness()
{
    SetCurrentTarotChoice(2);
}

private void SetCurrentTarotChoice(int choice)
{
    if (currentTarotIndex >= pendingTarotCards.Count)
        return;

    pendingTarotCards[currentTarotIndex].tarotChoice = choice;
    currentTarotIndex++;

    if (currentTarotIndex < pendingTarotCards.Count)
    {
        return;
    }

    tarotChoicePanel.SetActive(false);
    ResolveTurn();
}

    private static int GetHalfRoundedAwayFromZero(int value)
    {
        if (value == 0)
            return 0;

        int half = Mathf.CeilToInt(Mathf.Abs(value) / 2f);
        return value < 0 ? -half : half;
    }

    private void ResolveTurn()
    {
        if (isResolving)
            return;
        isResolving = true;

        List<InventoryItem> items = new List<InventoryItem>(inventoryGrid.GetPlacedItems());
        int scoreGained = 0;
        int darknessChange = 0;
        waveEntries.Clear();
        waveScore = 0;
        waveDarkness = 0;

        List<InventoryItem> consumedItems = new List<InventoryItem>();
        Dictionary<InventoryItem, Vector2Int> itemEffects =
            new Dictionary<InventoryItem, Vector2Int>();

        foreach (InventoryItem item in items)
        {
            int itemScore = 0;
            int itemDarkness = 0;
            string description = item.itemName;
            List<InventoryItem> neighbours = inventoryGrid.GetAdjacentItems(item);

            switch (item.itemName)
            {
                
case "Candle":
{
    // The candle's own effect is always -2 Darkness.
    // An adjacent Rose adds a separate -1 synergy event below.
    itemDarkness = -2;
    break;
}


                case "Cursed Book":
                {
                    // The book gives +5 Score by itself.
                    // An adjacent Feather adds a separate +3 synergy event below.
                    itemScore = 5;
                    itemDarkness = 5;
                    break;
                }

                case "Black Raven Feather":
                    itemScore = 3;
                    itemDarkness = 3;
                    break;

                case "Rose in Glass":
                    itemScore = 2;
                    itemDarkness = -3;
                    break;

                case "Crystal Ball":
                {
                    int randomEffect = Random.Range(0, 100);
                    if (randomEffect < 40)
                    {
                        itemScore = 20;
                        description = "Crystal Ball: +20 Score";
                    }
                    else if (randomEffect < 70)
                    {
                        itemDarkness = -10;
                        description = "Crystal Ball: -10 Darkness";
                    }
                    else
                    {
                        itemDarkness = 8;
                        description = "Crystal Ball: +8 Darkness";
                    }
                    break;
                }

                case "Gold Potion":
                    itemScore = 5;
                    consumedItems.Add(item);
                    break;

                case "Purification Potion":
                {
                    bool hasAdjacentBook = neighbours.Exists(n => n.itemName == "Cursed Book");
                    itemDarkness = hasAdjacentBook ? -3 : -5;
                    if (hasAdjacentBook)
                        description = "Purification Potion + Book";
                    consumedItems.Add(item);
                    break;
                }

                case "Tarot Cards":
{
    if (item.tarotChoice == 1)
    {
        itemScore = 10;
        itemDarkness = 5;
        description = "Tarot Cards: +10 Score, +5 Darkness";
    }
    else if (item.tarotChoice == 2)
    {
        itemDarkness = -8;
        description = "Tarot Cards: -8 Darkness";
    }
    break;
}

                case "Oracle Eye":
                    // Oracle effects are applied after all regular item effects are calculated.
                    break;
            }

            itemEffects[item] = new Vector2Int(itemScore, itemDarkness);
            scoreGained += itemScore;
            darknessChange += itemDarkness;

            if (itemScore != 0 || itemDarkness != 0)
            {
                waveEntries.Add(new WaveReportEntry(
                    description,
                    itemScore,
                    itemDarkness,
                    item
                ));
            }
        }

        // Resolve and record one Rose bonus per Candle, matching the original rules.
        foreach (InventoryItem candle in items)
        {
            if (candle.itemName != "Candle")
                continue;

            List<InventoryItem> adjacentRoses = inventoryGrid.GetAdjacentItems(candle)
                .FindAll(n => n.itemName == "Rose in Glass");

            if (adjacentRoses.Count == 0)
                continue;

            InventoryItem activeRose = null;
            foreach (InventoryItem rose in adjacentRoses)
            {
                bool roseBlocked = inventoryGrid.GetAdjacentItems(rose)
                    .Exists(n => n.itemName == "Black Raven Feather");

                if (!roseBlocked)
                {
                    activeRose = rose;
                    break;
                }
            }

            if (activeRose == null)
            {
                waveEntries.Add(new WaveReportEntry(
                    "Rose bonus blocked by Raven Feather",
                    0,
                    0,
                    candle,
                    adjacentRoses[0]
                ));
                continue;
            }

            darknessChange -= 1;

            Vector2Int candleEffect = itemEffects[candle];
            itemEffects[candle] = new Vector2Int(
                candleEffect.x,
                candleEffect.y - 1
            );

            waveEntries.Add(new WaveReportEntry(
                "Rose Blessing",
                0,
                -1,
                candle,
                activeRose
            ));
        }

        foreach (InventoryItem book in items)
        {
            if (book.itemName != "Cursed Book")
                continue;

            bool hasFeather = inventoryGrid.GetAdjacentItems(book)
                .Exists(n => n.itemName == "Black Raven Feather");

            if (!hasFeather)
                continue;

            scoreGained += 3;

            Vector2Int bookEffect = itemEffects[book];
            itemEffects[book] = new Vector2Int(
                bookEffect.x + 3,
                bookEffect.y
            );

            waveEntries.Add(new WaveReportEntry(
                "Raven Feather synergy",
                3,
                0,
                book,
                inventoryGrid.GetAdjacentItems(book)
                    .Find(n => n.itemName == "Black Raven Feather")
            ));
        }

        // Each Oracle copies 50% of exactly one selected adjacent item's original effect.
        // Copied effects are not added to itemEffects, so they cannot be copied again.
        foreach (InventoryItem oracle in items)
        {
            if (oracle.itemName != "Oracle Eye" || oracle.oracleTarget == null)
                continue;

            InventoryItem target = oracle.oracleTarget;
            if (target.itemName == "Oracle Eye" || !items.Contains(target) ||
                !inventoryGrid.GetAdjacentItems(oracle).Contains(target) ||
                !itemEffects.ContainsKey(target))
            {
                oracle.oracleTarget = null;
                continue;
            }

            Vector2Int originalEffect = itemEffects[target];
            int copiedScore = GetHalfRoundedAwayFromZero(originalEffect.x);
            int copiedDarkness = GetHalfRoundedAwayFromZero(originalEffect.y);

            scoreGained += copiedScore;
            darknessChange += copiedDarkness;

            if (copiedScore != 0 || copiedDarkness != 0)
            {
                waveEntries.Add(new WaveReportEntry(
                    "Oracle Eye copied " + target.itemName,
                    copiedScore,
                    copiedDarkness,
                    oracle,
                    target
                ));
            }
        }

        waveScore += scoreGained;
        waveDarkness += darknessChange;

        Debug.Log("Turn ended! Score gained: " + scoreGained +
                  ", Darkness change: " + darknessChange);

        StartCoroutine(PlayScoringSequence(consumedItems, scoreGained, darknessChange));
    }

    
private IEnumerator PlayScoringSequence(
    List<InventoryItem> consumedItems,
    int scoreGained,
    int darknessChange)
{
    // Show individual items and synergies on the grid first.
    if (scoringAnimationManager != null)
        yield return scoringAnimationManager.PlayEvents(waveEntries);
    else
        yield return new WaitForSecondsRealtime(0.15f);

    // Apply the actual totals once, after the explanatory callouts.
    GameManager.Instance.AddScore(scoreGained);
    GameManager.Instance.AddDarkness(darknessChange);

    foreach (InventoryItem item in consumedItems)
        inventoryGrid.RemovePlacedItem(item);

    // Quickly count the final Wave Score, then show the summary.
    const float countDuration = 0.5f;
    float elapsed = 0f;
    int displayedScore = 0;
    GameManager.Instance.UpdateWaveScoreUI(0);

    while (elapsed < countDuration)
    {
        elapsed += Time.unscaledDeltaTime;
        int nextScore = Mathf.RoundToInt(
            Mathf.Lerp(0f, waveScore, Mathf.Clamp01(elapsed / countDuration))
        );

        if (nextScore != displayedScore)
        {
            displayedScore = nextScore;
            GameManager.Instance.UpdateWaveScoreUI(displayedScore);
        }

        yield return null;
    }

    GameManager.Instance.UpdateWaveScoreUI(waveScore);
    yield return new WaitForSecondsRealtime(0.35f);

    EndWave();
}

public void EndWave()
{
    int currentWave = GameManager.Instance.currentWave;
    int scoreTarget = GameManager.Instance.GetScoreTarget(currentWave);

    if (waveScore < scoreTarget)
    {
        Debug.Log("GAME OVER! Score target not reached.");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText != null)
            {
                gameOverScoreText.text =
                "Total Score: " + GameManager.Instance.score;
            }
        }
        else
        {
            Debug.LogError("GameOverPanel is not assigned!");
        }

        return;
    }

    // Add the Darkness penalty for completing this wave.
    int darknessBonus =
        GameManager.Instance.GetWaveDarknessBonus(currentWave);

    GameManager.Instance.AddDarkness(darknessBonus);

    waveDarkness += darknessBonus;

    waveEntries.Add(new WaveReportEntry(
        "Wave difficulty penalty",
        0,
        darknessBonus
    ));

    // Check whether Darkness reached 100.
    if (GameManager.Instance.darkness >= 100)
    {
        Debug.Log("GAME OVER! Darkness reached 100!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText != null)
            {
                gameOverScoreText.text =
                "Total Score: " + GameManager.Instance.score;
            }
        }
        else
        {
            Debug.LogError("GameOverPanel is not assigned!");
        }

        return;
    }

    if (waveSummaryUI == null)
    {
        waveSummaryUI =
            FindFirstObjectByType<WaveSummaryUI>(FindObjectsInactive.Include);
    }

    if (waveSummaryUI == null)
    {
        Debug.LogError("WaveSummaryUI was not found in the scene!");
        return;
    }

    waveSummaryUI.ShowSummary(
        currentWave,
        waveScore,
        waveDarkness,
        waveEntries
    );

    waveSummaryUI.scoreSummaryText.text =
    "Wave Score: +" + waveScore +
    "\nTarget Score: " + scoreTarget +
    "\nTotal Score: " + GameManager.Instance.score;
}


    public void ResetWaveData()
    {
        isResolving = false;
        waveScore = 0;
        waveDarkness = 0;
        waveEntries.Clear();
        GameManager.Instance.UpdateWaveScoreUI(0);
        Debug.Log("Wave data reset!");
    }
}
