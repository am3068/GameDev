
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

        if (Keyboard.current.cKey.wasPressedThisFrame)
            inventory.CraftAxe();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            inventory.EquipAxe();

        if (Keyboard.current.vKey.wasPressedThisFrame)
            inventory.CraftSword();

        if (Keyboard.current.rKey.wasPressedThisFrame)
            inventory.EquipSword();
    }
}

