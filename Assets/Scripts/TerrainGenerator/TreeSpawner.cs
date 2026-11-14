
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

    void Start()
    {
        GenerateTrees();
    }
    void GenerateTrees()
    {
        if (islandTerrain == null || treePrefabs == null ||
            treePrefabs.Length == 0 || vegetationParent == null)
            return;

        System.Random random = new System.Random(seed);

        TerrainData data = islandTerrain.terrainData;
        int spawnedTrees = 0;
        int tries = 0;

        while (spawnedTrees < treeCount && tries < treeCount * 20)
        {
            tries++;

            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float radius = Mathf.Sqrt((float)random.NextDouble()) * 0.38f;

            float x = (0.5f + Mathf.Cos(angle) * radius) * data.size.x;
            float z = (0.5f + Mathf.Sin(angle) * radius) * data.size.z;

            Vector3 treePosition = islandTerrain.transform.position +
                new Vector3(x, 0f, z);

            treePosition.y = islandTerrain.SampleHeight(treePosition) +
                islandTerrain.transform.position.y;

            if (IsInNoSpawnZone(treePosition))
                continue;
            bool tooClose = false;
            for (int i = 0; i < vegetationParent.childCount; i++)
            {
                Vector3 otherTree = vegetationParent.GetChild(i).position;

                float distance = Vector2.Distance(
                    new Vector2(treePosition.x, treePosition.z),
                    new Vector2(otherTree.x, otherTree.z)
                );

                if (distance < minimumSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            GameObject chosenTree = treePrefabs[random.Next(treePrefabs.Length)];
            if (chosenTree == null)
                continue;

            GameObject newTree = Instantiate(
                chosenTree,
                treePosition,
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                vegetationParent
            );

            float treeSize = 0.9f + (float)random.NextDouble() * 0.3f;
            newTree.transform.localScale *= treeSize;

            spawnedTrees++;
        }
    }
    bool IsInNoSpawnZone(Vector3 position)
    {
        if (noSpawnZones == null)
            return false;

        foreach (BoxCollider zone in noSpawnZones)
        {
            if (zone == null)
                continue;
            Vector3 localPosition = zone.transform.InverseTransformPoint(position);
            Vector3 zoneSize = zone.size / 2f;
            if (Mathf.Abs(localPosition.x - zone.center.x) < zoneSize.x &&
                Mathf.Abs(localPosition.z - zone.center.z) < zoneSize.z)
                return true;
        }

        return false;
    }
}
