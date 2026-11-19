
using UnityEngine;

public class AnimalAnimation : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    public void UpdateAnimation(AnimalState state)
    {
        switch (state)
        {
            case AnimalState.Idle:
                animator.SetFloat("Vert", 0f);
                animator.SetFloat("State", 0f);
                break;

            case AnimalState.Walking:
                animator.SetFloat("Vert", 1f);
                animator.SetFloat("State", 0f);
                break;

            case AnimalState.Running:
                animator.SetFloat("Vert", 1f);
                animator.SetFloat("State", 1f);
                break;

            default:
                animator.SetFloat("Vert", 0f);
                animator.SetFloat("State", 0f);
                break;
        }
    }
}
