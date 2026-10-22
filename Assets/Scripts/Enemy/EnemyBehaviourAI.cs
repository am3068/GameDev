
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviourAI : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject EnemyBullet;
    public float bulletMoveSpeed = 10f;
    public float bulletLifetime = 3f;
    public float fireRate = 1f;
    public float attackRange = 5f;

    private float nextFireTime;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float patrolSpeed = 1.5f;
    public float rotationSpeed = 120f;

    [Header("Patrol and Obstacles")]
    public float wanderInterval = 1.5f;
    public float obstacleDetectionDistance = 2f;
    public LayerMask obstacleLayerMask;

    private float wanderTimer;
    private float currentTurnDirection;

    [Header("Ground and Height")]
    public LayerMask groundLayerMask;
    public float flightHeight = 0.5f;
    public float edgeOffsetForward = 0.8f;
    public float edgeRayLength = 4f;

    [Header("Player Detection")]
    public string playerTag = "Player";
    public float detectionRadius = 10f;
    public float losePlayerDistance = 14f;
    public float fieldOfViewAngle = 130f;

    private Transform playerTransform;
    private Rigidbody rb;
    private bool playerDetected;
    private float desiredHeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.drag = 2f;
    }

    private void Start()
    {
        FindPlayerReference();
        wanderTimer = Random.Range(0f, wanderInterval);
        desiredHeight = rb.position.y;
    }

    private void Update()
    {
        if (playerTransform == null)
            FindPlayerReference();

        CheckForPlayer();

        if (playerDetected && playerTransform != null)
        {
            Vector3 direction =
                playerTransform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            PatrolAndAvoid();
        }
    }

    private void FixedUpdate()
    {
        MaintainHeight();

        if (!playerDetected || playerTransform == null)
        {
            // Patrol slowly when the player is not detected.
            if (IsGroundAhead() && !IsPathBlocked())
                MoveAtSpeed(patrolSpeed);

            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            playerTransform.position
        );

        // Chase and shoot only while the player is within the chase distance.
        if (distance > losePlayerDistance)
            return;

        if (distance > attackRange)
        {
            if (IsGroundAhead() && !IsPathBlocked())
                MoveAtSpeed(moveSpeed);
        }

        Shoot();
    }

    private void FindPlayerReference()
    {
        GameObject playerObject = GameObject.FindWithTag(playerTag);

        if (playerObject != null)
            playerTransform = playerObject.transform;
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null)
        {
            playerDetected = false;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            playerTransform.position
        );

        if (playerDetected && distance > losePlayerDistance)
        {
            playerDetected = false;
            return;
        }

        if (!playerDetected && distance > detectionRadius)
            return;

        Vector3 direction =
            playerTransform.position - transform.position;
        direction.y = 0f;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > fieldOfViewAngle * 0.5f)
        {
            playerDetected = false;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = playerTransform.position;
        Vector3 rayDirection = target - origin;

        if (Physics.Raycast(
            origin,
            rayDirection.normalized,
            out RaycastHit hit,
            rayDirection.magnitude))
        {
            playerDetected =
                hit.collider.CompareTag(playerTag) ||
                hit.collider.transform.root.CompareTag(playerTag);
        }
        else
        {
            playerDetected = false;
        }
    }

    private void PatrolAndAvoid()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            wanderTimer = 0f;
            currentTurnDirection = Random.Range(-1f, 1f);
            wanderInterval = Random.Range(0.8f, 2f);
        }

        if (!IsGroundAhead() || IsPathBlocked())
        {
            currentTurnDirection = 1f;
            wanderTimer = 0f;
        }

        transform.Rotate(
            0f,
            currentTurnDirection * rotationSpeed * 0.5f *
            Time.deltaTime,
            0f
        );
    }

    private void MoveAtSpeed(float speed)
    {
        Vector3 movement =
            transform.forward * speed * Time.fixedDeltaTime;

        Vector3 newPosition = rb.position + movement;
        newPosition.y = desiredHeight;

        rb.MovePosition(newPosition);
    }

    private void MaintainHeight()
    {
        Vector3 rayStart = rb.position + Vector3.up * 0.5f;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            25f,
            groundLayerMask))
        {
            desiredHeight = Mathf.Lerp(
                desiredHeight,
                hit.point.y + flightHeight,
                Time.fixedDeltaTime * 5f
            );

            Vector3 position = rb.position;
            position.y = desiredHeight;
            rb.MovePosition(position);
        }
    }

    private bool IsGroundAhead()
    {
        Vector3 rayOrigin =
            rb.position +
            transform.forward * edgeOffsetForward +
            Vector3.up * 0.5f;

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            edgeRayLength,
            groundLayerMask
        );
    }

    private bool IsPathBlocked()
    {
        Vector3 origin = rb.position + Vector3.up * 0.5f;

        return Physics.Raycast(
            origin,
            transform.forward,
            obstacleDetectionDistance,
            obstacleLayerMask
        );
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime || EnemyBullet == null)
            return;

        nextFireTime = Time.time + fireRate;

        // Aim at the player's actual position, not above their head.
        Vector3 spawnPosition =
            transform.position + transform.forward * 0.8f;

        Vector3 targetPosition = playerTransform.position;
        Vector3 direction = targetPosition - spawnPosition;

        if (direction.sqrMagnitude < 0.01f)
            return;

        GameObject bullet = Instantiate(
            EnemyBullet,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        EnemyBullet bulletScript =
            bullet.GetComponent<EnemyBullet>();

        if (bulletScript != null)
            bulletScript.Initialize(bulletMoveSpeed, bulletLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, losePlayerDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 leftRay = Quaternion.Euler(
            0f, -fieldOfViewAngle * 0.5f, 0f
        ) * transform.forward;

        Vector3 rightRay = Quaternion.Euler(
            0f, fieldOfViewAngle * 0.5f, 0f
        ) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            transform.position, leftRay * detectionRadius
        );
        Gizmos.DrawRay(
            transform.position, rightRay * detectionRadius
        );
    }
}
