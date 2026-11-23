
using UnityEngine;

public class SwordVisual : MonoBehaviour
{
    public GameObject swordObject;

    private InventorySystem inventory;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Start()
    {
        if (swordObject != null)
            swordObject.SetActive(false);
    }

    private void Update()
    {
        if (inventory == null || swordObject == null)
            return;

        bool shouldShow = inventory.IsSwordEquipped;

        if (swordObject.activeSelf != shouldShow)
            swordObject.SetActive(shouldShow);
    }
}

