
using UnityEngine;

public class ForestRockSpawner : MonoBehaviour
{
    public Terrain islandTerrain;
    public GameObject[] rockPrefabs;
    public BoxCollider[] noSpawnZones;
    public Transform rockParent;

    public int rockCount = 50;
    public int seed = 67890;
    public float minimumSpacing = 4f;
    public float pathClearance = 3f;

    void Start()
    {
        var scatter = islandTerrain ? islandTerrain.GetComponent<NaturalTerrainScatter>() : null;
        if (scatter && scatter.isActiveAndEnabled)
            scatter.AddBatch(NaturalTerrainScatter.Kind.Rock, rockPrefabs, noSpawnZones, rockParent,
                rockCount, seed, minimumSpacing, pathClearance);
        else
            GenerateRocks();
    }

    void GenerateRocks()
    {
        if (!islandTerrain || !rockParent || rockPrefabs == null || rockPrefabs.Length == 0) return;

        TerrainData terrain = islandTerrain.terrainData;
        if (terrain.terrainLayers.Length < 3) return;

        float[,,] paint = terrain.GetAlphamaps(0, 0, terrain.alphamapWidth, terrain.alphamapHeight);
        System.Random random = new System.Random(seed);
        int spawned = 0;

        for (int attempt = 0; attempt < rockCount * 50 && spawned < rockCount; attempt++)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();

            // only spawn on grass, away from paths
            if (Layer(paint, u, v, 0) < 0.9f ||
                terrain.GetSteepness(u, v) > 30f ||
                NearPath(paint, terrain, u, v)) continue;

            Vector3 position = islandTerrain.transform.position +
                new Vector3(u * terrain.size.x, 0, v * terrain.size.z);

            position.y = islandTerrain.SampleHeight(position) + islandTerrain.transform.position.y;
            if (Blocked(position)) continue;

            // big rocks near steep mountain edges, small rocks anywhere on grass
            bool bigRock = random.NextDouble() < 0.35 && NearMountain(terrain, u, v);

            GameObject prefab = rockPrefabs[random.Next(rockPrefabs.Length)];
            if (!prefab) continue;

            GameObject rock = Instantiate(prefab, position,
                Quaternion.Euler(0, random.Next(360), 0), rockParent);

            float scale = bigRock
                ? 0.6f + (float)random.NextDouble() * 0.4f
                : 0.15f + (float)random.NextDouble() * 0.2f;

            rock.transform.localScale *= scale;
            spawned++;
        }
    }

    bool NearMountain(TerrainData terrain, float u, float v)
    {
        // look around for steep ground near the base of a mountain
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4f;

            for (float distance = 5f; distance <= 10f; distance += 5f)
            {
                float x = u + Mathf.Cos(angle) * distance / terrain.size.x;
                float z = v + Mathf.Sin(angle) * distance / terrain.size.z;

                if (x < 0 || x > 1 || z < 0 || z > 1) continue;

                float steepness = terrain.GetSteepness(x, z);
                if (steepness >= 42f && steepness <= 45f) return true;
            }
        }

        return false;
    }

    float Layer(float[,,] paint, float u, float v, int layer)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(u * (paint.GetLength(1) - 1)), 0, paint.GetLength(1) - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(v * (paint.GetLength(0) - 1)), 0, paint.GetLength(0) - 1);
        return paint[z, x, layer];
    }

    bool NearPath(float[,,] paint, TerrainData terrain, float u, float v)
    {
        if (Layer(paint, u, v, 1) > 0.1f) return true;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4f;
            float x = Mathf.Clamp01(u + Mathf.Cos(angle) * pathClearance / terrain.size.x);
            float z = Mathf.Clamp01(v + Mathf.Sin(angle) * pathClearance / terrain.size.z);

            if (Layer(paint, x, z, 1) > 0.1f) return true;
        }

        return false;
    }

    bool Blocked(Vector3 position)
    {
        if (noSpawnZones != null)
            foreach (BoxCollider zone in noSpawnZones)
            {
                if (!zone) continue;

                Vector3 point = zone.transform.InverseTransformPoint(position) - zone.center;
                if (Mathf.Abs(point.x) < zone.size.x / 2f &&
                    Mathf.Abs(point.z) < zone.size.z / 2f) return true;
            }

        for (int i = 0; i < rockParent.childCount; i++)
        {
            Vector3 other = rockParent.GetChild(i).position;
            if (Vector2.Distance(new Vector2(position.x, position.z),
                new Vector2(other.x, other.z)) < minimumSpacing) return true;
        }

        return false;
    }
}
