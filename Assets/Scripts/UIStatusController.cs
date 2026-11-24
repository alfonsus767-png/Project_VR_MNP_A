
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CatController catController;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text animalNameText;
    [SerializeField] private TMP_Text animationNameText;
    [SerializeField] private Slider durationSlider;

    private void Start()
    {
        if (catController == null)
        {
            Debug.LogError("CatController reference is missing!");
            return;
        }

        // Initialize UI
        UpdateAnimalName();
        UpdateAnimationName();

        if (durationSlider != null)
        {
            durationSlider.minValue = 0f;
            durationSlider.maxValue = 1f;
            durationSlider.value = 0f;

            // This is a display-only slider
            durationSlider.interactable = false;
        }
    }

    private void Update()
    {
        if (catController == null)
            return;

        UpdateAnimationName();
        UpdateDuration();
    }

    // ==========================================
    // ANIMAL NAME
    // ==========================================

    private void UpdateAnimalName()
    {
        if (animalNameText != null)
        {
            animalNameText.text = catController.animalName;
        }
    }

    // ==========================================
    // ANIMATION / STATE NAME
    // ==========================================

    private void UpdateAnimationName()
    {
        if (animationNameText != null)
        {
            animationNameText.text =
                catController.currentState.ToString();
        }
    }

    // ==========================================
    // ACTIVITY DURATION
    // ==========================================

    private void UpdateDuration()
    {
        if (durationSlider != null)
        {
            durationSlider.value =
                catController.ActivityProgress;
        }
    }
}
