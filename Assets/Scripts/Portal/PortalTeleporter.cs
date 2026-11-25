
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destination;
    public float teleportDelay = 1f;

    bool canTeleport = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !canTeleport || !destination)
            return;

        PortalTeleport[] portals = FindObjectsOfType<PortalTeleport>();

        foreach (PortalTeleport portal in portals)
            portal.canTeleport = false;

        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        Vector3 position = destination.position;

        foreach (Terrain terrain in Terrain.activeTerrains)
        {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            if (position.x >= origin.x && position.x <= origin.x + size.x &&
                position.z >= origin.z && position.z <= origin.z + size.z)
            {
                position.y = terrain.SampleHeight(position) + origin.y + 2f;
                break;
            }
        }

        other.transform.position = position;

        if (controller != null)
            controller.enabled = true;

                Invoke(nameof(ResetPortal), teleportDelay);
            }

    void ResetPortal()
    {
        PortalTeleport[] portals = FindObjectsOfType<PortalTeleport>();

        foreach (PortalTeleport portal in portals)
            portal.canTeleport = true;
    }
}
