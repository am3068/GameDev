using UnityEngine;

public class ForestBushSpawner : MonoBehaviour
{
    public Terrain forestTerrain;
    public GameObject[] bushPrefabs;
    public BoxCollider[] noSpawnZones;
    public Transform vegetationParent;
    public int bushCount = 80;
    public int seed = 54321;
    public float minimumSpacing = 2.5f;
    public float pathClearance = 3f;

    void Start()
    {
        var scatter = forestTerrain ? forestTerrain.GetComponent<NaturalTerrainScatter>() : null;
        if (scatter && scatter.isActiveAndEnabled)
            scatter.AddBatch(NaturalTerrainScatter.Kind.Plant, bushPrefabs, noSpawnZones, vegetationParent,
                bushCount, seed, minimumSpacing, pathClearance);
        else
            GenerateBushes();
    }

    void GenerateBushes()
    {
        if (!forestTerrain || bushPrefabs == null || bushPrefabs.Length == 0 || !vegetationParent) return;

        TerrainData terrain = forestTerrain.terrainData;
        if (terrain.terrainLayers.Length < 3) return;
        float[,,] paint = terrain.GetAlphamaps(0, 0, terrain.alphamapWidth, terrain.alphamapHeight);
        System.Random random = new System.Random(seed);
        int spawned = 0;

        for (int attempt = 0; attempt < bushCount * 30 && spawned < bushCount; attempt++)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();

            // bushes grow on grass, but not on paths or cliffs
            if (Layer(paint, u, v, 0) < 0.9f || terrain.GetSteepness(u, v) > 35f || NearPath(paint, terrain, u, v)) continue;

            Vector3 position = forestTerrain.transform.position + new Vector3(u * terrain.size.x, 0, v * terrain.size.z);
            position.y = forestTerrain.SampleHeight(position) + forestTerrain.transform.position.y;
            if (Blocked(position)) continue;

            GameObject prefab = bushPrefabs[random.Next(bushPrefabs.Length)];
            if (!prefab) continue;
            GameObject bush = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(360), 0), vegetationParent);
            bush.transform.localScale *= 0.8f + (float)random.NextDouble() * 0.4f;
            spawned++;
        }
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
                if (Mathf.Abs(point.x) < zone.size.x / 2f && Mathf.Abs(point.z) < zone.size.z / 2f) return true;
            }

        for (int i = 0; i < vegetationParent.childCount; i++)
        {
            Vector3 other = vegetationParent.GetChild(i).position;
            if (Vector2.Distance(new Vector2(position.x, position.z), new Vector2(other.x, other.z)) < minimumSpacing) return true;
        }
        return false;
    }
}
