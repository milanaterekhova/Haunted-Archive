
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1.2f;

    private bool isTransitioning = false;

    private void Start()
    {
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;

        StartCoroutine(Fade(1f, 0f));
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        Color color = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(start, end, elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = end;
        fadePanel.color = color;
    }
}
