using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(50)]
public class CrystalShardSites : MonoBehaviour
{
    [SerializeField] private CrystalShard.Kind kind;
    [SerializeField] private GameObject crystalPrefab;
    [SerializeField] private Vector2[] sites;

    private const float ClearingRadius = 4f;
    private readonly List<Vector2> clearings = new List<Vector2>();
    private readonly Collider[] nearby = new Collider[24];

    private void Start()
    {
        var ground = GetComponent<Collider>();
        if (!ground || !crystalPrefab || sites == null)
            return;

        var terrain = GetComponent<Terrain>();
        var generator = GetComponentInParent<ProceduralForestGenerator>();
        var pathMask = generator ? generator.pathMask : null;
        Bounds bounds = ground.bounds;
        if (terrain)
            bounds = new Bounds(terrain.transform.position + terrain.terrainData.size / 2f, terrain.terrainData.size);

        var group = new GameObject(kind + " crystal deposits").transform;
        group.SetParent(transform, true);
        var random = new System.Random(System.Guid.NewGuid().GetHashCode());
        Physics.SyncTransforms();
        for (int i = 0; i < sites.Length; i++)
        {
            if (TryFindSite(ground, pathMask, bounds, sites[i], random, out Vector3 position))
            {
                clearings.Add(new Vector2(position.x, position.z));
                SpawnCrystal(group, ground, position, i, random.Next(360));
            }
            else
                Debug.LogWarning($"No clear spot for {kind} crystal deposit {i + 1}.", this);
        }
    }

    public bool IsInClearing(Vector3 point, float padding = 0f)
    {
        float radius = ClearingRadius + padding;
        var horizontal = new Vector2(point.x, point.z);
        foreach (var center in clearings)
            if ((horizontal - center).sqrMagnitude < radius * radius)
                return true;
        return false;
    }

    private bool TryFindSite(Collider ground, Texture2D pathMask, Bounds bounds, Vector2 site,
        System.Random random, out Vector3 position)
    {
        Vector3 center = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, site.x),
            bounds.max.y + 3f, Mathf.Lerp(bounds.min.z, bounds.max.z, site.y));
        var candidates = new List<Vector3>();
        for (int radius = 0; radius <= 22; radius += 2)
        {
            int samples = Mathf.Max(1, radius * 2);
            for (int sample = 0; sample < samples; sample++)
            {
                float angle = sample / (float)samples * Mathf.PI * 2f;
                var origin = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                float u = (origin.x - bounds.min.x) / bounds.size.x;
                float v = (origin.z - bounds.min.z) / bounds.size.z;
                if (u < .08f || u > .92f || v < .08f || v > .92f ||
                    IsInClearing(origin, ClearingRadius + 2f) ||
                    (pathMask && !HasTrailAccess(pathMask, bounds, u, v)))
                    continue;

                if (!ground.Raycast(new Ray(origin, Vector3.down), out RaycastHit hit, bounds.size.y + 5f) ||
                    hit.normal.y < .94f || !HasClearSpace(hit.point, ground))
                    continue;

                candidates.Add(hit.point);
            }
        }

        if (candidates.Count > 0)
        {
            position = candidates[random.Next(candidates.Count)];
            return true;
        }
        position = default;
        return false;
    }

    private static bool HasTrailAccess(Texture2D mask, Bounds bounds, float u, float v)
    {
        if (mask.GetPixelBilinear(u, v).r > .16f)
            return false;

        bool nearTrail = false;
        for (int direction = 0; direction < 12; direction++)
        {
            float angle = direction * Mathf.PI / 6f;
            float x = Mathf.Cos(angle) / bounds.size.x;
            float z = Mathf.Sin(angle) / bounds.size.z;
            // Keep the crystal itself clear of the trail while leaving a short route from the trail into the clearing.
            if (mask.GetPixelBilinear(u + x * 2f, v + z * 2f).r > .4f)
                return false;
            for (int distance = 5; distance <= 15 && !nearTrail; distance += 2)
                nearTrail = mask.GetPixelBilinear(u + x * distance, v + z * distance).r > .55f;
        }
        return nearTrail;
    }

    private bool HasClearSpace(Vector3 position, Collider ground)
    {
        int count = Physics.OverlapSphereNonAlloc(position + Vector3.up, ClearingRadius - .6f, nearby,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (count == nearby.Length)
            return false;

        for (int i = 0; i < count; i++)
            if (nearby[i] != ground)
                return false;
        return true;
    }

    private void SpawnCrystal(Transform group, Collider ground, Vector3 position, int index, float yaw)
    {
        var crystal = new GameObject($"{kind} crystal deposit {index + 1}");
        crystal.transform.SetParent(group, false);
        crystal.transform.position = position;
        crystal.AddComponent<CrystalShard>().SetKind(kind);

        Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
        var bounds = AddCrystal(crystal.transform, ground, Vector3.zero, 2.65f, yaw, 0f);
        bounds.Encapsulate(AddCrystal(crystal.transform, ground, turn * new Vector3(.64f, 0f, .46f), 1.75f, yaw + 121f, -14f));
        bounds.Encapsulate(AddCrystal(crystal.transform, ground, turn * new Vector3(-.69f, 0f, .2f), 1.35f, yaw + 19f, 21f));
        bounds.Encapsulate(AddCrystal(crystal.transform, ground, turn * new Vector3(.08f, 0f, -.71f), 1.15f, yaw + 248f, 18f));

        var pickup = crystal.AddComponent<BoxCollider>();
        pickup.center = crystal.transform.InverseTransformPoint(bounds.center);
        pickup.size = bounds.size + Vector3.one * .12f;
        pickup.isTrigger = true;
        crystal.AddComponent<CrystalPickupMarker>();
    }

    private Bounds AddCrystal(Transform cluster, Collider ground, Vector3 offset, float height, float yaw, float tilt)
    {
        var visual = Instantiate(crystalPrefab, cluster);
        visual.transform.localPosition = offset;
        visual.transform.rotation = Quaternion.Euler(0f, yaw, tilt);
        foreach (var collider in visual.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        var renderer = visual.GetComponentInChildren<Renderer>();
        visual.transform.localScale *= height / Mathf.Max(.01f, renderer.bounds.size.y);
        float groundHeight = cluster.position.y;
        if (ground.Raycast(new Ray(visual.transform.position + Vector3.up * 2f, Vector3.down), out var hit, 4f))
            groundHeight = hit.point.y;
        visual.transform.position += Vector3.up * (groundHeight - renderer.bounds.min.y - .1f);
        return renderer.bounds;
    }
}
