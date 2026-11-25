using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class CrystalPickupMarker : MonoBehaviour
{
    private static readonly List<CrystalPickupMarker> active = new List<CrystalPickupMarker>();
    private BoxCollider pickup;

    public static IReadOnlyList<CrystalPickupMarker> Active => active;

    public Vector3 Position
    {
        get
        {
            Bounds bounds = pickup.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + .15f, bounds.center.z);
        }
    }

    private void Awake()
    {
        pickup = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
    }
}
