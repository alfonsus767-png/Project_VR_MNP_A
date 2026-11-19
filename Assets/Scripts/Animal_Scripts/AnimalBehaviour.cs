
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnimalBehavior : MonoBehaviour
{
    [Header("References")]
    public AnimalData animalData;
    public AnimalController animalController;
    public AnimalMovement animalMovement;

    [Header("Activity Points")]
    public Transform foodPoint;
    public Transform sleepPoint;
    public float exploreRadius = 5f;

    [Header("Decision Configuration")]
    public float minDecisionDelay = 2f;
    public float maxDecisionDelay = 5f;

    private bool isPerformingActivity = false;

    //KEMUNGKINAN AKTIVITAS KUCING BASED ON PERSONALITY//
    [Range(0, 100)]
    public float foodWeight = 30f;

    [Range(0, 100)]
    public float sleepWeight = 30f;

    [Range(0, 100)]
    public float idleWeight = 40f;

    public float exploreWeight = 30f;
    
    private void Start()
    {
        ApplyPersonality();
        StartCoroutine(AutomaticBehavior());
    }
    
    private IEnumerator AutomaticBehavior()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(minDecisionDelay, maxDecisionDelay)
            );

            ChooseActivity();

            yield return new WaitUntil(() => !isPerformingActivity);
        }
    }
    
    private void ChooseActivity()
    {
        if (isPerformingActivity) return;

        float totalWeight = foodWeight + sleepWeight + exploreWeight + idleWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < foodWeight)
        {
            StartCoroutine(PerformActivity(foodPoint, AnimalState.Eating));
        }
        else if (randomValue < foodWeight + sleepWeight)
        {
            StartCoroutine(PerformActivity(sleepPoint, AnimalState.Sleeping));
        }
        else if (randomValue < foodWeight + sleepWeight + exploreWeight)
        {
            Vector3 exploreDestination = FindExplorePoint();

            animalMovement.MoveTo(exploreDestination);
            isPerformingActivity = true;
            StartCoroutine(Explore());
        }
        else
        {
            animalMovement.StopMovement();
            animalController.ChangeState(AnimalState.Idle);
        }
    }

    private IEnumerator Explore()
    {
        // Tunggu sampai kucing tiba di tujuan
        yield return new WaitUntil(() => animalMovement.HasReachedDestination());

        // Berhenti berjalan
        animalMovement.StopMovement();

        // Diam sebentar setelah menjelajah
        yield return new WaitForSeconds(3f);

        // Kembali ke Idle
        animalController.ChangeState(AnimalState.Idle);

        isPerformingActivity = false;
    }

    private IEnumerator PerformActivity(Transform target, AnimalState activity)
    {
        if (target == null) yield break;

        isPerformingActivity = true;

        // Bergerak menuju tujuan
        animalMovement.MoveTo(target.position);

        // Tunggu sampai kucing tiba
        yield return new WaitUntil(() => animalMovement.HasReachedDestination());

        // Berhenti bergerak
        animalMovement.StopMovement();

        // Mulai aktivitas
        animalController.ChangeState(activity);

        // Lakukan aktivitas selama 5 detik
        yield return new WaitForSeconds(5f);

        // Kembali ke Idle
        animalController.ChangeState(AnimalState.Idle);

        isPerformingActivity = false;
    }

    //BUAT KUCING EXPLORE//
    private Vector3 FindExplorePoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * exploreRadius;
        randomDirection += transform.position;

        if (UnityEngine.AI.NavMesh.SamplePosition(
            randomDirection,
            out UnityEngine.AI.NavMeshHit hit,
            exploreRadius,
            UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    //BUAT NGETES//
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.bKey.wasPressedThisFrame)
        {
            ChooseActivity();
        }
    }

    private void ApplyPersonality()
    {
        switch (animalData.personality)
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
}
