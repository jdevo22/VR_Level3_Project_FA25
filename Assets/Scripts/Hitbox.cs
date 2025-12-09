using UnityEngine;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Hitbox : MonoBehaviour
{
    [Header("Feedback Colors")]
    public Color idleColor = Color.white;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    [Header("Modes")]
    public bool standaloneTestMode = false;  // toggle in Inspector when no GameManager yet

    private MeshRenderer meshRenderer;
    public bool isActiveTarget = false;

    [Header("Box Attributes")]
    public int id = -1;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetColor(idleColor);

        if (standaloneTestMode)
        {
            isActiveTarget = true; // treat as always correct in test mode
        }
    }

    private void Start()
    {
        if (id == -1)
        {
            Debug.LogWarning($"{gameObject.name} lacks an ID and will not function.");
        }
    }

    /// <summary>
    /// Called by the GameManager to activate/deactivate this hitbox for the round.
    /// Ignored if standaloneTestMode is enabled.
    /// </summary>
    public void SetActiveTarget(bool active)
    {
        if (standaloneTestMode) return;

        isActiveTarget = active;
        SetColor(idleColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision is hit");
        //Debug.Log("PlayerArm tag");

        if (isActiveTarget && other.tag == "PlayerArm")
        {
            Debug.Log("Target hit.");
            SetColor(correctColor);
        }
        else if (!isActiveTarget && other.tag == "PlayerArm")
        {
            SetColor(incorrectColor);
            Debug.Log("Incorrect target hit.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
         SetColor(idleColor);
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = color;
    }

}

