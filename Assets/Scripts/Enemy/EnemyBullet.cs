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
        // Check whether the projectile hit the player
        PlayerSurvival playerSurvival =
            other.GetComponentInParent<PlayerSurvival>();

        if (playerSurvival != null)
        {
            // Reduce player health by 10 points
            playerSurvival.TakeDamage(10f);

            Debug.Log("PLAYER HIT! 10 DAMAGE DEALT!");

            // Destroy bullet after hitting player
            Destroy(gameObject);
            return;
        }

        // Destroy bullet if it hits an obstacle or the ground
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}