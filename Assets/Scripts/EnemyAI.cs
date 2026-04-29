using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ParanormalManager))]
public class EnemyAI : MonoBehaviour
{
    public float HearingRange = 10f;
    public float ViewAngle = 60f;
    public float huntSanity = 20f;
    public float huntTimer = 20f;
    public float huntBreakTime = 15f;
    public bool GameOver = false;

    public EnemyState currentState;

    bool hasLastSeen = false;
    Vector3 lastSeen;

    bool reacting = false;
    float reactionTimer;

    float huntResetTimer;
    float huntBreak;

    bool canSee;
    bool huntBreakActive;

    bool forcedHunt = false;
    float forcedHuntTimer = 0f;

    float flickerTimer = 0f;
    bool isVisible = false;

    // NEW SEARCH SYSTEM
    bool isSearching = false;
    float searchTimer = 0f;
    Vector3 searchCenter;

    NavMeshAgent agent;
    PlayerSystem player;
    ParanormalManager Manager;
    Renderer[] ghostRenderers;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSystem>();
        Manager = GetComponent<ParanormalManager>();

        ghostRenderers = GetComponentsInChildren<Renderer>();

        // Initially invisible
        SetGhostVisible(false);
    }

    void Update()
    {
        if (player == null || player.isDead || GameOver) return;

        UpdateState();
        HandleGhostVisual();

        switch (currentState)
        {
            case EnemyState.Idle:
                Patrol();
                break;

            case EnemyState.Stalking:
                ParanormalBehavior();
                break;

            case EnemyState.Hunting:
                Hunt();
                break;
        }
    }

    void SetGhostVisible(bool value)
    {
        isVisible = value;

        foreach (var r in ghostRenderers)
        {
            r.enabled = value;
        }
    }

    void HandleGhostVisual()
    {
        if (currentState != EnemyState.Hunting)
        {
            SetGhostVisible(false);
            return;
        }

        // flicker logic
        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0f)
        {
            SetGhostVisible(!isVisible);
            flickerTimer = Random.Range(0.05f, 0.3f);
        }
    }

    void UpdateState()
    {
        if (forcedHunt)
        {
            forcedHuntTimer -= Time.deltaTime;

            currentState = EnemyState.Hunting;

            if (forcedHuntTimer <= 0f)
                forcedHunt = false;

            return;
        }

        if (player.sanity < huntSanity && !huntBreakActive && !forcedHunt)
        {
            currentState = EnemyState.Hunting;
        }
        else if (player.sanity < 70f)
        {
            currentState = EnemyState.Stalking;
        }
        else
        {
            currentState = EnemyState.Idle;
        }

        if (huntBreakActive && player.sanity < huntSanity)
        {
            huntBreak += Time.deltaTime;

            if (huntBreak >= huntBreakTime)
            {
                huntBreakActive = false;
                huntBreak = 0f;
            }
        }
    }

    public void StartForcedHunt(float duration)
    {
        forcedHunt = true;
        forcedHuntTimer = duration;
    }

    void Hunt()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < 1.5f)
        {
            player.isDead = true;
            player.Death();
            return;
        }

        // ----------- VISION ----------- 
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

        // ----------- NOISE ----------- 
        float noise = player.noiseVal;
        bool heardPlayer = noise > 0.3f && distance < HearingRange * noise;

        // ----------- REACTION ----------- 
        if (heardPlayer || canSee)
        {
            if (!reacting)
            {
                reacting = true;
                reactionTimer = Random.Range(0.5f, 2f);
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
        else if (hasLastSeen)
        {
            agent.SetDestination(lastSeen);

            if (Vector3.Distance(transform.position, lastSeen) < 1f)
            {
                hasLastSeen = false;

                // START SEARCH
                isSearching = true;
                searchTimer = Random.Range(3f, 6f);
                searchCenter = lastSeen;
            }
        }
        else if (isSearching)
        {
            searchTimer -= Time.deltaTime;

            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 randomPoint = searchCenter + Random.insideUnitSphere * 3f;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hitNav, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hitNav.position);
                }
            }

            if (searchTimer <= 0f)
            {
                isSearching = false;
            }
        }
        else
        {
            Patrol();
        }

        // FIXED HUNT TIMER
        if (heardPlayer || canSee || hasLastSeen || isSearching)
        {
            huntResetTimer += Time.deltaTime;
        }

        if (huntResetTimer > huntTimer)
        {
            currentState = EnemyState.Idle;
            huntResetTimer = 0f;
            huntBreakActive = true;

            isSearching = false;
            hasLastSeen = false;
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

    void ParanormalBehavior()
    {
        if (Manager != null)
        {
            Manager.TryTrigger(player);
        }

        Patrol();
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, HearingRange);

        Handles.color = Color.green;

        Vector3 leftBoundary = Quaternion.Euler(0, -ViewAngle, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, ViewAngle, 0) * transform.forward;

        float viewDistance = HearingRange;

        Handles.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Handles.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
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