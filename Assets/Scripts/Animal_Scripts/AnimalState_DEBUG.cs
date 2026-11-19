
using UnityEngine;
using TMPro;

public class AnimalDebug : MonoBehaviour
{
    [Header("Debug UI")]
    public TextMeshPro stateText;

    public void UpdateState(AnimalState state)
    {
        if (stateText != null)
        {
            stateText.text = state.ToString().ToUpper();
        }
    }
}
