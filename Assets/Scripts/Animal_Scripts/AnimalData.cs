
using UnityEngine;

public enum AnimalPersonality
{
    Rakus,
    Pemalas,
    Energetic
}

public class AnimalData : MonoBehaviour
{
    [Header("Cat Information")]
    public string animalName;
    public int animalAge;
    public string animalBreed;

    [TextArea(2, 4)]
    public string animalDescription;

    public Sprite animalIcon;

    [Header("Personality")]
    public AnimalPersonality personality;

    [Header("Movement Configuration")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float rotationSpeed = 180f;

    [Header("Idle Configuration")]
    public float minIdleTime = 3f;
    public float maxIdleTime = 8f;
}
