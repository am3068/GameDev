using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[DefaultExecutionOrder(100)]
public class NaturalTerrainScatter : MonoBehaviour
{
    public enum Kind { Tree, Rock, Litter, Plant }

    [SerializeField] private GameObject[] trailLitterPrefabs;
    [SerializeField, Min(0)] private int trailLitterCount;
    [SerializeField, Range(.5f, 1f)] private float treeCanopySpacing = .8f;
    [SerializeField] private Shader foggedVegetation;

    private class Batch
    {
        public Kind kind;
        public GameObject[] prefabs;
        public BoxCollider[] exclusions;
        public Transform parent;
        public int count, seed;
        public float spacing, pathClearance;
    }

    private struct Shape
    {
        public GameObject prefab;
        public Vector3 baseOffset;
        public float radius, height;
    }

    private struct Placement
    {
        public Kind kind;
        public Vector2 point;
        public float radius;
    }

    private readonly List<Batch> batches = new List<Batch>();
    private readonly List<Placement> placements = new List<Placement>();
    private readonly Dictionary<Material, Material> vegetationMaterials = new Dictionary<Material, Material>();
    private CrystalShardSites shardSites;

    public void AddBatch(Kind kind, GameObject[] prefabs, BoxCollider[] exclusions, Transform parent,
        int count, int seed, float spacing, float pathClearance)
    {
        batches.Add(new Batch
        {
            kind = kind, prefabs = prefabs, exclusions = exclusions, parent = parent,
            count = count, seed = seed, spacing = spacing, pathClearance = pathClearance
        });
    }

    private void Start()
    {
        var terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;
        if (!data || data.terrainLayers.Length < 3)
            return;

        var paint = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        var trees = batches.Find(batch => batch.kind == Kind.Tree);
        if (trees != null && trailLitterCount > 0)
            AddBatch(Kind.Litter, trailLitterPrefabs, trees.exclusions, trees.parent, trailLitterCount, trees.seed + 41, .8f, .12f);

        var generator = GetComponentInParent<ProceduralForestGenerator>();
        shardSites = GetComponent<CrystalShardSites>();
        batches.Sort((left, right) => left.kind.CompareTo(right.kind));
        foreach (var batch in batches)
            Scatter(batch, terrain, paint, generator);
        batches.Clear();
    }

    private void Scatter(Batch batch, Terrain terrain, float[,,] paint, ProceduralForestGenerator generator)
    {
        if (!batch.parent || batch.prefabs == null)
            return;

        var shapes = new List<Shape>();
        foreach (var prefab in batch.prefabs)
            if (prefab && TryMeasure(prefab, out var shape))
                shapes.Add(shape);
        if (shapes.Count == 0)
            return;

        var data = terrain.terrainData;
        var origin = terrain.transform.position;
        var random = new System.Random(batch.seed);
        bool mountainRocks = batch.kind == Kind.Rock && generator && generator.useSingleMountain && generator.addMountains;
        int placed = 0;
        for (int attempt = 0; attempt < batch.count * 60 && placed < batch.count; attempt++)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();
            float x = u * data.size.x;
            float z = v * data.size.z;
            float slope = data.GetSteepness(u, v);
            float groundWeight = Layer(paint, data.size, x, z, 0);
            if (mountainRocks)
                groundWeight += Layer(paint, data.size, x, z, 2);
            if (slope > (batch.kind == Kind.Tree ? 24f : mountainRocks ? 40f : 33f) || groundWeight < .85f)
                continue;

            // Broad patches give the island open meadows between groups of trees and undergrowth.
            float patch = Mathf.PerlinNoise(x * .045f + 12.7f, z * .045f + 45.1f);
            float chance = batch.kind == Kind.Rock
                ? Mathf.Lerp(.35f, .85f, slope / 33f)
                : Mathf.Lerp(.12f, .95f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.28f, .68f, patch)));
            if (mountainRocks)
            {
                float distance = Vector2.Distance(new Vector2(u, v), generator.mountainCenter);
                float foothills = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(generator.mountainRadius * .65f,
                    generator.mountainRadius * 1.5f, distance));
                chance = Mathf.Lerp(.035f, .96f, foothills) * Mathf.Lerp(.52f, 1f, slope / 40f);
            }
            if (random.NextDouble() > chance)
                continue;

            var shape = shapes[random.Next(shapes.Count)];
            float variation = (float)random.NextDouble();
            float scale = batch.kind == Kind.Rock ? Mathf.Lerp(.38f, .97f, variation * variation) :
                batch.kind == Kind.Tree ? Mathf.Lerp(.76f, 1.10f, variation) : Mathf.Lerp(.75f, 1.26f, variation);
            if (batch.kind == Kind.Litter)
                scale = Mathf.Lerp(.46f, .87f, variation);
            float radius = shape.radius * scale;
            float edge = radius + .5f;
            if (x < edge || z < edge || x > data.size.x - edge || z > data.size.z - edge)
                continue;

            var ground = origin + new Vector3(x, data.GetInterpolatedHeight(u, v), z);
            var point = new Vector2(ground.x, ground.z);
            if (batch.kind == Kind.Litter && !OnTrail(paint, data.size, x, z, radius + 2.6f))
                continue;
            if (InsideExclusion(batch.exclusions, ground, radius) ||
                (shardSites && shardSites.IsInClearing(ground, radius)) ||
                OnTrail(paint, data.size, x, z, radius + batch.pathClearance) ||
                TooClose(batch, point, radius))
                continue;

            float lean = batch.kind == Kind.Rock ? .8f : batch.kind == Kind.Litter ? 1f : batch.kind == Kind.Plant ? .2f : 0f;
            var normal = Vector3.Slerp(Vector3.up, data.GetInterpolatedNormal(u, v), lean);
            var rotation = Quaternion.FromToRotation(Vector3.up, normal) *
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
            float embed = batch.kind == Kind.Rock ? shape.height * scale * .2f : .035f;
            var position = ground - rotation * (shape.baseOffset * scale) - Vector3.up * embed;
            var instance = Instantiate(shape.prefab, position, rotation * shape.prefab.transform.rotation, batch.parent);
            instance.transform.localScale *= scale;
            if (foggedVegetation && batch.kind != Kind.Rock)
                SetVegetationMaterials(instance);
            placements.Add(new Placement { kind = batch.kind, point = point, radius = radius });
            placed++;
        }
    }

    private void SetVegetationMaterials(GameObject instance)
    {
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var original = materials[i];
                if (!original)
                    continue;
                bool cutout = original.shader.name == "Shader Graphs/TerrainGrass";
                if (!cutout && original.shader.name != "Standard")
                    continue;
                if (!vegetationMaterials.TryGetValue(original, out var replacement))
                {
                    replacement = new Material(foggedVegetation) { name = original.name + " (Fogged)" };
                    if (cutout)
                    {
                        replacement.mainTexture = original.GetTexture("Texture2D_E1B0D043");
                        replacement.color = new Color(.80f, .81f, .76f);
                        replacement.SetFloat("_Cutoff", .38f);
                        replacement.SetFloat("_Cull", 0f);
                        replacement.SetFloat("_Wind", .06f);
                    }
                    else
                    {
                        replacement.mainTexture = original.mainTexture;
                        replacement.mainTextureScale = original.mainTextureScale;
                        replacement.mainTextureOffset = original.mainTextureOffset;
                        Color tint = original.color;
                        replacement.color = new Color(tint.r * .83f, tint.g * .83f, tint.b * .83f, tint.a);
                    }
                    vegetationMaterials.Add(original, replacement);
                }
                materials[i] = replacement;
            }
            renderer.sharedMaterials = materials;
        }
    }

    private void OnDestroy()
    {
        foreach (var material in vegetationMaterials.Values)
            Destroy(material);
    }

    private bool TooClose(Batch batch, Vector2 point, float radius)
    {
        foreach (var other in placements)
        {
            float spacing = batch.kind == Kind.Tree && other.kind == Kind.Tree ? treeCanopySpacing : .8f;
            float distance = (radius + other.radius) * spacing;
            if (batch.kind == other.kind)
                distance = Mathf.Max(distance, batch.spacing);
            else if ((batch.kind == Kind.Plant || batch.kind == Kind.Litter) && other.kind == Kind.Tree)
                distance = radius + .65f;
            if ((point - other.point).sqrMagnitude < distance * distance)
                return true;
        }
        return false;
    }

    private static bool OnTrail(float[,,] paint, Vector3 size, float x, float z, float clearance)
    {
        if (Layer(paint, size, x, z, 1) > .1f)
            return true;

        int rings = Mathf.Max(1, Mathf.CeilToInt(clearance / .55f));
        for (int ring = 1; ring <= rings; ring++)
        {
            float distance = clearance * ring / rings;
            for (int step = 0; step < 16; step++)
            {
                float angle = step * Mathf.PI / 8f;
                if (Layer(paint, size, x + Mathf.Cos(angle) * distance, z + Mathf.Sin(angle) * distance, 1) > .1f)
                    return true;
            }
        }
        return false;
    }

    private static float Layer(float[,,] paint, Vector3 size, float x, float z, int layer)
    {
        int px = Mathf.RoundToInt(Mathf.Clamp01(x / size.x) * (paint.GetLength(1) - 1));
        int pz = Mathf.RoundToInt(Mathf.Clamp01(z / size.z) * (paint.GetLength(0) - 1));
        return paint[pz, px, layer];
    }

    private static bool InsideExclusion(BoxCollider[] zones, Vector3 ground, float radius)
    {
        if (zones == null)
            return false;
        foreach (var zone in zones)
        {
            if (!zone)
                continue;
            var position = zone.transform.InverseTransformPoint(ground) - zone.center;
            var scale = zone.transform.lossyScale;
            if (Mathf.Abs(position.x) < zone.size.x * .5f + radius / Mathf.Max(.01f, Mathf.Abs(scale.x)) &&
                Mathf.Abs(position.z) < zone.size.z * .5f + radius / Mathf.Max(.01f, Mathf.Abs(scale.z)))
                return true;
        }
        return false;
    }

    private static bool TryMeasure(GameObject prefab, out Shape shape)
    {
        shape = default;
        var bounds = new Bounds();
        bool measured = false;
        foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>())
        {
            if (!filter.sharedMesh)
                continue;
            var mesh = filter.sharedMesh.bounds;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                var corner = mesh.center + Vector3.Scale(mesh.extents, new Vector3(x, y, z));
                var point = filter.transform.TransformPoint(corner) - prefab.transform.position;
                if (measured)
                    bounds.Encapsulate(point);
                else
                    bounds = new Bounds(point, Vector3.zero);
                measured = true;
            }
        }
        if (!measured)
            return false;

        shape = new Shape
        {
            prefab = prefab,
            baseOffset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
            radius = new Vector2(bounds.extents.x, bounds.extents.z).magnitude,
            height = bounds.size.y
        };
        return true;
    }
}
