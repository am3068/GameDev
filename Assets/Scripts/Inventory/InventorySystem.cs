
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private List<string> itemNames = new List<string>();
    private List<int> itemCounts = new List<int>();

    private PlayerSurvival survival;

    private void Awake()
    {
        survival = GetComponent<PlayerSurvival>();
    }

    public static string GetPickupName(Collider target)
    {
        if (!target || !target.gameObject.activeInHierarchy)
            return null;

        switch (target.tag)
        {
            case "wood": return "Wood";
            case "food": return "Food";
            case "plant": return "Plant";
            case "ore": return "Ore";
            default: return null;
        }
    }

    public bool CanCollect(string item)
    {
        return item != null && (itemNames.Count < 10 || itemNames.Contains(item));
    }

    public void Collect(Collider target)
    {
        string item = GetPickupName(target);
        if (!CanCollect(item))
            return;

        int slot = itemNames.IndexOf(item);

        if (slot >= 0)
        {
            // item exists so increase its count.
            itemCounts[slot]++;
        }
        else
        {
            itemNames.Add(item);
            itemCounts.Add(1);
        }

        target.gameObject.SetActive(false);
    }

    public void UseItemInSlot(int slot)
    {
        if (survival == null || survival.IsDead)
            return;

        if (slot < 0 || slot >= itemNames.Count)
            return;

        string item = itemNames[slot];

        if (item == "Food")
        {
            if (survival.CurrentHunger >= survival.MaxHunger)
                return;

            survival.RestoreHunger(50f);
        }
        else if (item == "Plant")
        {
            if (survival.CurrentHealth >= survival.MaxHealth)
                return;

            survival.RestoreHealth(50f);
        }
        else
        {
            return;
        }

        itemCounts[slot]--;

        if (itemCounts[slot] == 0)
        {
            itemNames.RemoveAt(slot);
            itemCounts.RemoveAt(slot);
        }
    }

    public int GetSlotCount()
    {
        return itemNames.Count;
    }

    public string GetItemName(int slot)
    {
        return itemNames[slot];
    }

    public int GetItemCount(int slot)
    {
        return itemCounts[slot];
    }
}
