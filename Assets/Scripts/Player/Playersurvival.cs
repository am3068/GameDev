using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSurvival : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger;

    [Tooltip("How much hunger is lost per second.")]
    [SerializeField] private float hungerDrainPerSecond = 0.2f;

    [Tooltip("Health lost per second when hunger reaches zero.")]
    [SerializeField] private float starvationDamagePerSecond = 5f;

    private bool isDead;

    // UI and enemy AI are able to read these values.
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHunger => currentHunger;
    public float MaxHunger => maxHunger;
    public bool IsDead => isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
    }

    private void Update()
    {
        if (isDead)
            return;

        // Hunger decreases over time.
        currentHunger = Mathf.Max(
            0f,
            currentHunger - hungerDrainPerSecond * Time.deltaTime
        );

        // Starvation will damage the player when hunger reaches zero.
        if (currentHunger <= 0f)
        {
            TakeDamage(starvationDamagePerSecond * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void RestoreHunger(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
    }

    private void Die()
    {
        isDead = true;

        // Stops player movement.
        PlayerController controller = GetComponent<PlayerController>();

        if (controller != null)
            controller.enabled = false;

        // Releases the mouse cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("You have died...");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "plant")
        {
            other.gameObject.SetActive(false);
            RestoreHealth(50f);
        }
            
        else if (other.tag == "food")
        {
            other.gameObject.SetActive(false);
            RestoreHunger(50f);
        }
    }
}
