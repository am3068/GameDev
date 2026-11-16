
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destination;
    public float teleportDelay = 1f;

    bool canTeleport = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !canTeleport)
            return;

        PortalTeleport[] portals = FindObjectsOfType<PortalTeleport>();

        foreach (PortalTeleport portal in portals)
            portal.canTeleport = false;

        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        other.transform.position = destination.position;

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
