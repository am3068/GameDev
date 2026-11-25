using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(175)]
public class BiomeEnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyPrefab;
    [SerializeField, Min(0)] private int roamingEnemies;
    [SerializeField] private GameObject forestGemPrefab;
    [SerializeField] private Transform guardianPortal;

    private readonly List<Vector2> spawns = new List<Vector2>();
    private readonly Collider[] overlaps = new Collider[16];
    private Collider ground;
    private Bounds bounds;
    private System.Random random;
    private Transform group;

    private void Start()
    {
        ground = GetComponent<Collider>();
        if (!ground || !enemyPrefab)
            return;

        var terrain = GetComponent<Terrain>();
        bounds = terrain ? new Bounds(terrain.transform.position + terrain.terrainData.size / 2f, terrain.terrainData.size) : ground.bounds;
        random = new System.Random(System.Guid.NewGuid().GetHashCode());
        group = new GameObject("Test Enemies").transform;
        group.SetParent(transform, true);
        Physics.SyncTransforms();

        if (forestGemPrefab)
        {
            if (!guardianPortal || !TrySpawnPortalGuardian())
                Debug.LogWarning("Could not place the Test Guardian beside the Forest Portal.", this);
            return;
        }

        foreach (var shard in GetComponentsInChildren<CrystalShard>())
        {
            if (!TrySpawnNear(shard.transform.position, 3f, 8f, "Test Guardian", 125f))
                Debug.LogWarning("Could not place a guardian for " + shard.name, this);
        }

        int placed = 0;
        for (int attempt = 0; attempt < roamingEnemies * 60 && placed < roamingEnemies; attempt++)
        {
            var point = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, .11f + .78f * (float)random.NextDouble()),
                bounds.max.y, Mathf.Lerp(bounds.min.z, bounds.max.z, .11f + .78f * (float)random.NextDouble()));
            if (!TrySpawn(point, 13f, 17f, "Test Enemy", 0f))
                continue;
            placed++;
        }
    }

    private bool TrySpawnPortalGuardian()
    {
        Vector3 approach = guardianPortal.position - guardianPortal.forward * 4f;
        Vector3 side = guardianPortal.right * 3.5f;
        return TrySpawnNear(approach + side, 0f, 2f, "Test Guardian", 65f) ||
            TrySpawnNear(approach - side, 0f, 2f, "Test Guardian", 65f);
    }

    private bool TrySpawnNear(Vector3 center, float minimum, float maximum, string enemyName, float health)
    {
        float heading = (float)random.NextDouble() * Mathf.PI * 2f;
        for (float radius = minimum; radius <= maximum; radius += 1f)
        for (int sample = 0; sample < (radius == 0f ? 1 : 12); sample++)
        {
            float angle = heading + sample * Mathf.PI / 6f;
            Vector3 point = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            if (TrySpawn(point, 1.6f, 3f, enemyName, health))
                return true;
        }
        return false;
    }

    private bool TrySpawn(Vector3 point, float patrolRadius, float separation, string enemyName, float health)
    {
        var horizontal = new Vector2(point.x, point.z);
        foreach (var spawn in spawns)
            if ((horizontal - spawn).sqrMagnitude < separation * separation)
                return false;

        point.y = bounds.max.y + 3f;
        if (!ground.Raycast(new Ray(point, Vector3.down), out var hit, bounds.size.y + 5f) || hit.normal.y < .87f)
            return false;

        int count = Physics.OverlapSphereNonAlloc(hit.point + Vector3.up * .8f, 1.15f, overlaps,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (count == overlaps.Length)
            return false;
        for (int i = 0; i < count; i++)
            if (overlaps[i] != ground)
                return false;

        var enemy = Instantiate(enemyPrefab, hit.point + Vector3.up * .5f,
            Quaternion.Euler(0f, random.Next(360), 0f), group);
        enemy.name = enemyName;
        enemy.gameObject.layer = LayerMask.NameToLayer("Enemy");
        var ai = enemy.GetComponent<EnemyBehaviourAI>();
        ai.SetPatrolArea(ground, hit.point, patrolRadius);
        ai.obstacleLayerMask |= LayerMask.GetMask("Default");
        if (health > 0f)
        {
            enemy.maxHealth = health;
            enemy.CanRespawn = false;
            ai.patrolSpeed = .2f;
            ai.moveSpeed = .45f;
        }
        if (forestGemPrefab)
        {
            enemy.ForestGemPrefab = forestGemPrefab;
            ai.fireRate *= 1.65f;
        }
        spawns.Add(horizontal);
        return true;
    }
}
