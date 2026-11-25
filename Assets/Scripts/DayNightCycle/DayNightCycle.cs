using TMPro;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    public Light sun;

    [Header("Time Settings")]
    [Min(1f)] public float dayLengthInSeconds = 240f;

    [Range(0f, 1f)]
    public float timeOfDay = 0.25f;

    public int currentDay = 1;
    public float nightAmbientIntensity = 0.05f;
    public float dayAmbientIntensity = 1f;
    public bool tintFogWithDaylight;

    [SerializeField] private TMP_Text clockLabel;

    private float elapsedInDay;
    private int lastMinute = -1;
    private Color daytimeFog;

    void Start()
    {
        daytimeFog = RenderSettings.fogColor;
        UpdateSun();
        UpdateClockLabel();
    }

    void Update()
    {
        if (dayLengthInSeconds <= 0f)
            return;

        float dayProgress = Time.deltaTime / dayLengthInSeconds;
        timeOfDay = Mathf.Repeat(timeOfDay + dayProgress, 1f);
        elapsedInDay += dayProgress;

        int completedDays = Mathf.FloorToInt(elapsedInDay);
        if (completedDays > 0)
        {
            elapsedInDay -= completedDays;
            currentDay += completedDays;
            lastMinute = -1;

            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

            foreach (EnemyHealth enemy in enemies)
            {
                enemy.Respawn();
            }
        }

        UpdateSun();
        UpdateClockLabel();
    }

    void UpdateSun()
    {
        Shader.SetGlobalFloat("_SceneryFogEnd", RenderSettings.fog && RenderSettings.fogMode == FogMode.Linear
            ? RenderSettings.fogEndDistance : 0f);
        if (sun == null)
            return;

        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float daylight = Mathf.Clamp01(-Mathf.Cos(timeOfDay * Mathf.PI * 2f));

        sun.intensity = Mathf.Lerp(0.05f, 1f, daylight);

        Color nightColor = new Color(0.04f, 0.05f, 0.10f);
        Color dayColor = new Color(0.65f, 0.65f, 0.65f);

        RenderSettings.ambientLight = Color.Lerp(
            nightColor,
            dayColor,
            daylight
        );
        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, daylight);
        if (tintFogWithDaylight && RenderSettings.fog)
            RenderSettings.fogColor = Color.Lerp(nightColor, daytimeFog, daylight);
    }

    void UpdateClockLabel()
    {
        if (!clockLabel)
            return;

        int minute = Mathf.FloorToInt(Mathf.Repeat(timeOfDay, 1f) * 1440f);
        if (minute == lastMinute)
            return;

        lastMinute = minute;
        clockLabel.text = $"Day {currentDay}   {minute / 60:00}:{minute % 60:00}";
    }
}

