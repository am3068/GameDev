using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingInput : MonoBehaviour
{
    private InventorySystem inventory;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Update()
    {
        if (Keyboard.current == null || inventory == null)
            return;

        // Press C to craft an axe.
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            inventory.CraftAxe();
        }

        // Press E to equip or unequip the axe.
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            inventory.EquipAxe();
        }
    }
}
