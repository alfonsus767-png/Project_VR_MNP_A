
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalMovement : MonoBehaviour
{
    [Header("References")]
    public AnimalData animalData;
    public NavMeshAgent agent;
    public AnimalController animalController;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (animalData != null)
        {
            agent.speed = animalData.walkSpeed;
            agent.angularSpeed = animalData.rotationSpeed;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);

        if (animalController != null)
        {
            animalController.ChangeState(AnimalState.Walking);
        }
    }

    public void StopMovement()
    {
        agent.isStopped = true;

        if (animalController != null)
        {
            animalController.ChangeState(AnimalState.Idle);
        }
    }

    public bool HasReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance;
    }
}
