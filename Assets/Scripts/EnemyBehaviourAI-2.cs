using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviourAI : MonoBehaviour
{
    [Header("EnemyBullet Settings")]
    public GameObject EnemyBullet;
    public float bulletMoveSpeed = 10f;
    public float bulletLifetime = 3f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 50f;
    public float flightHeight = 2.0f;
    private float desiredHeight;
    private bool shouldMoveForward;

    [Header("Evasive Attack Movement")]
    public float evadeDistance = 2f;
    public float evadeSpeed = 2f;
    private bool isEvading = false;
    private float evadeStartDistance;
    private bool movingBackward = true;

    [Header("Patrol & Avoidance")]
    public float obstacleDetectionDistance = 2.0f;
    public string obstacleTag = "Obstacle";
    public string enemyTag = "Enemy";
    private float wanderTimer = 0f;
    private float wanderInterval = 3f;
    private float currentTurnDirection = 0f;
    private float stuckSpinTimer = 0f;

    [Header("Edge Detection")]
    public string groundTag = "Ground";
    public float edgeOffsetForward = 0.8f;
    public float edgeRayLength = 4.0f;

    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionRadius = 30f;
    public float fieldOfViewAngle = 130f;
    public float attackRange = 9.0f;

    [Header("Shooting Settings")]
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;

    // References & State
    private Transform playerTransform;
    private bool playerDetected = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.drag = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        FindPlayerReference();
        wanderTimer = Random.Range(0f, wanderInterval);
        desiredHeight = rb.position.y;
    }

    void Update()
    {
        shouldMoveForward = false;
        if (playerTransform == null)
        {
            
            FindPlayerReference();
        }

        CheckForPlayer();

        if (playerDetected && playerTransform != null)
        {
            // --- CHASE & SHOOT STATE ---
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            Vector3 targetDir = (playerTransform.position - transform.position).normalized;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 0.1f);
            }

            if (distanceToPlayer <= attackRange || isEvading)
            {
                if (!isEvading)
                {
                    isEvading = true;
                    movingBackward = true;
                    evadeStartDistance = distanceToPlayer;
                }

                rb.velocity = new Vector3(0, rb.velocity.y, 0);

                Shoot();
            }
            else
            {
                shouldMoveForward = true;
            }
        }
        else
        {
            // --- PATROL & WANDER STATE ---
            // Player no longer detected
            isEvading = false;
            PatrolAndAvoid();
        }
    }

    void FixedUpdate()
    {
        MaintainLevitation();

        if (isEvading)
        {
            EvasiveMovement();
        }
        else if (shouldMoveForward)
        {
            MoveForward();
        }
        else
        {
            Vector3 newPosition = rb.position;
            newPosition.y = desiredHeight;

            rb.MovePosition(newPosition);
        }
    }

    //Evasive movement for attack
    void EvasiveMovement()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(
            rb.position,
            playerTransform.position
        );

        float targetDistance;

        if (movingBackward)
        {
            targetDistance = evadeStartDistance + evadeDistance;
        }
        else
        {
            targetDistance = evadeStartDistance;
        }

        if (movingBackward && distanceToPlayer >= targetDistance)
        {
            movingBackward = false;
        }
        else if (!movingBackward && distanceToPlayer <= targetDistance)
        {
            movingBackward = true;
        }

        Vector3 moveDirection =
            movingBackward ? -transform.forward : transform.forward;

        Vector3 newPosition =
            rb.position + moveDirection * evadeSpeed * Time.fixedDeltaTime;

        newPosition.y = desiredHeight;

        rb.MovePosition(newPosition);
    }

    void MaintainLevitation()
    {
        // Find the ground beneath the insect
        Vector3 rayStart = rb.position + Vector3.up * 0.5f;

        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 25f))
        {
            if (hit.collider.CompareTag(groundTag))
            {
                // Calculate the height we want to maintain
                float targetY = hit.point.y + flightHeight;

                // Gradually move towards the target height
                float newY = Mathf.Lerp(
                    rb.position.y,
                    targetY,
                    Time.fixedDeltaTime * 5f
                );

                // Save the vertical target instead of moving immediately
                desiredHeight = newY;
            }
        }

    }

    void MoveForward()
    {
        Vector3 forwardMovement =
        transform.forward * moveSpeed * Time.fixedDeltaTime;

        Vector3 newPosition = rb.position + forwardMovement;

        // Apply the desired flying height
        newPosition.y = desiredHeight;

        rb.MovePosition(newPosition);
    }

    void FindPlayerReference()
    {
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    bool IsGroundAhead()
    {
        Vector3 rayOrigin = transform.position + (transform.forward * edgeOffsetForward) + Vector3.up * 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, edgeRayLength))
        {
            if (hit.collider.CompareTag(groundTag))
            {
                return true;
            }
        }

        return false;
    }

    void CheckForPlayer()
    {
        if (playerTransform == null)
        {
            playerDetected = false;
            return;
        }

        Vector3 directionToPlayer =
            playerTransform.position - transform.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRadius)
        {
            // Default field of view (130 degrees)
            float currentFieldOfView = fieldOfViewAngle;

            // Give the insect 360-degree vision within 15 units
            if (distanceToPlayer <= detectionRadius * 0.5f)
            {
                currentFieldOfView = 360f;
            }

            Vector3 flatForward = transform.forward;
            flatForward.y = 0;

            Vector3 flatDirToPlayer = directionToPlayer;
            flatDirToPlayer.y = 0;

            float angle = Vector3.Angle(
                flatForward,
                flatDirToPlayer
            );

            // Check the adjusted field of view
            if (angle <= currentFieldOfView * 0.5f)
            {
                Vector3 rayOrigin =
                    transform.position + transform.forward * 0.5f;

                Vector3 rayDirection =
                    playerTransform.position - rayOrigin;

                float rayDistance = rayDirection.magnitude;

                RaycastHit hit;

                if (Physics.Raycast(
                    rayOrigin,
                    rayDirection.normalized,
                    out hit,
                    rayDistance))
                {
                    if (hit.collider.CompareTag(playerTag))
                    {
                        playerDetected = true;
                        stuckSpinTimer = 0f;
                        return;
                    }
                }
            }
        }

        playerDetected = false;
    }

    void PatrolAndAvoid()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + (transform.forward * 1.0f) + (Vector3.up * 0.5f);

        bool pathBlocked = false;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, obstacleDetectionDistance))
        {
            if (hit.collider.CompareTag(obstacleTag) || hit.collider.CompareTag(enemyTag))
            {
                pathBlocked = true;
            }
        }

        //bool nearEdge = !IsGroundAhead();

        if (pathBlocked)
        {
            stuckSpinTimer += Time.deltaTime;

            if (stuckSpinTimer > 1.5f)
            {
                stuckSpinTimer = 0f;
                transform.Rotate(0, 90f, 0);
            }
            else
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            stuckSpinTimer = 0f;
            shouldMoveForward = true;

            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                wanderTimer = 0f;
                currentTurnDirection = Random.Range(-1f, 1f);
                wanderInterval = Random.Range(2f, 5f);
            }

            transform.Rotate(0, currentTurnDirection * (rotationSpeed * 0.3f) * Time.deltaTime, 0);
        }
    }

    void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (EnemyBullet != null)
            {
                Vector3 spawnPos = transform.position + transform.forward * 1.0f;
                Quaternion lookRotation = Quaternion.LookRotation((playerTransform.position - spawnPos).normalized);
                GameObject spawnedBullet = Instantiate(EnemyBullet, spawnPos, lookRotation);

                EnemyBullet bulletScript = spawnedBullet.GetComponent<EnemyBullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(bulletMoveSpeed, bulletLifetime);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 edgeRayOrigin = transform.position + (transform.forward * edgeOffsetForward) + Vector3.up * 0.5f;
        Gizmos.DrawRay(edgeRayOrigin, Vector3.down * edgeRayLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, leftRay * detectionRadius);
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, rightRay * detectionRadius);

        Gizmos.color = Color.cyan;
        Vector3 forwardRayOrigin = transform.position + (transform.forward * 1.0f) + (Vector3.up * 0.5f);
        Gizmos.DrawRay(forwardRayOrigin, transform.forward * obstacleDetectionDistance);
    }
}