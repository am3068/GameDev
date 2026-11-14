
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

    void Start()
    {
        GenerateRocks();
    }

    void GenerateRocks()
    {
        if (islandTerrain == null || rockPrefabs == null ||
            rockPrefabs.Length == 0 || rockParent == null)
            return;

        System.Random random = new System.Random(seed);

        TerrainData data = islandTerrain.terrainData;
        int spawnedRocks = 0;
        int tries = 0;

        while (spawnedRocks < rockCount && tries < rockCount * 20)
        {
            tries++;

            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float radius = Mathf.Sqrt((float)random.NextDouble()) * 0.38f;

            float x = (0.5f + Mathf.Cos(angle) * radius) * data.size.x;
            float z = (0.5f + Mathf.Sin(angle) * radius) * data.size.z;

            Vector3 rockPosition = islandTerrain.transform.position +
                new Vector3(x, 0f, z);

            rockPosition.y = islandTerrain.SampleHeight(rockPosition) +
                islandTerrain.transform.position.y;

            if (IsInNoSpawnZone(rockPosition))
                continue;

            bool tooClose = false;

            for (int i = 0; i < rockParent.childCount; i++)
            {
                Vector3 otherRock = rockParent.GetChild(i).position;

                float distance = Vector2.Distance(
                    new Vector2(rockPosition.x, rockPosition.z),
                    new Vector2(otherRock.x, otherRock.z)
                );

                if (distance < minimumSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            GameObject chosenRock = rockPrefabs[random.Next(rockPrefabs.Length)];

            if (chosenRock == null)
                continue;

            GameObject newRock = Instantiate(
                chosenRock,
                rockPosition,
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                rockParent
            );

            float rockSize = 0.7f + (float)random.NextDouble() * 0.8f;
            newRock.transform.localScale *= rockSize;

            spawnedRocks++;
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
