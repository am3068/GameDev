using UnityEngine;

public class ForestTreeSpawner : MonoBehaviour
{
    public Terrain islandTerrain;
    public GameObject[] treePrefabs;
    public BoxCollider[] noSpawnZones;
    public Transform vegetationParent;
    public int treeCount = 100;
    public int seed = 12345;
    public float minimumSpacing = 2.5f;
    public float pathClearance = 4f;

    void Start() => GenerateTrees();

    void GenerateTrees()
    {
        if (!islandTerrain || treePrefabs == null || treePrefabs.Length == 0 || !vegetationParent) return;

        TerrainData terrain = islandTerrain.terrainData;
        if (terrain.terrainLayers.Length < 3) return;
        float[,,] paint = terrain.GetAlphamaps(0, 0, terrain.alphamapWidth, terrain.alphamapHeight);
        System.Random random = new System.Random(seed);
        int spawned = 0;

        for (int attempt = 0; attempt < treeCount * 30 && spawned < treeCount; attempt++)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();

            // trees only grow on gentle grass, away from paths
            if (Layer(paint, u, v, 0) < 0.9f || terrain.GetSteepness(u, v) > 30f || NearPath(paint, terrain, u, v)) continue;

            Vector3 position = islandTerrain.transform.position + new Vector3(u * terrain.size.x, 0, v * terrain.size.z);
            position.y = islandTerrain.SampleHeight(position) + islandTerrain.transform.position.y;
            if (Blocked(position)) continue;

            GameObject prefab = treePrefabs[random.Next(treePrefabs.Length)];
            if (!prefab) continue;
            GameObject tree = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(360), 0), vegetationParent);
            tree.transform.localScale *= 0.9f + (float)random.NextDouble() * 0.3f;
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
