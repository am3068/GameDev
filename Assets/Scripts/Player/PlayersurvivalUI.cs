using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurvivalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSurvival playersurvival;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;

    private void Start()
    {
        if (playersurvival == null ||
            healthSlider == null ||
            hungerSlider == null)
        {
            Debug.LogError(
                "PlayerSurvivalUI: Assign the player and both sliders."
            );

            enabled = false;
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = playersurvival.MaxHealth;

        hungerSlider.minValue = 0;
        hungerSlider.maxValue = playersurvival.MaxHunger;

        UpdateBars();
    }

    private void Update()
    {
        UpdateBars();
    }

    private void UpdateBars()
    {
        healthSlider.value = playersurvival.CurrentHealth;
        hungerSlider.value = playersurvival.CurrentHunger;
    }
}

