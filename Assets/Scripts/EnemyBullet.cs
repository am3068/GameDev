using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float bulletMoveSpeed = 10f;
    private float bulletLifetime = 3f;

    // Called by the enemy script upon instantiation to set speed and die time
    public void Initialize(float speed, float lifetime)
    {
        bulletMoveSpeed = speed;
        bulletLifetime = lifetime;
        Destroy(gameObject, bulletLifetime); // Auto-destroy after set time if it misses
    }

    void Update()
    {
        BulletMove();
    }

    private void BulletMove()
    {
        transform.Translate(Vector3.forward * bulletMoveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}