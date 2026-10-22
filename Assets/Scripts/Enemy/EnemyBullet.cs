
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float bulletMoveSpeed = 10f;
    private float bulletLifetime = 3f;
    private Vector3 moveDirection;

    public void Initialize(float speed, float lifetime)
    {
        bulletMoveSpeed = speed;
        bulletLifetime = lifetime;
        moveDirection = transform.forward;

        Destroy(gameObject, bulletLifetime);
    }

    private void Update()
    {
        transform.position +=
            moveDirection * bulletMoveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerSurvival survival =
            other.GetComponentInParent<PlayerSurvival>();

        if (survival != null)
        {
            survival.TakeDamage(10f);
            Debug.Log("PLAYER HIT! 10 DAMAGE DEALT!");
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
