
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isDead = false;

    private EnemyBehaviourAI enemyAI;
    private Collider[] enemyColliders;
    private Renderer[] enemyRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        enemyAI = GetComponent<EnemyBehaviourAI>();
        enemyColliders = GetComponentsInChildren<Collider>();
        enemyRenderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            originalColors[i] = enemyRenderers[i].material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        FlashRed();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void FlashRed()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    IEnumerator FlashRedCoroutine()
    {
        foreach (Renderer enemyRenderer in enemyRenderers)
        {
            if (enemyRenderer != null)
                enemyRenderer.material.color = hitColor;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null)
                enemyRenderers[i].material.color = originalColors[i];
        }

        flashCoroutine = null;
    }

    void Die()
    {
        isDead = true;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null)
            {
                enemyRenderers[i].material.color = originalColors[i];
                enemyRenderers[i].enabled = false;
            }
        }

        foreach (Collider enemyCollider in enemyColliders)
            enemyCollider.enabled = false;

        if (enemyAI != null)
            enemyAI.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void Respawn()
    {
        if (!isDead) return;

        currentHealth = maxHealth;
        isDead = false;

        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
        }

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null)
            {
                enemyRenderers[i].material.color = originalColors[i];
                enemyRenderers[i].enabled = true;
            }
        }

        foreach (Collider enemyCollider in enemyColliders)
            enemyCollider.enabled = true;

        if (enemyAI != null)
            enemyAI.enabled = true;
    }
}
