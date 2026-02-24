using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    public string nextLevelName;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTransitioning)
        {
            if (other.CompareTag("Player") && !isTransitioning)
            {
                isTransitioning = true;
                StartCoroutine(TransitionToNextLevel());
            }
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        // Start fade out effect
        yield return StartCoroutine(FadeOut());
        // Load the next level
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
    }

    private IEnumerator FadeOut()
    {
        CanvasGroup fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        if (fadeCanvasGroup != null) {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration); // Fade to black
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

    }
}
