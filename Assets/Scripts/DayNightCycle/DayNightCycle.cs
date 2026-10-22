using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    public Light sun;

    [Header("Time Settings")]
    public float dayLengthInSeconds = 120f;

    [Range(0f, 1f)]
    public float timeOfDay = 0.25f;

    public int currentDay = 1;
    public float nightAmbientIntensity = 0.05f;
    public float dayAmbientIntensity = 1f;


    void Start()
    {
        UpdateSun();
    }

    void Update()
    {
        if (sun == null || dayLengthInSeconds <= 0f)
            return;

        timeOfDay += Time.deltaTime / dayLengthInSeconds;

        if (timeOfDay >= 1f)
        {
            timeOfDay -= 1f;
            currentDay++;
            Debug.Log("Day " + currentDay);

            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

            foreach (EnemyHealth enemy in enemies)
            {
                enemy.Respawn();
            }
        }

        UpdateSun();
    }

    void UpdateSun()
    {
        if (sun == null)
            return;

        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float daylight = Mathf.Clamp01(
            Mathf.Sin(timeOfDay * Mathf.PI * 2f)
        );

        sun.intensity = Mathf.Lerp(0.05f, 1f, daylight);

        Color nightColor = new Color(0.04f, 0.05f, 0.10f);
        Color dayColor = new Color(0.65f, 0.65f, 0.65f);

        RenderSettings.ambientLight = Color.Lerp(
            nightColor,
            dayColor,
            daylight
        );

    }
}

