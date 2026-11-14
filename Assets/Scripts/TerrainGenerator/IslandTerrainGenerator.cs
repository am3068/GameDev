
using UnityEngine;

public class IslandTerrainGenerator : MonoBehaviour
{
    public Terrain islandTerrain;
    [Header("Island Settings")]
    public int seed = 12345;
    public float noiseScale = 3f;
    public float islandHeight = 0.15f;
    [Header("Terrain Textures")]
    public TerrainLayer[] groundTextures;
    [ContextMenu("Generate Island")]
    void GenerateIsland()
    {
        if (islandTerrain == null)
            return;
        TerrainData terrainData = islandTerrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        float randomX = seed * 0.1f;
        float randomZ = seed * 0.2f;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float xPos = (float)x / (resolution - 1);
                float zPos = (float)z / (resolution - 1);
                float noise = Mathf.PerlinNoise(
                    xPos * noiseScale + randomX,
                    zPos * noiseScale + randomZ
                );
                float distance = Vector2.Distance(
                    new Vector2(xPos, zPos),
                    new Vector2(0.5f, 0.5f)
                );
                float islandShape = 1f - Mathf.Clamp01(distance * 1.6f);
                heights[z, x] = noise * islandHeight * islandShape;
            }
        }
        terrainData.SetHeights(0, 0, heights);
        PaintIsland(terrainData);
    }

    void PaintIsland(TerrainData terrainData)
    {
        if (groundTextures == null || groundTextures.Length != 3)
            return;

        terrainData.terrainLayers = groundTextures;

        int resolution = terrainData.alphamapResolution;
        float[,,] textures = new float[resolution, resolution, 3];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float xPos = (float)x / (resolution - 1);
                float zPos = (float)z / (resolution - 1);
                float groundHeight = terrainData.GetInterpolatedHeight(xPos, zPos);
                groundHeight /= terrainData.size.y;

                float sand = 1f - Mathf.InverseLerp(0.08f, 0.13f, groundHeight);
                float rock = Mathf.InverseLerp(0.23f, 0.25f, groundHeight);
                float grass = 1f - sand - rock;

                sand = Mathf.Clamp01(sand);
                grass = Mathf.Clamp01(grass);
                rock = Mathf.Clamp01(rock);

                float total = sand + grass + rock;

                textures[z, x, 0] = sand / total;
                textures[z, x, 1] = grass / total;
                textures[z, x, 2] = rock / total;
            }
        }
        terrainData.SetAlphamaps(0, 0, textures);
    }
}
