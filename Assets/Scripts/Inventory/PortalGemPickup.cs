using UnityEngine;

public class PortalGemPickup : MonoBehaviour
{
    public const string ForestGem = "Forest Gem";

    public static void DropForestGem(GameObject prefab, Vector3 position, Collider ground)
    {
        if (ground && ground.Raycast(new Ray(position + Vector3.up, Vector3.down), out var hit, 20f))
            position.y = hit.point.y;

        var gem = Instantiate(prefab, position, Quaternion.identity);
        gem.name = ForestGem;
        gem.AddComponent<PortalGemPickup>();
        foreach (var collider in gem.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        var renderer = gem.GetComponentInChildren<Renderer>();
        gem.transform.localScale *= .55f / Mathf.Max(.01f, renderer.bounds.size.y);
        gem.transform.position += Vector3.up * (position.y - renderer.bounds.min.y + .06f);

        var pickup = gem.AddComponent<BoxCollider>();
        pickup.center = gem.transform.InverseTransformPoint(renderer.bounds.center);
        pickup.size = renderer.bounds.size / gem.transform.localScale.x + Vector3.one * .25f;
        pickup.isTrigger = true;
        gem.AddComponent<CrystalPickupMarker>();
    }
}
