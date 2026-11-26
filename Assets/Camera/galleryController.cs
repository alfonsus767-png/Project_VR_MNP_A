using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class galleryController : MonoBehaviour
{
    [Header("Gallery UI")]
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private RawImage photoDisplay;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private GameObject emptyText;

    [Header("VR Input (Unity Input System")]
    [Tooltip("Assign an InputActionReference (e.g., XRI LeftHand/SecondaryButton or Menu).")]
    [SerializeField] private InputActionReference toggleGalleryAction;

    [Header("VR Gallery Placement")]
    [Tooltip("Places the gallery in front of the player's headset when opened.")]
    [SerializeField] private bool spawnInFrontOfHeadset = true;
    [SerializeField] private float spawnDistance = 1.2f;
    [SerializeField] private float verticalOffset = -0.1f;

    [Header("Gallery Settings")]
    [SerializeField] private int maxPhotos = 30; // Kept modest for Quest 3S shared RAM/VRAM

    // In-memory storage only (cleared automatically when the app closes)
    private readonly List<Texture2D> photos = new List<Texture2D>();
    private int currentIndex = 0;
    private bool isOpen = false;
    private Transform headTransform;

    private void Awake()
    {
        if (Camera.main != null)
            headTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (toggleGalleryAction != null && toggleGalleryAction.action != null)
        {
            toggleGalleryAction.action.Enable();
            toggleGalleryAction.action.performed += OnToggleGalleryPerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleGalleryAction != null && toggleGalleryAction.action != null)
        {
            toggleGalleryAction.action.performed -= OnToggleGalleryPerformed;
        }
    }

    private void Start()
    {
        CloseGallery();
    }

    private void OnToggleGalleryPerformed(InputAction.CallbackContext context)
    {
        ToggleGallery();
    }

    /// <summary>
    /// Receives a captured Texture2D from the iPad and stores it in memory.
    /// </summary>
    public void AddPhoto(Texture2D newPhoto)
    {
        if (newPhoto == null) return;

        if (photos.Count >= maxPhotos)
        {
            Destroy(photos[0]);
            photos.RemoveAt(0);
        }

        photos.Add(newPhoto);
        currentIndex = photos.Count - 1;

        if (isOpen)
        {
            RefreshDisplay();
        }
    }

    public void OpenGallery()
    {
        isOpen = true;

        if (spawnInFrontOfHeadset)
            PositionInFrontOfPlayer();

        if (galleryPanel != null)
            galleryPanel.SetActive(true);

        RefreshDisplay();
    }

    public void CloseGallery()
    {
        isOpen = false;

        if (galleryPanel != null)
            galleryPanel.SetActive(false);
    }

    public void ToggleGallery()
    {
        if (isOpen) CloseGallery();
        else OpenGallery();
    }

    public void NextPhoto()
    {
        if (photos.Count == 0) return;
        currentIndex = (currentIndex + 1) % photos.Count;
        RefreshDisplay();
    }

    public void PreviousPhoto()
    {
        if (photos.Count == 0) return;
        currentIndex = (currentIndex - 1 + photos.Count) % photos.Count;
        RefreshDisplay();
    }

    public void DeleteCurrentPhoto()
    {
        if (photos.Count == 0) return;

        Destroy(photos[currentIndex]);
        photos.RemoveAt(currentIndex);

        if (currentIndex >= photos.Count)
            currentIndex = Mathf.Max(0, photos.Count - 1);

        RefreshDisplay();
    }

    private void PositionInFrontOfPlayer()
    {
        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;

        if (headTransform == null) return;

        // Project headset forward direction onto the horizontal plane so the UI stays upright
        Vector3 flatForward = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = headTransform.forward;

        transform.position = headTransform.position + (flatForward * spawnDistance) + (Vector3.up * verticalOffset);
        transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    }

    private void RefreshDisplay()
    {
        bool hasPhotos = photos.Count > 0;

        if (emptyText != null)
            emptyText.SetActive(!hasPhotos);

        if (photoDisplay != null)
        {
            photoDisplay.enabled = hasPhotos;
            photoDisplay.texture = hasPhotos ? photos[currentIndex] : null;
        }

        if (counterText != null)
            counterText.text = hasPhotos ? $"{currentIndex + 1} / {photos.Count}" : "0 / 0";
    }

    private void OnDestroy()
    {
        for (int i = 0; i < photos.Count; i++)
        {
            if (photos[i] != null)
                Destroy(photos[i]);
        }
        photos.Clear();
    }
}