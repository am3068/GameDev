using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BiomePrecipitation : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private Transform player;
    [SerializeField, Min(0)] private float heightAbovePlayer = 8f;

    private ParticleSystem precipitation;
    private bool emitting;

    private void Awake()
    {
        precipitation = GetComponent<ParticleSystem>();
        // The emitter starts above the view, before the first particles fall into frame.
        var main = precipitation.main;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        precipitation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void LateUpdate()
    {
        if (!terrain || !terrain.terrainData || !player)
        {
            StopWeather();
            return;
        }

        Vector3 offset = player.position - terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        if (offset.x < 0f || offset.x > size.x || offset.z < 0f || offset.z > size.z || offset.y < -4f)
        {
            StopWeather();
            return;
        }

        transform.position = player.position + Vector3.up * heightAbovePlayer;
        if (!emitting)
        {
            precipitation.Play(true);
            emitting = true;
        }
    }

    private void OnDisable() => StopWeather();

    private void StopWeather()
    {
        if (!emitting)
            return;

        precipitation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        emitting = false;
    }
}
