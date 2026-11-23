
using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public int woodReward = 3;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, InventorySystem inventory)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= damage;

        Debug.Log("Tree hit! Health: " +
            Mathf.Max(0f, currentHealth));

        if (currentHealth <= 0f)
        {
            if (inventory != null &&
                inventory.AddItem("Wood", woodReward))
            {
                Debug.Log("Tree chopped! Collected " +
                    woodReward + " Wood.");

                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
    }
}
