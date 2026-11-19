
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalController : MonoBehaviour
{
    [Header("References")]
    public AnimalData animalData;
    public AnimalDebug animalDebug;
    public AnimalAnimation animalAnimation;

    [Header("Current State")]
    public AnimalState currentState;

    private void Start()
    {
        ChangeState(AnimalState.Idle);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("TOMBOL T TERDETEKSI!");
            TestNextState();
        }
    }

    public void ChangeState(AnimalState newState)
    {
        currentState = newState;

        // Update tulisan debug
        if (animalDebug != null)
        {
            animalDebug.UpdateState(currentState);
        }

        // Update animasi
        if (animalAnimation != null)
        {
            animalAnimation.UpdateAnimation(currentState);
        }

        Debug.Log(animalData.animalName + " State: " + currentState);
    }

    public void TestNextState()
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
}
