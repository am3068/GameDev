using UnityEngine;
using System.Collections;

public class ProceduralForestGenerator : MonoBehaviour
{
    [Header("Textures")]
    public Terrain forestTerrain;
    public Texture2D pathMask;
    public TerrainLayer grassTexture, pathTexture, rockTexture;

    [Header("Terrain Settings")]
    public int seed = 12345;
    public float hillHeight = 10f;
    public float mountainHeight = 15f;
    public bool addMountains = true;

    [Header("Paths")]
    [Range(1, 45)] public float maxPathAngle = 21f;
    [Min(1)] public float mountainPathClearance = 15f;
    [Range(0, 90)] public float rockSteepness = 45f;

    [SerializeField, HideInInspector] byte[] savedPaths;
    [SerializeField, HideInInspector] int maskWidth, maskHeight;

    [ContextMenu("Generate Forest")]

    IEnumerator Start()
    {
        yield return null;
        SnapObjectsToTerrain();
    }  
    public void GenerateForest()
    {
        if (!forestTerrain || !grassTexture || !pathTexture ||
            !rockTexture || !HasMask()) return;

        TerrainData terrain = forestTerrain.terrainData;
        terrain.terrainLayers = new[] { grassTexture, pathTexture, rockTexture };

        terrain.size = new Vector3(terrain.size.x,
            Mathf.Max(terrain.size.y, hillHeight + mountainHeight + 40f),
            terrain.size.z);

        int resolution = terrain.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        float[,] pathDistances = FindPathDistances(terrain);
        float noiseOffset = seed * 0.013f;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / (resolution - 1);
                float v = (float)z / (resolution - 1);

                // keeps the same hill style but changes the noise with each seed
                float hills =
                    Noise(u, v, 1.8f, noiseOffset) * 0.60f +
                    Noise(u, v, 4.5f, noiseOffset + 50f) * 0.25f +
                    Noise(u, v, 12f, noiseOffset + 100f) * 0.10f +
                    Noise(u, v, 25f, noiseOffset + 150f) * 0.05f;

                float height = Mathf.Clamp01(0.5f + hills * 1.6f) * hillHeight;

                // adds some larger hills across the terrain
                height += Mathf.PerlinNoise(
                    u * 1.2f + noiseOffset + 200f,
                    v * 1.2f + noiseOffset + 200f) * 8f;

                // mountains get taller towards the edges
                float edgeDistance = Mathf.Min(u, 1f - u, v, 1f - v);
                float mountains = (1f - Mathf.SmoothStep(
                    0.06f, 0.32f, edgeDistance)) * mountainHeight;

                mountains *= 0.5f + Mathf.PerlinNoise(
                    u * 6f + noiseOffset + 300f,
                    v * 6f + noiseOffset + 300f) * 0.5f;

                // extra mountains in fixed spots between paths
                if (addMountains)
                    mountains +=
                        Mountain(u, v, 0.48f, 0.63f, 0.18f) * 18f +
                        Mountain(u, v, 0.43f, 0.29f, 0.12f) * 15f +
                        Mountain(u, v, 0.63f, 0.24f, 0.10f) * 10f;

                // stops mountains from growing directly onto paths
                float pathClearance = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01(pathDistances[z, x] / mountainPathClearance));

                heights[z, x] = Mathf.Clamp01(
                    (height + mountains * pathClearance) / terrain.size.y);
            }
        }

    LimitPathSlopes(terrain, heights, pathDistances);
    terrain.SetHeights(0, 0, heights);
    PaintTerrain(terrain);
    SnapObjectsToTerrain();
    }
public Transform[] objectParents;

    [ContextMenu("Snap Objects To Terrain")]
    public void SnapObjectsToTerrain()
    {
        foreach (Transform parent in objectParents)
        {
            if (!parent) continue;

            foreach (Transform obj in parent)
            {
                Vector3 position = obj.position;
                position.y = forestTerrain.SampleHeight(position)
                        + forestTerrain.transform.position.y;

                obj.position = position;
            }
        }
    }

    [ContextMenu("Save Path Mask")]
    public void SavePathMask()
    {
        if (!forestTerrain || !pathTexture) return;

        TerrainData terrain = forestTerrain.terrainData;
        int pathLayer = System.Array.IndexOf(terrain.terrainLayers, pathTexture);
        if (pathLayer < 0) return;

        maskWidth = terrain.alphamapWidth;
        maskHeight = terrain.alphamapHeight;
        savedPaths = new byte[maskWidth * maskHeight];

        float[,,] paint = terrain.GetAlphamaps(0, 0, maskWidth, maskHeight);

        // saves the painted paths so they can be reused
        for (int z = 0; z < maskHeight; z++)
            for (int x = 0; x < maskWidth; x++)
                savedPaths[z * maskWidth + x] =
                    (byte)(paint[z, x, pathLayer] >= 0.5f ? 255 : 0);

        pathMask = null;
    }

    void PaintTerrain(TerrainData terrain)
    {
        int width = terrain.alphamapWidth;
        int height = terrain.alphamapHeight;
        float[,,] paint = new float[height, width, 3];

        // grass on normal ground, rock on steep slopes, dirt on paths
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                float v = (float)z / (height - 1);

                float path = GetPath(u, v);
                float rock = terrain.GetSteepness(u, v) >= rockSteepness
                    ? 1f - path : 0f;

                paint[z, x, 0] = 1f - path - rock;
                paint[z, x, 1] = path;
                paint[z, x, 2] = rock;
            }
        }

        terrain.SetAlphamaps(0, 0, paint);
    }

    float[,] FindPathDistances(TerrainData terrain)
    {
        int size = terrain.heightmapResolution;
        float[,] distances = new float[size, size];
        float stepX = terrain.size.x / (size - 1);
        float stepZ = terrain.size.z / (size - 1);
        float diagonal = Mathf.Sqrt(stepX * stepX + stepZ * stepZ);

        // path pixels start at 0, everything else starts far away
        for (int z = 0; z < size; z++)
            for (int x = 0; x < size; x++)
                distances[z, x] = GetPath(
                    (float)x / (size - 1),
                    (float)z / (size - 1)) > 0f ? 0f : 100000f;

        // checks both directions to find the closest path
        for (int pass = 0; pass < 2; pass++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int px = pass == 0 ? x : size - 1 - x;
                    int pz = pass == 0 ? z : size - 1 - z;
                    int direction = pass == 0 ? -1 : 1;

                    distances[pz, px] = Mathf.Min(distances[pz, px],
                        Mathf.Min(
                            CheckNearby(distances, px + direction, pz, stepX),
                            CheckNearby(distances, px, pz + direction, stepZ)));

                    distances[pz, px] = Mathf.Min(distances[pz, px],
                        Mathf.Min(
                            CheckNearby(distances, px + direction, pz + direction, diagonal),
                            CheckNearby(distances, px - direction, pz + direction, diagonal)));
                }
            }
        }

        return distances;
    }

    void LimitPathSlopes(TerrainData terrain, float[,] heights, float[,] distances)
    {
        int size = heights.GetLength(0);
        float stepX = terrain.size.x / (size - 1);
        float stepZ = terrain.size.z / (size - 1);
        float diagonal = Mathf.Sqrt(stepX * stepX + stepZ * stepZ);

        // lowers steep parts of paths and blends into nearby ground
        for (int pass = 0; pass < 2; pass++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int px = pass == 0 ? x : size - 1 - x;
                    int pz = pass == 0 ? z : size - 1 - z;

                    if (distances[pz, px] > mountainPathClearance) continue;

                    float blend = Mathf.SmoothStep(0f, 1f,
                        distances[pz, px] / mountainPathClearance);

                    float angle = Mathf.Lerp(maxPathAngle, 75f, blend);
                    float slope = Mathf.Tan(angle * Mathf.Deg2Rad) /
                                  terrain.size.y;

                    int direction = pass == 0 ? -1 : 1;

                    heights[pz, px] = Mathf.Min(heights[pz, px],
                        CheckNearby(heights, px + direction, pz, stepX * slope),
                        CheckNearby(heights, px, pz + direction, stepZ * slope),
                        CheckNearby(heights, px + direction, pz + direction, diagonal * slope),
                        CheckNearby(heights, px - direction, pz + direction, diagonal * slope));
                }
            }
        }
    }

    float CheckNearby(float[,] values, int x, int z, float amount)
    {
        int size = values.GetLength(0);
        return x >= 0 && z >= 0 && x < size && z < size
            ? values[z, x] + amount : 100000f;
    }

    float Mountain(float u, float v, float x, float z, float radius)
    {
        float distance = Vector2.Distance(new Vector2(u, v), new Vector2(x, z));
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(distance / radius));
    }

    float Noise(float u, float v, float scale, float offset)
    {
        return Mathf.PerlinNoise(u * scale + offset,
            v * scale + offset) * 2f - 1f;
    }

    bool HasMask()
    {
        return pathMask || (savedPaths != null && maskWidth > 0 &&
            maskHeight > 0 && savedPaths.Length == maskWidth * maskHeight);
    }

    float GetPath(float u, float v)
    {
        float value;

        if (pathMask)
            value = pathMask.GetPixelBilinear(u, v).r;
        else
        {
            int x = Mathf.RoundToInt(u * (maskWidth - 1));
            int z = Mathf.RoundToInt(v * (maskHeight - 1));
            value = savedPaths[z * maskWidth + x] / 255f;
        }

        return value >= 0.5f ? 1f : 0f;
    }
}
