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

    // Values available to the UI, inventory and enemy AI.
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

        // hunger decreases
        currentHunger = Mathf.Max(
            0f,
            currentHunger - hungerDrainPerSecond * Time.deltaTime
        );

        // starvation damages player once hunger = 0
        if (currentHunger <= 0f)
        {
            TakeDamage(
                starvationDamagePerSecond * Time.deltaTime
            );
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - amount
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );
    }

    public void RestoreHunger(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHunger = Mathf.Min(
            maxHunger,
            currentHunger + amount
        );
    }

    private void Die()
    {
        isDead = true;

        // stop player movement once dead
        PlayerController controller =
            GetComponent<PlayerController>();

        if (controller != null)
            controller.enabled = false;

        // release mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("You have died...");
    }
}