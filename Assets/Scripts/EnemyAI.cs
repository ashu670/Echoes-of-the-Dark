using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public float HearingRange = 10f;
    public float ViewAngle = 60f;
    public float huntSanity = 20f;

    public EnemyState currentState;

    // Last seen system
    bool hasLastSeen = false;
    Vector3 lastSeen;

    // Reaction delay system (for horror feel)
    bool reacting = false;
    float reactionTimer;

    bool canSee;

    NavMeshAgent agent;
    PlayerSystem player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSystem>();
    }

    void Update()
    {
        if (player == null || player.isDead) return;

        UpdateState();
        Debug.Log("Enemy State: " + currentState);

        switch (currentState)
        {
            case EnemyState.Idle:
                Patrol();
                break;

            case EnemyState.Stalking:
                //ParanormalBehavior();
                break;

            case EnemyState.Hunting:
                Hunt();
                break;
        }
    }

    void UpdateState()
    {
        if (player.sanity < huntSanity)
        {
            currentState = EnemyState.Hunting;
        }
        else if (player.sanity < 60f)
        {
            currentState = EnemyState.Stalking;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    void Hunt()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        Debug.Log(distance);
        if (distance < 1.5f)
        {
            player.isDead = true;
            player.Death();
            return;
        }

        // ----------- VISION CHECK -----------
        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, distance))
        {
            canSee = Vector3.Angle(transform.forward, dirToPlayer) < ViewAngle &&
                     hit.transform.CompareTag("Player");
        }
        else
        {
            canSee = false;
        }

        // ----------- NOISE CHECK -----------
        float noise = player.noiseVal;

        bool heardPlayer = noise > 0.3f && distance < HearingRange * noise;

        // ----------- REACTION LOGIC -----------
        if (heardPlayer || canSee)
        {
            if (!reacting)
            {
                reacting = true;
                reactionTimer = Random.Range(0.5f, 2f); // delay for horror effect
            }
        }

        if (reacting)
        {
            reactionTimer -= Time.deltaTime;

            if (reactionTimer <= 0)
            {
                agent.SetDestination(player.transform.position);
                lastSeen = player.transform.position;
                hasLastSeen = true;
                reacting = false;
            }
        }

        // ----------- SEARCH LAST POSITION -----------
        else if (hasLastSeen)
        {
            agent.SetDestination(lastSeen);

            if (Vector3.Distance(transform.position, lastSeen) < 1f)
            {
                hasLastSeen = false;
            }
        }

        // ----------- PATROL (IDLE BEHAVIOR) -----------
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    //void ParanormalBehavior()
    //{
    //    // EnemyAI decides WHEN to trigger events; ParanormalManager decides WHAT event occurs
    //    if (paranormalManager != null)
    //    {
    //        paranormalManager.TryTriggerEvent(player);
    //    }

    //    // Keep light patrol movement so the enemy doesn't feel static
    //    Patrol();
    //}
}

public enum EnemyState
{
    Idle,
    Stalking,
    Hunting
}

public enum EventType
{
    Footstep,
    Appearance,
    Whisper
}