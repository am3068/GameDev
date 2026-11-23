
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Punch Settings")]
    public float attackRange = 2f;
    public float attackDamage = 25f;
    public float attackRadius = 0.75f;
    public float attackCooldown = 0.5f;

    private float nextAttackTime;

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera is not assigned!");
            return;
        }

        Vector3 attackCenter =
            playerCamera.transform.position +
            playerCamera.transform.forward * attackRange;

        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            attackRadius
        );

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);

                Debug.Log("Punched enemy! Damage: " + attackDamage);
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 attackCenter =
            playerCamera.transform.position +
            playerCamera.transform.forward * attackRange;

        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }
}
