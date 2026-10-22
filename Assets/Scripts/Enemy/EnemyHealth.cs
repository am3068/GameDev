using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isDead = false;

    private EnemyBehaviourAI enemyAI;
    private Collider[] enemyColliders;
    private Renderer[] enemyRenderers;

    void Start()
    {
        currentHealth = maxHealth;

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        enemyAI = GetComponent<EnemyBehaviourAI>();
        enemyColliders = GetComponentsInChildren<Collider>();
        enemyRenderers = GetComponentsInChildren<Renderer>();
    }



    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (enemyAI != null)
            enemyAI.enabled = false;

        foreach (Renderer enemyRenderer in enemyRenderers)
        {
            enemyRenderer.enabled = false;
        }

        foreach (Collider enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void Respawn()
    {
        if (!isDead)
            return;

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

        foreach (Renderer enemyRenderer in enemyRenderers)
        {
            enemyRenderer.enabled = true;
        }

        foreach (Collider enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = true;
        }

        if (enemyAI != null)
            enemyAI.enabled = true;
    }
}

