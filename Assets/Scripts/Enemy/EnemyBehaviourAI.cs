using Unity.VisualScripting;
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
    public LayerMask obstacleLayerMask;
    //public string obstacleTag = "Obstacle";
    public string enemyTag = "Enemy";
    private float wanderTimer = 0f;
    private float wanderInterval = 3f;
    private float currentTurnDirection = 0f;
    private float stuckSpinTimer = 0f;

    [Header("Edge Detection")]
    //public string groundTag = "Ground";
    public LayerMask groundLayerMask;
    public float edgeOffsetForward = 0.8f;
    public float edgeRayLength = 4.0f;

    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 130f;
    public float attackRange = 5.0f;

    [Header("Shooting Settings")]
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;

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
        FindPlayerReference(); //finds and stores Player reference when game starts
        wanderTimer = Random.Range(0f, wanderInterval); // sets intial wandering time
        desiredHeight = rb.position.y; //sets initial levitating height
    }

    // updates enemy's detection, patrol, chase, and attack behaviour every frame.
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
            // player no longer detected
            isEvading = false;
            PatrolAndAvoid();
        }
    }

    //controls enemy physcial movement and leviation at fixed intervals
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

    //evasive movement for enemy's backward and forward motion during Player attack
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
        
        //switch between moving backwards and forwards if target distances are reached
        if (movingBackward && distanceToPlayer >= targetDistance)
        {
            movingBackward = false;
        }
        else if (!movingBackward && distanceToPlayer <= targetDistance)
        {
            movingBackward = true;
        }

        //determines if enemy should move backwards or forwards 
        Vector3 moveDirection =
            movingBackward ? -transform.forward : transform.forward;

        Vector3 newPosition =
            rb.position + moveDirection * evadeSpeed * Time.fixedDeltaTime;

        newPosition.y = desiredHeight;

        rb.MovePosition(newPosition);
    }

    // maintaining levitation for enemy
    void MaintainLevitation()
    {
        // find the ground underneath enemy
        Vector3 rayStart = rb.position + Vector3.up * 0.5f;

        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 25f, groundLayerMask))
        {
                float targetY = hit.point.y + flightHeight;

                float newY = Mathf.Lerp(rb.position.y, targetY, Time.fixedDeltaTime * 5f);

                desiredHeight = newY;
        }

    }

    //enemy forward movement method
    void MoveForward()
    {
        Vector3 forwardMovement =
        transform.forward * moveSpeed * Time.fixedDeltaTime;

        Vector3 newPosition = rb.position + forwardMovement;

        if (!IsGroundAhead(newPosition))
        {
            return;
        }
        newPosition.y = desiredHeight; //maintains enemy's levitating height

        rb.MovePosition(newPosition); // moves enemy to new position
    }

    //looks for GameObject tagged as Player and stores its Transform reference

    void FindPlayerReference()
    {
        GameObject playerObj = GameObject.FindWithTag(playerTag);

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;

            //Debug.Log(gameObject.name + " FOUND THE PLAYER!");
        }

    }
    // checking for Player 
    void CheckForPlayer()
    {   // detecting for Player movement
        if (playerTransform == null) 
        {
            playerDetected = false;
            return;
        }

        Vector3 directionToPlayer = playerTransform.position - transform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);//directionToPlayer.magnitude;
        playerDetected = distanceToPlayer <= detectionRadius;

        // checks if Player is within detection radius and calculates
        // viewing between enemy's forward direction and Player
        if (distanceToPlayer <= detectionRadius)
        {
            float currentFieldOfView = fieldOfViewAngle;

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

            // checking if player is within enemy's field of view
            if (angle <= currentFieldOfView * 0.5f)
            { 
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; //sets origin just before enemy's sight
                Vector3 playerTarget = playerTransform.position + Vector3.up * 1.0f;

                // calculates the distance and direction between player's postion and enemy's ray cast
                Vector3 rayDirection = playerTarget - rayOrigin; 
                float rayDistance = rayDirection.magnitude;

                RaycastHit hit;
                //checks that line path between Player and enemy is clear
                if (Physics.Raycast(rayOrigin, rayDirection.normalized, out hit, rayDistance))
                {
                    //confirms player detection if ray cast hits Player
                    if (hit.collider.CompareTag(playerTag) || hit.collider.transform.root.CompareTag(playerTag))
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
    bool IsGroundAhead(Vector3 position)
    {
        Vector3 rayOrigin = position + transform.forward * edgeOffsetForward + Vector3.up * 0.5f;

        return Physics.Raycast(rayOrigin, Vector3.down, edgeRayLength, groundLayerMask);
    }

    // patrol for Player and avoid colliding with other enemies and obstacles
    void PatrolAndAvoid()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + (transform.forward * 1.0f) + (Vector3.up * 0.5f);// determining where the Raycast starts relative to enemy

        //if enemy Raycast hit obstacle or another enemy then path is blocked
        bool pathBlocked = false;

        if (!IsGroundAhead(transform.position))
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

            shouldMoveForward = false;

            return;
        }

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, obstacleDetectionDistance, obstacleLayerMask))
        {
            
                pathBlocked = true;
        }

        //if path is blocked, rotate enemy to a new direction
        if (pathBlocked)
        {
            stuckSpinTimer += Time.deltaTime;
            //if enemy has been stuck for over 1.5 secs, then rotate the enemy 90 degrees y axis and reset stuckSpinTimer
            if (stuckSpinTimer > 1.5f)
            {
                stuckSpinTimer = 0f;
                transform.Rotate(0, 90f, 0);
            }
            else
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0); // else rotate gradually while searching for new path
            }
        }
        else
        {
            stuckSpinTimer = 0f;
            shouldMoveForward = true; // move if not stuck

            wanderTimer += Time.deltaTime;

            //if enemy has been wandering longer than wanderInteval, change rotation direction
            if (wanderTimer >= wanderInterval)
            {
                wanderTimer = 0f;
                currentTurnDirection = Random.Range(-1f, 1f);
                wanderInterval = Random.Range(2f, 5f);
            }

            transform.Rotate(0, currentTurnDirection * (rotationSpeed * 0.3f) * Time.deltaTime, 0);
        }
    }

    //fire projectiles towards player at specified time intervals 
    void Shoot()
    {
        if (Time.time >= nextFireTime)
        {   
            nextFireTime = Time.time + fireRate; //fire enemy at the given rate from time to time

            if (EnemyBullet != null)
            {   //spawn projectiles and shoot from enemy's given direction
                Vector3 spawnPos = transform.position + transform.forward * 1.0f;
                Quaternion lookRotation = Quaternion.LookRotation((playerTransform.position - spawnPos).normalized);
                GameObject spawnedBullet = Instantiate(EnemyBullet, spawnPos, lookRotation);

                EnemyBullet bulletScript = spawnedBullet.GetComponent<EnemyBullet>(); //getting the bullet script
                if (bulletScript != null)
                {
                    bulletScript.Initialize(bulletMoveSpeed, bulletLifetime); // if bullet script exists, initialise its speed and lifetime
                }
            }
        }
    }

    // displays enemy's detection range, attack range, field of view, and obstacle detection rays in Scene view
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