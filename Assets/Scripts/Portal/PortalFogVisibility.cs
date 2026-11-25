using UnityEngine;

public class PortalFogVisibility : MonoBehaviour
{
    [SerializeField] private Transform viewer;
    [SerializeField] private Renderer[] geometry;
    [SerializeField] private Terrain island;
    [SerializeField] private Renderer islandBeacon;
    private Transform glow;
    private PortalTeleport teleporter;
    private Collider teleportTrigger;

    public bool IsActivated { get; private set; }
    public Vector3 InteractionMarkerPosition => transform.TransformPoint(new Vector3(0f, 1.15f, -.4f));

    private void Awake()
    {
        glow = transform.Find("Portal Glow");
        teleporter = GetComponentInChildren<PortalTeleport>(true);
        teleportTrigger = teleporter ? teleporter.GetComponent<Collider>() : null;
        SetActivation(false);
    }

    private void Start()
    {
        if (!islandBeacon)
            return;

        var appearance = new MaterialPropertyBlock();
        islandBeacon.GetPropertyBlock(appearance);
        appearance.SetFloat("_Vertical", 1f);
        appearance.SetFloat("_Range", 245f);
        appearance.SetFloat("_Intensity", 1.4f);
        islandBeacon.SetPropertyBlock(appearance);
        var frame = GetComponent<MeshFilter>();
        if (frame && frame.sharedMesh)
        {
            var position = islandBeacon.transform.localPosition;
            position.y = frame.sharedMesh.bounds.max.y + .38f + islandBeacon.transform.localScale.y * .5f;
            islandBeacon.transform.localPosition = position;
        }
        islandBeacon.enabled = true;
    }

    public float GetApproachDistanceSquared(Vector3 playerPosition)
    {
        if (!isActiveAndEnabled || transform.InverseTransformPoint(playerPosition).z > .35f)
            return float.PositiveInfinity;

        Vector3 approach = transform.TransformPoint(new Vector3(0f, .75f, -.35f));
        return (playerPosition - approach).sqrMagnitude;
    }

    public void Activate()
    {
        if (IsActivated)
            return;

        SetActivation(true);
        if (teleporter && teleporter.destination)
        {
            var returnPortal = teleporter.destination.GetComponentInParent<PortalFogVisibility>();
            if (returnPortal && !returnPortal.IsActivated)
                returnPortal.SetActivation(true);
        }
    }

    private void SetActivation(bool active)
    {
        IsActivated = active;
        if (glow)
            glow.gameObject.SetActive(active);
        if (teleportTrigger)
            teleportTrigger.enabled = active;

        if (geometry == null)
            return;

        var appearance = new MaterialPropertyBlock();
        foreach (var part in geometry)
        {
            if (!part)
                continue;

            var materials = part.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (!material || !material.HasProperty("_EmissionColor"))
                    continue;

                part.GetPropertyBlock(appearance, i);
                appearance.SetColor("_EmissionColor", active ? material.GetColor("_EmissionColor") : Color.black);
                if (part.transform == transform && i == 1 && material.HasProperty("_Color"))
                {
                    Color tint = material.color;
                    appearance.SetColor("_Color", active ? tint : new Color(tint.r * .28f, tint.g * .28f, tint.b * .28f, tint.a));
                }
                part.SetPropertyBlock(appearance, i);
            }
        }
    }

    private void LateUpdate()
    {
        if (!viewer || geometry == null)
            return;

        bool useFogLimit = RenderSettings.fog && RenderSettings.fogMode == FogMode.Linear;
        float rangeSquared = RenderSettings.fogEndDistance * RenderSettings.fogEndDistance;
        foreach (var part in geometry)
            if (part)
                part.forceRenderingOff = useFogLimit && part.bounds.SqrDistance(viewer.position) > rangeSquared;

        if (islandBeacon && island)
        {
            Vector3 offset = viewer.position - island.transform.position;
            Vector3 size = island.terrainData.size;
            islandBeacon.forceRenderingOff = offset.x < 0 || offset.x > size.x || offset.z < 0 || offset.z > size.z;
        }
    }

    private void OnDisable()
    {
        if (islandBeacon)
            islandBeacon.forceRenderingOff = true;

        if (geometry == null)
            return;
        foreach (var part in geometry)
            if (part)
                part.forceRenderingOff = false;
    }
}
