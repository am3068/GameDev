using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Terrain))]
public class TerrainGrassFX : MonoBehaviour
{
    [SerializeField] private Material grassMaterial;
    [SerializeField] private BoxCollider[] noGrassZones;
    [SerializeField] private Color tipColour = new Color(.40f, .53f, .26f);
    [SerializeField, Range(1, 3)] private int growthLayers = 1;

    private const int CellsPerPatch = 8;
    private const float CellWidth = 1f;
    private static readonly Vector2[] LayerOffsets =
    {
        Vector2.zero, new Vector2(.36f, .30f), new Vector2(-.34f, .55f)
    };
    private readonly List<Mesh> meshes = new List<Mesh>();
    private Material instanceMaterial;
    private GameObject root;

    private void Start()
    {
        if (!grassMaterial || !grassMaterial.shader.isSupported)
            return;

        var terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;
        if (!data || data.alphamapLayers < 2)
            return;

        instanceMaterial = new Material(grassMaterial) { name = "Terrain grass (runtime)" };
        instanceMaterial.SetColor("_TopColor", tipColour);
        instanceMaterial.SetColor("_BottomColor", new Color(.12f, .19f, .075f));
        instanceMaterial.SetFloat("_BladeHeight", .27f);
        instanceMaterial.SetFloat("_BladeHeightRandom", .11f);
        instanceMaterial.SetFloat("_BladeWidth", .026f);
        instanceMaterial.SetFloat("_BladeWidthRandom", .011f);
        instanceMaterial.SetFloat("_WindStrength", .025f);
        instanceMaterial.SetFloat("_TranslucentGain", .25f);
        instanceMaterial.SetFloat("_ViewLOD", 32f);
        instanceMaterial.SetFloat("_MaxStages", 3f);
        instanceMaterial.SetFloat("_GrassDrawDistance", 62f);
        instanceMaterial.SetFloat("_StableBlades", 1f);

        root = new GameObject("Procedural grass surfaces");
        root.transform.SetParent(transform, false);
        var paint = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        int xCells = Mathf.CeilToInt(data.size.x / CellWidth);
        int zCells = Mathf.CeilToInt(data.size.z / CellWidth);
        for (int z = 0; z < zCells; z += CellsPerPatch)
        for (int x = 0; x < xCells; x += CellsPerPatch)
            CreatePatch(data, paint, x, z, xCells, zCells);
    }

    private void CreatePatch(TerrainData data, float[,,] paint, int startX, int startZ, int xCells, int zCells)
    {
        int width = Mathf.Min(CellsPerPatch, xCells - startX);
        int height = Mathf.Min(CellsPerPatch, zCells - startZ);
        int stride = width + 1;
        int layerLength = stride * (height + 1);
        int layerCount = Mathf.Clamp(growthLayers, 1, LayerOffsets.Length);
        int length = layerLength * layerCount;
        var vertices = new Vector3[length];
        var normals = new Vector3[length];
        var tangents = new Vector4[length];
        var density = new Color[length];
        var allowed = new bool[length];
        var indices = new List<int>(width * height * 6 * layerCount);
        for (int layer = 0; layer < layerCount; layer++)
        {
            int first = layer * layerLength;
            var offset = LayerOffsets[layer];
            for (int z = 0; z <= height; z++)
            for (int x = 0; x <= width; x++)
            {
                int index = first + z * stride + x;
                int gx = startX + x;
                int gz = startZ + z;
                float u = gx / (float)xCells;
                float v = gz / (float)zCells;
                if (gx > 0 && gx < xCells)
                    u += (offset.x + (Mathf.PerlinNoise(gx * .61f, gz * .73f) - .5f) * .57f) / xCells;
                if (gz > 0 && gz < zCells)
                    v += (offset.y + (Mathf.PerlinNoise(gz * .65f + 18f, gx * .79f) - .5f) * .57f) / zCells;

                float ground = data.GetInterpolatedHeight(u, v);
                vertices[index] = new Vector3(u * data.size.x, ground - .045f, v * data.size.z);
                var normal = data.GetInterpolatedNormal(u, v);
                normals[index] = normal;
                var tangent = Vector3.ProjectOnPlane(Vector3.right, normal).normalized;
                tangents[index] = new Vector4(tangent.x, tangent.y, tangent.z, 1f);
                allowed[index] = IsGrass(data, paint, u, v);
                float meadow = Mathf.PerlinNoise(vertices[index].x * .12f + 13f, vertices[index].z * .12f + 7f);
                float patchDensity = Mathf.Lerp(.66f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.22f, .76f, meadow)));
                density[index] = new Color(patchDensity, 0, 0, 1);
            }

            for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                int corner = first + z * stride + x;
                AddTriangle(corner, corner + stride, corner + 1);
                AddTriangle(corner + 1, corner + stride, corner + stride + 1);
            }
        }
        if (indices.Count == 0)
            return;

        var mesh = new Mesh { name = "Grass surface patch" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.colors = density;
        mesh.SetTriangles(indices, 0);
        var bounds = mesh.bounds;
        bounds.Expand(1.4f);
        mesh.bounds = bounds;
        meshes.Add(mesh);
        var patch = new GameObject("Grass patch");
        patch.transform.SetParent(root.transform, false);
        patch.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = patch.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = instanceMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        void AddTriangle(int a, int b, int c)
        {
            if (!allowed[a] || !allowed[b] || !allowed[c])
                return;
            Vector3 centre = (vertices[a] + vertices[b] + vertices[c]) / 3f;
            if (!IsGrass(data, paint, centre.x / data.size.x, centre.z / data.size.z))
                return;
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }
    }

    private bool IsGrass(TerrainData data, float[,,] paint, float u, float v)
    {
        int x = Mathf.RoundToInt(u * (paint.GetLength(1) - 1));
        int z = Mathf.RoundToInt(v * (paint.GetLength(0) - 1));
        if (paint[z, x, 0] < .76f || paint[z, x, 1] > .20f || data.GetSteepness(u, v) > 31f)
            return false;

        if (noGrassZones != null)
        {
            Vector3 point = transform.TransformPoint(new Vector3(u * data.size.x,
                data.GetInterpolatedHeight(u, v), v * data.size.z));
            foreach (var zone in noGrassZones)
            {
                if (!zone)
                    continue;
                Vector3 local = zone.transform.InverseTransformPoint(point) - zone.center;
                if (Mathf.Abs(local.x) <= zone.size.x * .5f && Mathf.Abs(local.z) <= zone.size.z * .5f)
                    return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        foreach (var mesh in meshes)
            Destroy(mesh);
        if (instanceMaterial)
            Destroy(instanceMaterial);
        if (root)
            Destroy(root);
    }
}
