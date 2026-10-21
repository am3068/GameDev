using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float bulletMoveSpeed = 10f;
    private float bulletLifetime = 3f;

    // called by the enemy script upon instantiation to set speed and die time
    public void Initialize(float speed, float lifetime)
    {
        bulletMoveSpeed = speed;
        bulletLifetime = lifetime;
        Destroy(gameObject, bulletLifetime); // auto-destroy bullets after set time if it misses
    }

    //updates bullet motion
    void Update()
    {
        BulletMove();
    }

    //bullet motion method
    private void BulletMove()
    {
        transform.Translate(Vector3.forward * bulletMoveSpeed * Time.deltaTime);
    }

    //handles bullet collison
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //destroy both Player and bullet upon collision
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            //destroy bullet when it hits other enemies, Obstacles or Ground
            Destroy(gameObject);
        }
    }
}