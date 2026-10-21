using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private int wood;
    private int ore;

    // Start is called before the first frame update
    void Start()
    {
        wood = 0;
        ore = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WoodPickUp")
            wood++;
        else if (other.tag == "StonePickUp")
            ore++;
    }

    public int GetWood()
    {
        return wood;
    }

    public int GetOre()
    {
        return ore;
    }
}
