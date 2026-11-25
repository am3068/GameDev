
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
    public bool useSingleMountain;
    public Vector2 mountainCenter = new Vector2(.52f, .66f);
    [Range(.05f, .5f)] public float mountainRadius = .24f;

    [Header("Paths")]
    [Min(1)] public float mountainPathClearance = 15f;
    [Range(0, 90)] public float rockSteepness = 45f;
    public bool smoothPathTransitions;
    [Min(1)] public float pathShoulderWidth = 5f;

    [Header("Objects")]
    public Transform[] objectParents;
    [Min(0)] public float objectGroundInset;

    [Header("Shoreline")]
    public Transform shorelineWater;
    public TerrainLayer shorelineSand;
    public Transform[] shorelineAccessPoints;

    [SerializeField, HideInInspector] byte[] savedPaths;
    [SerializeField, HideInInspector] int maskWidth, maskHeight;

    IEnumerator Start()
    {
        yield return null;
        SnapObjectsToTerrain();
    }

    [ContextMenu("Generate Forest")]
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

                // keeps the same hills but changes the noise with each seed
                float broadHills =
                    Noise(u, v, 1.8f, noiseOffset) * 0.60f +
                    Noise(u, v, 4.5f, noiseOffset + 50f) * 0.25f;
                float hills = broadHills +
                    Noise(u, v, 12f, noiseOffset + 100f) * 0.10f +
                    Noise(u, v, 25f, noiseOffset + 150f) * 0.05f;

                float height = Mathf.Clamp01(0.5f + hills * 1.6f) * hillHeight;

                // adds larger hills around the map
                float rollingHeight = Mathf.PerlinNoise(
                    u * 1.2f + noiseOffset + 200f,
                    v * 1.2f + noiseOffset + 200f) * 8f;
                height += rollingHeight;

                if (smoothPathTransitions)
                {
                    float trailHeight = Mathf.Clamp01(0.5f + broadHills) * hillHeight
                        + Mathf.Lerp(4f, rollingHeight, 0.65f);
                    float shoulder = Mathf.SmoothStep(0f, 1f, pathDistances[z, x] / pathShoulderWidth);
                    height = Mathf.Lerp(trailHeight, height, shoulder);
                }

                float mountains = MountainHeight(u, v, noiseOffset);

                // mountains fade out before reaching paths
                float clearance = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01(pathDistances[z, x] / mountainPathClearance));

                heights[z, x] = Mathf.Clamp01(
                    (height + mountains * clearance) / terrain.size.y);
            }
        }

        terrain.SetHeights(0, 0, heights);
        PaintTerrain(terrain);
        BlendShoreline();
        SnapObjectsToTerrain();
    }

    float MountainHeight(float u, float v, float noiseOffset)
    {
        if (useSingleMountain)
        {
            if (!addMountains)
                return 0f;

            var offset = (new Vector2(u, v) - mountainCenter) / mountainRadius;
            var ridge = new Vector2((.85f * offset.x + .52f * offset.y) * .82f,
                (-.52f * offset.x + .85f * offset.y) * 1.16f);
            float volume = 1f - Mathf.SmoothStep(0f, 1f, ridge.magnitude);
            float relief = Noise(u, v, 17f, noiseOffset + 710f) * .14f
                + Noise(u, v, 39f, noiseOffset + 770f) * .04f;
            return Mathf.Max(0f, volume + relief * 4f * volume * (1f - volume)) * mountainHeight;
        }

        float edgeDistance = Mathf.Min(u, 1f - u, v, 1f - v);
        float mountains = (1f - Mathf.SmoothStep(0.06f, 0.32f, edgeDistance)) * mountainHeight;
        mountains *= 0.5f + Mathf.PerlinNoise(u * 6f + noiseOffset + 300f, v * 6f + noiseOffset + 300f) * 0.5f;

        if (addMountains)
            mountains += Mountain(u, v, 0.48f, 0.63f, 0.18f) * 18f +
                Mountain(u, v, 0.43f, 0.29f, 0.12f) * 15f +
                Mountain(u, v, 0.63f, 0.24f, 0.10f) * 10f;

        return mountains;
    }

    public void BlendShoreline()
    {
        if (!forestTerrain || !shorelineWater || !shorelineSand)
            return;

        TerrainData terrain = forestTerrain.terrainData;
        Vector3 origin = forestTerrain.transform.position;
        Vector3 size = terrain.size;
        float sea = shorelineWater.position.y - origin.y;
        if (sea <= 0 || sea >= size.y)
            return;

        int resolution = terrain.heightmapResolution;
        float[,] heights = terrain.GetHeights(0, 0, resolution, resolution);
        const float cornerRadius = 10f;
        const float coastWidth = 12f;
        float submerged = Mathf.Max(.015f, sea - .65f);
        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            var point = new Vector2(x * size.x / (resolution - 1), z * size.z / (resolution - 1));
            float distance = CoastDistance(point, size, cornerRadius);
            if (distance >= coastWidth)
                continue;

            float accessDistance = float.MaxValue;
            if (shorelineAccessPoints != null)
                foreach (var access in shorelineAccessPoints)
                    if (access)
                    {
                        Vector3 local = access.position - origin;
                        accessDistance = Mathf.Min(accessDistance, Vector2.Distance(point, new Vector2(local.x, local.z)));
                    }

            float width = Mathf.Lerp(3.6f, coastWidth, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 23f, accessDistance)));
            if (distance >= width)
                continue;

            float blend = Mathf.SmoothStep(0f, 1f, distance / width);
            heights[z, x] = Mathf.Lerp(submerged / size.y, heights[z, x], blend);
        }
        terrain.SetHeights(0, 0, heights);
        PaintShoreline(terrain, sea);
    }

    static float CoastDistance(Vector2 point, Vector3 terrainSize, float radius)
    {
        Vector2 half = new Vector2(terrainSize.x, terrainSize.z) * .5f;
        var edge = new Vector2(Mathf.Abs(point.x - half.x), Mathf.Abs(point.y - half.y))
            - half + Vector2.one * radius;
        return radius - Vector2.Max(edge, Vector2.zero).magnitude - Mathf.Min(Mathf.Max(edge.x, edge.y), 0f);
    }

    public void RefreshShorelinePaint()
    {
        if (forestTerrain && shorelineWater && shorelineSand)
            PaintShoreline(forestTerrain.terrainData, shorelineWater.position.y - forestTerrain.transform.position.y);
    }

    void PaintShoreline(TerrainData terrain, float sea)
    {
        int width = terrain.alphamapWidth;
        int height = terrain.alphamapHeight;
        var layers = terrain.terrainLayers;
        int sand = System.Array.IndexOf(layers, shorelineSand);
        var previous = terrain.GetAlphamaps(0, 0, width, height);
        if (sand < 0)
        {
            sand = layers.Length;
            System.Array.Resize(ref layers, layers.Length + 1);
            layers[sand] = shorelineSand;
            terrain.terrainLayers = layers;
        }
        int path = System.Array.IndexOf(layers, pathTexture);
        var paint = new float[height, width, layers.Length];
        for (int z = 0; z < height; z++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / (width - 1);
            float v = (float)z / (height - 1);
            float distance = CoastDistance(new Vector2(u * terrain.size.x, v * terrain.size.z), terrain.size, 10f);
            float existingSand = sand < previous.GetLength(2) ? previous[z, x, sand] : 0f;
            float beach = 0f;
            if (distance < 14f)
            {
                float elevation = terrain.GetInterpolatedHeight(u, v);
                beach = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(sea + 1.0f, sea + 2.8f, elevation));
                beach *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(10f, 14f, distance));
                if (path >= 0)
                    beach *= 1f - previous[z, x, path];
            }
            float weight = Mathf.Max(existingSand, beach);
            float remaining = existingSand < 1f ? (1f - weight) / (1f - existingSand) : 0f;
            for (int layer = 0; layer < layers.Length; layer++)
                paint[z, x, layer] = layer == sand ? weight : previous[z, x, layer] * remaining;
        }
        terrain.SetAlphamaps(0, 0, paint);
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

        // saves the painted paths for later
        for (int z = 0; z < maskHeight; z++)
            for (int x = 0; x < maskWidth; x++)
                savedPaths[z * maskWidth + x] = (byte)Mathf.RoundToInt(paint[z, x, pathLayer] * 255f);

        pathMask = null;
    }

    [ContextMenu("Snap Objects To Terrain")]
    public void SnapObjectsToTerrain()
    {
        if (!forestTerrain || objectParents == null) return;

        foreach (Transform parent in objectParents)
        {
            if (!parent) continue;

            // Move the whole prefab, not its individual parts
            foreach (Transform obj in parent)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;

                float bottom = float.MaxValue;

                foreach (Renderer part in renderers)
                    bottom = Mathf.Min(bottom, part.bounds.min.y);

                Vector3 position = obj.position;
                float ground = forestTerrain.SampleHeight(position)
                            + forestTerrain.transform.position.y;

                position.y += ground - bottom - objectGroundInset;
                obj.position = position;
            }
        }
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
                float slope = terrain.GetSteepness(u, v);
                float rockWeight = smoothPathTransitions
                    ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(rockSteepness - 6f, rockSteepness + 6f, slope))
                    : (slope >= rockSteepness ? 1f : 0f);
                float rock = (1f - path) * rockWeight;

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

        // paths start at 0, everything else starts far away
        for (int z = 0; z < size; z++)
            for (int x = 0; x < size; x++)
                distances[z, x] = GetPath(
                    (float)x / (size - 1),
                    (float)z / (size - 1)) >= 0.5f ? 0f : 100000f;

        // finds the nearest path from both directions
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
        return Mathf.PerlinNoise(
            u * scale + offset,
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
            value = SampleSavedPath(u, v);

        return smoothPathTransitions ? value : (value >= 0.5f ? 1f : 0f);
    }

    float SampleSavedPath(float u, float v)
    {
        float x = u * (maskWidth - 1);
        float z = v * (maskHeight - 1);
        if (!smoothPathTransitions)
            return savedPaths[Mathf.RoundToInt(z) * maskWidth + Mathf.RoundToInt(x)] / 255f;

        int left = Mathf.FloorToInt(x);
        int right = Mathf.Min(left + 1, maskWidth - 1);
        int bottom = Mathf.FloorToInt(z);
        int top = Mathf.Min(bottom + 1, maskHeight - 1);
        float low = Mathf.Lerp(savedPaths[bottom * maskWidth + left], savedPaths[bottom * maskWidth + right], x - left);
        float high = Mathf.Lerp(savedPaths[top * maskWidth + left], savedPaths[top * maskWidth + right], x - left);
        return Mathf.Lerp(low, high, z - bottom) / 255f;
    }
}
