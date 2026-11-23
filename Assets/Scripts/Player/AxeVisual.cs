using UnityEngine;

public class AxeVisual : MonoBehaviour
{
    public GameObject axeObject;

    private InventorySystem inventory;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Start()
    {
        if (axeObject != null)
            axeObject.SetActive(false);
    }

    private void Update()
    {
        if (inventory == null || axeObject == null)
            return;

        bool shouldShow = inventory.IsAxeEquipped;

        if (axeObject.activeSelf != shouldShow)
            axeObject.SetActive(shouldShow);
    }
}
