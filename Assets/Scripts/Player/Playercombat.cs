
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Camera playerCamera;
    public float attackRange = 2f;
    public float attackDamage = 25f;

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            if (hit.collider.CompareTag("Enemy") ||
                hit.collider.transform.root.CompareTag("Enemy"))
            {
                EnemyHealth enemy =
                    hit.collider.GetComponentInParent<EnemyHealth>();

                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                    Debug.Log("Hit enemy! Damage: " + attackDamage);
                }
            }
        }
    }
}
