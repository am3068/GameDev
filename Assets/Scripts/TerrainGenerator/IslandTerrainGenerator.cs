
using UnityEngine;

public class IslandTerrainGenerator : MonoBehaviour
{
    public Terrain terrain;

    public int seed = 12345;
    public float noiseScale = 3.5f;
    public float height = 0.10f;
    public float edgeHeight = 0.30f;

    [ContextMenu("Generate Island")]
    void GenerateIsland()
    {
        if (terrain == null)
        {
            Debug.LogError("Assign your terrain first!");
            return;
        }

        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;

        float[,] heights = new float[resolution, resolution];

        float offsetX = seed * 0.1f;
        float offsetZ = seed * 0.2f;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float xPosition = (float)x / (resolution - 1);
                float zPosition = (float)z / (resolution - 1);

                float noise = Mathf.PerlinNoise(
                    xPosition * noiseScale + offsetX,
                    zPosition * noiseScale + offsetZ
                );

                // Generate mountains on three sides
                float left = 1f - Mathf.Clamp01(xPosition / 0.2f);
                float right = 1f - Mathf.Clamp01((1f - xPosition) / 0.2f);
                float far = 1f - Mathf.Clamp01((1f - zPosition) / 0.2f);

                float mountains = Mathf.Max(left, Mathf.Max(right, far));

                // Keep the bridge side low
                float bridgeSide = Mathf.Clamp01(zPosition / 0.12f);

                float normalHeight = 0.02f + noise * height;

                heights[z, x] = normalHeight * bridgeSide
                    + mountains * edgeHeight * bridgeSide;
            }
        }

        data.SetHeights(0, 0, heights);

        Debug.Log("Island generated!");
    }
}
