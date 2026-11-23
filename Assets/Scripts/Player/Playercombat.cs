
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

    [Header("Axe Settings")]
    public float axeDamage = 35f;
    public float axeRange = 3f;

    private float nextAttackTime;
    private InventorySystem inventory;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera is not assigned!");
            return;
        }

        if (inventory != null && inventory.IsAxeEquipped)
        {
            ChopTree();
            return;
        }

        PunchEnemy();
    }

    private void ChopTree()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, axeRange))
        {
            TreeHealth tree =
                hit.collider.GetComponentInParent<TreeHealth>();

            if (tree != null)
            {
                tree.TakeDamage(axeDamage, inventory);
            }
        }
    }

    private void PunchEnemy()
    {
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

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 attackCenter =
            playerCamera.transform.position +
            playerCamera.transform.forward * attackRange;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );
    }
}
