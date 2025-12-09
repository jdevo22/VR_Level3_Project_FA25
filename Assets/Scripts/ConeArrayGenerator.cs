using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Generates 24 Labanotation "cone" zones.
/// Calibration: The user holds the controller at their sternum and clicks.
/// The array spawns at that exact location.
/// </summary>
public class LabanotationConeGenerator : MonoBehaviour
{
    public GameManager gameManager;
    
    [Header("Zone Configuration")]
    [Tooltip("Prefab for the cone zone. Must have its point at 0,0,0 and 'point up' its Y-axis.")]
    public GameObject zonePrefab;
    [Tooltip("A parent transform to keep the generated zones organized.")]
    public Transform zoneParent;

    [Header("Calibration Settings")]
    [Tooltip("The physical controller transform (Right or Left Hand) used to set the position.")]
    public Transform calibrationController;

    [Tooltip("How long the cones should be (in meters), since we aren't auto-measuring arms anymore.")]
    public float defaultConeLength = 1.0f;

    [Header("Debug Settings")]
    [Tooltip("Sets Sternum position to (0,1,0) for testing without a headset.")]
    public bool debugMode = false;

    // --- Private Fields ---
    private List<Hitbox> _generatedZones = new List<Hitbox>();
    private int hitboxId = 0;

    // --- Constant Data Arrays ---
    private readonly string[] heightNames = { "Low", "Middle", "High" };
    private readonly float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private readonly string[] directionalNames = {
        "Forward", "Forward Right Diagonal", "Right", "Back Right Diagonal",
        "Backward", "Back Left Diagonal", "Left", "Forward Left Diagonal"
    };

    /// <summary>
    /// This method is called by the PlayerInput component when the 'Calibrate' action is performed.
    /// </summary>
    /// 

    private void Awake()
    {
        Debug.Log("test");
        if (debugMode)
        {
            Debug.Log("Debug mode active - setting sternum position to 0, 1, 0...");
            CalibrateAndGenerateZones();
        }
    }
    public void OnCalibrate(InputAction.CallbackContext context)
    {
        // We check for 'performed' to make sure it only runs once per press
        if (context.performed)
        {
            Debug.Log("'Calibrate' action performed! Setting Sternum position...");
            CalibrateAndGenerateZones();
        }
    }

    /// <summary>
    /// Moves the array to the controller's current position and generates the cones.
    /// </summary>
    [ContextMenu("Calibrate and Generate Zones")]
    public void CalibrateAndGenerateZones()
    {
        if (calibrationController == null)
        {
            Debug.LogError("Calibration Controller must be assigned in the Inspector!");
            return;
        }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // --- 1. SET STERNUM POSITION ---
        // Move the entire zone parent to exactly where the controller is right now.

        parent.position = calibrationController.position;

        // Optional: If you want the array to rotate to face the same way the player is facing,
        // you can uncomment the line below. Otherwise, it stays aligned with the world/room.
        
        parent.rotation = Quaternion.Euler(0, calibrationController.eulerAngles.y, 0);

        Debug.Log($"Sternum set to: {parent.position}");

        // --- 2. GENERATE ZONES ---
        // We use the fixed 'defaultConeLength' now.
        GenerateZonesInternal(defaultConeLength);
    }

    /// <summary>
    /// Generates the 24 cone zones using the fixed length.
    /// </summary>
    private void GenerateZonesInternal(float coneLength)
    {
        ClearExistingZones();

        //if (zonePrefab == null) { return; }
        //if (zonePrefab.GetComponent<Hitbox>() == null) { return; }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // Loop through the three height levels
        for (int i = 0; i < 3; i++)
        {
            // i=0 (Low) -> Y = -1, i=1 (Middle) -> Y = 0, i=2 (High) -> Y = 1
            float yDir = (i - 1);
            string currentHeightName = heightNames[i];

            // Loop through the 8 horizontal directions
            for (int j = 0; j < 8; j++)
            {
                float angleRad = angles[j] * Mathf.Deg2Rad;
                float xDir = Mathf.Sin(angleRad);
                float zDir = Mathf.Cos(angleRad);

                // Create vector and normalize
                Vector3 direction = new Vector3(xDir, yDir, zDir).normalized;

                // Calculate rotation to point the cone
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

                string zoneName = $"{currentHeightName} - {directionalNames[j]}";
                InstantiateZone(zoneName, parent, rotation, coneLength);
            }
        }
        Debug.Log($"Successfully generated {_generatedZones.Count} Labanotation Zones.");
        UpdateGameManager();
    }

    // (ClearExistingZones is unchanged)
    public void ClearExistingZones()
    {
        for (int i = _generatedZones.Count - 1; i >= 0; i--)
        {
            if (_generatedZones[i] != null)
            {
                if (Application.isEditor && !Application.isPlaying) DestroyImmediate(_generatedZones[i].gameObject);
                else Destroy(_generatedZones[i].gameObject);
            }
        }
        int count = _generatedZones.Count;
        _generatedZones.Clear();
    }

    // (InstantiateZone is unchanged)
    private void InstantiateZone(string zoneName, Transform parent, Quaternion rotation, float length)
    {
        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        GameObject newZoneObj = Instantiate(zonePrefab, parent);
        newZoneObj.transform.localPosition = Vector3.zero;
        newZoneObj.transform.localRotation = rotation;
        newZoneObj.name = zoneName;
        newZoneObj.transform.localScale = new Vector3(1, length, 1);

        Hitbox hitbox = newZoneObj.GetComponentInChildren<Hitbox>();
        if (hitbox != null) {
            _generatedZones.Add(hitbox);
            hitbox.id = hitboxId;
            ++hitboxId;
            }
    }

    public void UpdateGameManager()
    {
        for (int i = 0; i < _generatedZones.Count; ++i)
        {
            gameManager.hitboxes[i] = _generatedZones[i];
        }
    }
}
