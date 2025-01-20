using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float patrolRadius = 10f;
    public float detectionRadius = 7f;
    public float attackRadius = 2.25f;
    public float attackCooldown = 0f;
    public float ignorePlayerTime = 5f;
    public Transform hammer;
    [SerializeField] private GameObject hammerTrigger;
    public float swingSpeed = 100f;

    private NavMeshAgent agent;
    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private float ignorePlayerTimer = 0f;

    private bool isAttacking = false;
    private bool isSwinging = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    public bool isPlayerInHammerTrigger = false;

    private enum EnemyState { Patrolling, Chasing, Attacking, IgnoringPlayer }
    private EnemyState currentState;

    [SerializeField] private PlayerStateMachine playerStateMachine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = EnemyState.Patrolling;
        MoveToRandomPatrolPoint();

        initialRotation = hammer.localRotation;
        targetRotation = Quaternion.Euler(initialRotation.eulerAngles + new Vector3(45f, 0f, 0f));
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRadius && ignorePlayerTimer <= 0f)
                {
                    currentState = EnemyState.Chasing;
                }
                else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    MoveToRandomPatrolPoint();
                }
                break;

            case EnemyState.Chasing:
                if (playerStateMachine.currentStateEnum == PlayerStates.Springy)
                {
                    currentState = EnemyState.IgnoringPlayer;
                    ignorePlayerTimer = ignorePlayerTime;
                    break;
                }

                if (distanceToPlayer > detectionRadius)
                {
                    currentState = EnemyState.Patrolling;
                    MoveToRandomPatrolPoint();
                }
                else if (distanceToPlayer <= attackRadius)
                {
                    currentState = EnemyState.Attacking;
                    isAttacking = true;
                }
                else
                {
                    agent.SetDestination(player.position);
                }
                break;

            case EnemyState.Attacking:
                if (isAttacking)
                {
                    KeepPlayerAtAttackRadius();

                    // Swing hammer and check if the player was hit
                    if (!isSwinging && Time.time >= lastAttackTime + attackCooldown)
                    {
                        StartCoroutine(SwingHammer());
                    }
                }

                if (distanceToPlayer > attackRadius)
                {
                    isAttacking = false;
                    currentState = EnemyState.Chasing;
                }
                break;

            case EnemyState.IgnoringPlayer:
                ignorePlayerTimer -= Time.deltaTime;
                if (ignorePlayerTimer <= 0f)
                {
                    currentState = EnemyState.Patrolling;
                    MoveToRandomPatrolPoint();
                }
                break;
        }
    }

    void MoveToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void KeepPlayerAtAttackRadius()
    {
        // Calculate the position on the edge of the attack radius
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 desiredPosition = player.position - directionToPlayer * attackRadius;

        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    System.Collections.IEnumerator SwingHammer()
    {
        isSwinging = true;

        Debug.Log("Enemy is attacking");
        hammerTrigger.SetActive(true);

        // Rotate the enemy to face the player with an offset in the Y-axis
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)) * Quaternion.Euler(0, 15f, 0);

        float rotationSpeed = 30f;
        while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        float elapsedTime = 0f;
        while (elapsedTime < 45f / swingSpeed)
        {
            hammer.localRotation = Quaternion.RotateTowards(hammer.localRotation, targetRotation, swingSpeed * 10f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"is in hammertrigger: {isPlayerInHammerTrigger}");
        if (isPlayerInHammerTrigger)
        {
            Debug.Log("Player hit by hammer!");
            hammerTrigger.SetActive(false);
            currentState = EnemyState.IgnoringPlayer;
            ignorePlayerTimer = ignorePlayerTime;
            isAttacking = false;
            MoveToRandomPatrolPoint();
        }

        elapsedTime = 0f;
        while (elapsedTime < 45f / swingSpeed)
        {
            hammer.localRotation = Quaternion.RotateTowards(hammer.localRotation, initialRotation, swingSpeed * 10f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lastAttackTime = Time.time;
        isSwinging = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
