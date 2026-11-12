
using UnityEngine;

public class ForestBushSpawner : MonoBehaviour
{
    public Terrain forestTerrain;
    public GameObject[] bushPrefabs;
    public BoxCollider treeExclusionZone;
    public Transform vegetationParent;

    public int bushCount = 80;
    public int seed = 54321;
    public float minimumSpacing = 4f;

    void Start()
    {
        GenerateBushes();
    }

    void GenerateBushes()
    {
        if (forestTerrain == null || bushPrefabs == null ||
            bushPrefabs.Length == 0 || vegetationParent == null)
        {
            Debug.LogError("Assign all required references!");
            return;
        }

        System.Random random = new System.Random(seed);
        TerrainData data = forestTerrain.terrainData;

        int spawned = 0;
        int attempts = 0;

        while (spawned < bushCount && attempts < bushCount * 20)
        {
            attempts++;

            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float radius = Mathf.Sqrt((float)random.NextDouble()) * 0.38f;

            float x = (0.5f + Mathf.Cos(angle) * radius) * data.size.x;
            float z = (0.5f + Mathf.Sin(angle) * radius) * data.size.z;

            Vector3 position = forestTerrain.transform.position +
                new Vector3(x, 0f, z);

            position.y = forestTerrain.SampleHeight(position) +
                forestTerrain.transform.position.y;

            // Keep bushes away from the shed
            if (treeExclusionZone != null &&
                treeExclusionZone.bounds.Contains(position))
                continue;

            // Check distance from existing trees and bushes
            bool tooClose = false;

            for (int i = 0; i < vegetationParent.childCount; i++)
            {
                Vector3 otherPosition = vegetationParent.GetChild(i).position;

                Vector2 current = new Vector2(position.x, position.z);
                Vector2 other = new Vector2(otherPosition.x, otherPosition.z);

                if (Vector2.Distance(current, other) < minimumSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            GameObject prefab = bushPrefabs[random.Next(bushPrefabs.Length)];

            if (prefab == null)
                continue;

            GameObject bush = Instantiate(
                prefab,
                position,
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                vegetationParent
            );

            float scale = 0.8f + (float)random.NextDouble() * 0.4f;
            bush.transform.localScale *= scale;

            spawned++;
        }

        Debug.Log("Generated " + spawned + " bushes!");
    }
}
