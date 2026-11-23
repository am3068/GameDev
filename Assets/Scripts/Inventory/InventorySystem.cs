
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private List<string> itemNames = new List<string>();
    private List<int> itemCounts = new List<int>();

    private PlayerSurvival survival;

    private bool axeEquipped = false;

    public bool IsAxeEquipped
    {
        get { return axeEquipped; }
    }

    private void Awake()
    {
        survival = GetComponent<PlayerSurvival>();
    }

    private void OnTriggerEnter(Collider other)
    {
        string item = "";

        if (other.CompareTag("wood"))
            item = "Wood";
        else if (other.CompareTag("food"))
            item = "Food";
        else if (other.CompareTag("plant"))
            item = "Plant";
        else if (other.CompareTag("ore"))
            item = "Ore";

        if (item == "")
            return;

        int slot = itemNames.IndexOf(item);

        if (slot >= 0)
        {
            // item exists so increase its count.
            itemCounts[slot]++;
        }
        else
        {
            // new item goes into next empty slot.
            if (itemNames.Count >= 10)
                return;

            itemNames.Add(item);
            itemCounts.Add(1);
        }

        other.gameObject.SetActive(false);
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


    public int GetItemQuantity(string item)
    {
        int slot = itemNames.IndexOf(item);

        if (slot == -1)
            return 0;

        return itemCounts[slot];
    }

    public bool RemoveItem(string item, int amount)
    {
        if (amount <= 0)
            return false;

        int slot = itemNames.IndexOf(item);

        if (slot == -1 || itemCounts[slot] < amount)
            return false;

        itemCounts[slot] -= amount;

        if (itemCounts[slot] == 0)
        {
            itemNames.RemoveAt(slot);
            itemCounts.RemoveAt(slot);
        }

        return true;
    }

    public bool AddItem(string item, int amount)
    {
        if (string.IsNullOrEmpty(item) || amount <= 0)
            return false;

        int slot = itemNames.IndexOf(item);

        if (slot >= 0)
        {
            itemCounts[slot] += amount;
            return true;
        }

        if (itemNames.Count >= 10)
            return false;

        itemNames.Add(item);
        itemCounts.Add(amount);
        return true;
    }


    public bool CraftAxe()
    {
        if (GetItemQuantity("Wood") < 2 ||
            GetItemQuantity("Ore") < 1)
        {
            Debug.Log("Not enough resources to craft an axe!");
            return false;
        }

        if (GetItemQuantity("Axe") == 0 && itemNames.Count >= 10)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        RemoveItem("Wood", 2);
        RemoveItem("Ore", 1);
        AddItem("Axe", 1);

        Debug.Log("Axe crafted successfully!");
        return true;
    }


    public void EquipAxe()
    {
        if (GetItemQuantity("Axe") < 1)
        {
            Debug.Log("You don't have an axe!");
            axeEquipped = false;
            return;
        }

        axeEquipped = !axeEquipped;

        if (axeEquipped)
            Debug.Log("Axe equipped!");
        else
            Debug.Log("Axe unequipped!");
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
