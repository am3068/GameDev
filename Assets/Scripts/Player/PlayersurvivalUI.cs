
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurvivalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSurvival playerSurvival;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;

    [Header("Fade Transition")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool gameOverStarted;

    private void Start()
    {
        if (playerSurvival == null ||
            healthSlider == null ||
            hungerSlider == null ||
            gameOverPanel == null ||
            gameOverCanvasGroup == null)
        {
            Debug.LogError(
                "PlayerSurvivalUI: Assign all references in the Inspector."
            );
            enabled = false;
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = playerSurvival.MaxHealth;

        hungerSlider.minValue = 0;
        hungerSlider.maxValue = playerSurvival.MaxHunger;

        gameOverPanel.SetActive(false);
        gameOverCanvasGroup.alpha = 0f;

        UpdateBars();
    }

    private void Update()
    {
        UpdateBars();

        if (playerSurvival.IsDead && !gameOverStarted)
        {
            gameOverStarted = true;
            StartCoroutine(FadeToBlack());
        }
    }

    private void UpdateBars()
    {
        healthSlider.value = playerSurvival.CurrentHealth;
        hungerSlider.value = playerSurvival.CurrentHunger;
    }
    // Death screen fade 
    private IEnumerator FadeToBlack()
    {
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            gameOverCanvasGroup.alpha =
                Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
    }
}
