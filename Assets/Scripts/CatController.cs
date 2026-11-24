
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public enum AnimalState
{
    Idle,
    Walking,
    Running,
    Eating,
    Sleeping
}

public enum AnimalPersonality
{
    Rakus,
    Pemalas,
    Energetic
}

[RequireComponent(typeof(NavMeshAgent))]
public class CatController : MonoBehaviour
{
    // ==========================================
    // CAT INFORMATION
    // ==========================================

    [Header("Cat Information")]
    public string animalName;
    public int animalAge;
    public string animalBreed;

    [TextArea(2, 4)]
    public string animalDescription;

    public Sprite animalIcon;

    [Header("Activity Progress")]
    [Range(0f, 1f)]
    public float activityProgress = 0f;

    public float ActivityProgress => activityProgress;

    [Header("Animation Duration")]
    public float sliderFillDuration = 5f;

    private float stateElapsedTime = 0f;

    // ==========================================
    // PERSONALITY
    // ==========================================

    [Header("Personality")]
    public AnimalPersonality personality;

    [Range(0, 100)]
    public float foodWeight = 30f;

    [Range(0, 100)]
    public float sleepWeight = 30f;

    [Range(0, 100)]
    public float idleWeight = 40f;

    [Range(0, 100)]
    public float exploreWeight = 30f;

    // ==========================================
    // MOVEMENT
    // ==========================================

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float rotationSpeed = 180f;

    [Header("Idle Configuration")]
    public float minIdleTime = 3f;
    public float maxIdleTime = 8f;

    [Header("Activity Points")]
    public Transform foodPoint;
    public Transform sleepPoint;
    public float exploreRadius = 5f;

    // ==========================================
    // BEHAVIOR
    // ==========================================

    [Header("Behavior")]
    public float minDecisionDelay = 2f;
    public float maxDecisionDelay = 5f;
    public float activityDuration = 5f;
    public float exploreDuration = 3f;

    // ==========================================
    // REFERENCES
    // ==========================================

    [Header("References")]
    public Animator animator;
    public TextMeshPro stateText;

    private NavMeshAgent agent;

    // ==========================================
    // CURRENT STATE
    // ==========================================

    [Header("Current State")]
    public AnimalState currentState = AnimalState.Idle;

    private bool isPerformingActivity;

    // ==========================================
    // ANIMATION SETTINGS
    // ==========================================

    [Header("Animator Parameters")]
    public string verticalParameter = "Vert";
    public string stateParameter = "State";

    // ==========================================
    // DEBUG SETTINGS
    // ==========================================

    [Header("Debug")]
    public bool enableDebugInput = true;

    private Coroutine behaviorRoutine;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Configure NavMeshAgent
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;

        // Apply personality settings
        ApplyPersonality();

        // Initialize state
        ChangeState(AnimalState.Idle);

        // Start autonomous behavior
        behaviorRoutine = StartCoroutine(BehaviorLoop());
    }

    // ==========================================
    // UPDATE
    // ==========================================

    private void Update()
    {
        UpdateStateDuration();

        if (!enableDebugInput || Keyboard.current == null)
            return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TestNextState();
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ChooseActivity();
        }
    }

    private void UpdateStateDuration()
    {
        stateElapsedTime += Time.deltaTime;

        if (sliderFillDuration <= 0f)
        {
            activityProgress = 1f;
            return;
        }

        activityProgress = Mathf.Clamp01(
            stateElapsedTime / sliderFillDuration
        );
    }

    // ==========================================
    // STATE MANAGEMENT
    // ==========================================

    public void ChangeState(AnimalState newState)
    {
        // Only reset when the state actually changes
        if (currentState == newState)
            return;

        currentState = newState;

        // Reset animation duration
        stateElapsedTime = 0f;
        activityProgress = 0f;

        UpdateDebugUI();
        UpdateAnimation();

        Debug.Log(animalName + " State: " + currentState);
    }

    private void UpdateDebugUI()
    {
        if (stateText != null)
        {
            stateText.text = currentState.ToString().ToUpper();
        }
    }

    // ==========================================
    // ANIMATION
    // ==========================================

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        float vertical = 0f;
        float state = 0f;

        switch (currentState)
        {
            case AnimalState.Walking:
                vertical = 1f;
                break;

            case AnimalState.Running:
                vertical = 1f;
                state = 1f;
                break;
        }

        animator.SetFloat(verticalParameter, vertical);
        animator.SetFloat(stateParameter, state);
    }

    // ==========================================
    // MOVEMENT
    // ==========================================

    private bool MoveTo(Vector3 destination)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning(animalName + " is not on a NavMesh!");
            return false;
        }

        agent.speed = walkSpeed;
        agent.isStopped = false;

        bool success = agent.SetDestination(destination);

        if (success)
        {
            ChangeState(AnimalState.Walking);
        }

        return success;
    }

    private void StopMovement()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private bool HasReachedDestination()
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        return !agent.pathPending &&
               agent.pathStatus != NavMeshPathStatus.PathInvalid &&
               agent.remainingDistance <=
               agent.stoppingDistance + 0.05f;
    }

    // ==========================================
    // AUTONOMOUS BEHAVIOR
    // ==========================================

    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            float delay = Random.Range(
                minDecisionDelay,
                maxDecisionDelay
            );

            yield return new WaitForSeconds(delay);

            if (!isPerformingActivity)
            {
                ChooseActivity();
            }

            yield return new WaitUntil(
                () => !isPerformingActivity
            );
        }
    }

    private void ChooseActivity()
    {
        if (isPerformingActivity)
            return;

        float totalWeight =
            foodWeight +
            sleepWeight +
            exploreWeight +
            idleWeight;

        if (totalWeight <= 0f)
        {
            StopMovement();
            ChangeState(AnimalState.Idle);
            return;
        }

        float randomValue = Random.Range(0f, totalWeight);

        // EAT
        if (randomValue < foodWeight)
        {
            if (foodPoint != null)
            {
                StartCoroutine(
                    PerformActivity(
                        foodPoint.position,
                        AnimalState.Eating,
                        activityDuration
                    )
                );
            }
        }

        // SLEEP
        else if (randomValue < foodWeight + sleepWeight)
        {
            if (sleepPoint != null)
            {
                StartCoroutine(
                    PerformActivity(
                        sleepPoint.position,
                        AnimalState.Sleeping,
                        activityDuration
                    )
                );
            }
        }

        // EXPLORE
        else if (
            randomValue <
            foodWeight + sleepWeight + exploreWeight
        )
        {
            Vector3 destination = FindExplorePoint();

            StartCoroutine(
                PerformActivity(
                    destination,
                    AnimalState.Idle,
                    exploreDuration
                )
            );
        }

        // IDLE
        else
        {
            StopMovement();
            ChangeState(AnimalState.Idle);
        }
    }

    // ==========================================
    // PERFORM ACTIVITY
    // ==========================================


    private IEnumerator PerformActivity(
    Vector3 destination,
    AnimalState activity,
    float duration
)
    {
        isPerformingActivity = true;

        if (!MoveTo(destination))
        {
            FinishActivity();
            yield break;
        }

        while (agent.pathPending)
        {
            yield return null;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            FinishActivity();
            yield break;
        }

        while (!HasReachedDestination())
        {
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                FinishActivity();
                yield break;
            }

            yield return null;
        }

        StopMovement();

        ChangeState(activity);

        // Wait for the activity duration.
        // The UpdateStateDuration() method handles the slider.
        yield return new WaitForSeconds(duration);

        FinishActivity();
    }


    private void FinishActivity()
    {
        StopMovement();

        // Reset activity progress
        activityProgress = 0f;

        // Return to Idle
        ChangeState(AnimalState.Idle);

        isPerformingActivity = false;
    }

    // ==========================================
    // EXPLORE
    // ==========================================

    private Vector3 FindExplorePoint()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * exploreRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            exploreRadius,
            NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    // ==========================================
    // PERSONALITY
    // ==========================================

    private void ApplyPersonality()
    {
        switch (personality)
        {
            case AnimalPersonality.Rakus:

                foodWeight = 60f;
                sleepWeight = 10f;
                idleWeight = 30f;
                exploreWeight = 20f;

                break;

            case AnimalPersonality.Pemalas:

                foodWeight = 15f;
                sleepWeight = 60f;
                idleWeight = 25f;
                exploreWeight = 10f;

                break;

            case AnimalPersonality.Energetic:

                foodWeight = 30f;
                sleepWeight = 10f;
                idleWeight = 60f;
                exploreWeight = 50f;

                break;
        }
    }

    // ==========================================
    // DEBUG TESTING
    // ==========================================

    private void TestNextState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                ChangeState(AnimalState.Walking);
                break;

            case AnimalState.Walking:
                ChangeState(AnimalState.Running);
                break;

            case AnimalState.Running:
                ChangeState(AnimalState.Idle);
                break;

            default:
                ChangeState(AnimalState.Idle);
                break;
        }
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDisable()
    {
        if (behaviorRoutine != null)
        {
            StopCoroutine(behaviorRoutine);
            behaviorRoutine = null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        isPerformingActivity = false;
    }
}
