using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePortalController : MonoBehaviour
{
    public DayNightCycle dayNightCycle;
    public GameObject portalCentre;
    public int unlockDay = 4;

    private bool isUnlocked = false;
    private bool hasEscaped = false;

    void Start()
    {
        portalCentre.SetActive(false);
    }

    void Update()
    {
        if (!isUnlocked && dayNightCycle.currentDay >= unlockDay)
        {
            isUnlocked = true;
            portalCentre.SetActive(true);

            Debug.Log("Escape portal is now OPEN!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isUnlocked && !hasEscaped &&
            other.CompareTag("Player"))
        {
            hasEscaped = true;
            Debug.Log("YOU ESCAPED! YOU WIN!");
        }
    }
}

