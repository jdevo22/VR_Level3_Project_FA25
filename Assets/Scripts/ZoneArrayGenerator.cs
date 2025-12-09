using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Generates 27 Labanotation zones, positioned at chest-height and scaled to the player.
/// Calibration is triggered by a controller button press at runtime.
/// </summary>
public class LabanotationZoneGenerator : MonoBehaviour
{
    public GameManager gameManager;

    [Header("Zone Configuration")]
    [Tooltip("Prefab for the zone marker. Must have a 'Hitbox' component.")]
    public GameObject zonePrefab;
    [Tooltip("A parent transform to keep the generated zones organized.")]
    public Transform zoneParent;

    [Header("Dynamic Calibration")]
    [Tooltip("Reference to the player's head (e.g., the Main Camera in an XR rig).")]
    public Transform playerHead;
    [Tooltip("Reference to one of the player's hands (e.g., a controller transform).")]
    public Transform playerHand;
    [Range(0.5f, 1.2f)]
    public float armLengthMultiplier = 0.9f;
    [Range(0.2f, 10f)]
    public float heightSpacingMultiplier = 10f;

    [Header("Runtime Calibration")]
    [Tooltip("Positions the center of the zones at this height relative to the head (0.75 = chest).")]
    [Range(0.5f, 1.0f)]
    public float verticalOffsetMultiplier = 0.5f;
    [Tooltip("The name of the button/axis in the Input Manager to trigger calibration.")]
    public string calibrationButtonName = "Fire1"; // Common default for a trigger press

    // --- Private Fields ---
    private List<Hitbox> _generatedZones = new List<Hitbox>();
    private bool _isCalibrated = false;
    private int hitboxId = 0;


    // --- Constant Data Arrays ---
    // (Arrays for heightNames, angles, directionalNames are unchanged)
    private readonly string[] heightNames = { "Low", "Middle", "High" };
    private readonly float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private readonly string[] directionalNames = {
        "Forward", "Forward Right Diagonal", "Right", "Back Right Diagonal",
        "Backward", "Back Left Diagonal", "Left", "Forward Left Diagonal"
    };

    /// <summary>
    /// Listens for the calibration button press every frame.
    /// </summary>
    void Update()
    {
        //Check if the specified controller button was pressed down this frame
        //if (Input.GetButtonDown(calibrationButtonName))
        //{
        //    Debug.Log($"'{calibrationButtonName}' pressed! Calibrating and generating zones...");
        //    CalibrateAndGenerateZones();
        //}
    }

    /// <summary>
    /// This method is called by the PlayerInput component when the 'Calibrate' action is performed.
    /// </summary>
    public void OnCalibrate(InputAction.CallbackContext context)
    {
        // We check for 'performed' to make sure it only runs once per press
        if (context.performed)
        {
            Debug.Log("'Calibrate' action performed! Calibrating and generating zones...");
            CalibrateAndGenerateZones();
        }
    }

    /// <summary>
    /// Calibrates dimensions, sets the vertical position, and generates the zones.
    /// </summary>
    [ContextMenu("Calibrate and Generate Zones")]
    public void CalibrateAndGenerateZones()
    {
      
        if (playerHead == null || playerHand == null)
        {
            Debug.LogError("Player Head and Hand transforms must be assigned!");
            return;
        }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // --- 1. SET VERTICAL POSITION ---
        // Calculate the ideal vertical center for the zones (e.g., chest height).
        float verticalCenter = playerHead.position.y * verticalOffsetMultiplier;
        // Position the parent of the zones at this height. We only move the Y axis.
        parent.position = new Vector3(parent.position.x, verticalCenter, parent.position.z);
        Debug.Log($"Array center set to Y: {verticalCenter:F2}");

        // --- 2. CALIBRATE SIZE (VERTICAL INCLUDED) ---

        // First, find the arm length radius
        Vector3 handPosition = playerHand.localPosition;
        Vector2 handHorizontalPosition = new Vector2(handPosition.x, handPosition.z);
        float calibratedRadius = handHorizontalPosition.magnitude * armLengthMultiplier;

        // *** NEW ***
        // Now, base the height spacing on that same radius
        float calibratedHeightSpacing = calibratedRadius * heightSpacingMultiplier;

        // (The old line was: float calibratedHeightSpacing = playerHead.localPosition.y * heightSpacingMultiplier;)

        _isCalibrated = true;
        Debug.Log($"Calibration complete: Radius={calibratedRadius:F2}, HeightSpacing={calibratedHeightSpacing:F2}");

        // --- 3. GENERATE ZONES ---
        GenerateZonesInternal(calibratedRadius, calibratedHeightSpacing);
    }

    /// <summary>
    /// Generates the array of zones using the calibrated dimensions.
    /// </summary>
    private void GenerateZonesInternal(float radius, float spacing)
    {
        ClearExistingZones();

        if (zonePrefab == null) { /* Error checks are unchanged */ return; }
        if (zonePrefab.GetComponent<Hitbox>() == null) { /* Error checks are unchanged */ return; }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // The rest of the generation logic is the same, but uses the passed-in radius/spacing
        for (int i = 0; i < 3; i++)
        {
            float yPos = (i - 1) * spacing;
            // The rest of this loop is unchanged...
            string currentHeightName = heightNames[i];
            for (int j = 0; j < 8; j++)
            {
                float angleRad = angles[j] * Mathf.Deg2Rad;
                float xPos = radius * Mathf.Sin(angleRad);
                float zPos = radius * Mathf.Cos(angleRad);
                Vector3 position = new Vector3(xPos, yPos, zPos);
                string zoneName = $"{currentHeightName} - {directionalNames[j]}";
                InstantiateZone(position, zoneName, parent);
            }
            //Vector3 centerPosition = new Vector3(0f, yPos, 0f);
            //string centerZoneName = $"{currentHeightName} - Place" + (i == 1 ? " (Center)" : "");
            //InstantiateZone(centerPosition, centerZoneName, parent);
        }

        Debug.Log($"Successfully generated {_generatedZones.Count} Labanotation Zones.");
        UpdateGameManager();
    }

    /// <summary>
    /// Destroys all previously generated zone objects.
    /// </summary>
    [ContextMenu("Clear Zones")]
    public void ClearExistingZones()
    {
        // Iterate backwards because we are removing items from the list
        for (int i = _generatedZones.Count - 1; i >= 0; i--)
        {
            if (_generatedZones[i] != null)
            {
                // Use DestroyImmediate in Edit mode, Destroy in Play mode
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(_generatedZones[i].gameObject);
                }
                else
                {
                    Destroy(_generatedZones[i].gameObject);
                }
            }
        }

        int count = _generatedZones.Count;
        _generatedZones.Clear();

        if (count > 0)
        {
            Debug.Log($"Cleared {count} existing zone objects.");
        }
    }

    /// <summary>
    /// Helper function to instantiate, name, and register a zone object.
    /// </summary>
    private void InstantiateZone(Vector3 localPosition, string zoneName, Transform parent)
    {
        GameObject newZoneObj = Instantiate(zonePrefab, parent);
        newZoneObj.transform.localPosition = localPosition;
        newZoneObj.transform.localRotation = Quaternion.identity;
        newZoneObj.name = zoneName;

        // Get the hitbox component and add it to our list for management
        Hitbox hitbox = newZoneObj.GetComponentInChildren<Hitbox>();
        if (hitbox != null && hitbox.id == -1)
        {
            // Optional: Initialize the hitbox with its name or other data
            // hitbox.Initialize(zoneName); 
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