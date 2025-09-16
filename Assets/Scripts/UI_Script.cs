using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour
{
    [Header("UI References")]
    public Image background;              // Main background for the note display
    public Image leftSymbolSlot;          // UI slot for left-hand symbols
    public Image rightSymbolSlot;         // UI slot for right-hand symbols

    [Header("Symbol Sets")]
    public Sprite[] leftHandSymbols;      // 27 sprites for left-hand (indexes 0–26)
    public Sprite[] rightHandSymbols;     // 27 sprites for right-hand (indexes 27–53)

    [Header("Display Options")]
    public bool showBackground = true;

    /// <summary>
    /// Call this with a value 0–53 to display the corresponding note symbol.
    /// Left side is 0–26, right side is 27–53.
    /// </summary>
    public void ShowNoteSymbol(int index)
    {
        if (index < 0 || index > 53)
        {
            Debug.LogWarning("NoteDisplayManager: Index out of range (0–53).");
            return;
        }

        // Enable background if needed
        if (background != null)
            background.enabled = showBackground;

        // Clear both slots first
        if (leftSymbolSlot != null) leftSymbolSlot.sprite = null;
        if (rightSymbolSlot != null) rightSymbolSlot.sprite = null;

        // Left-hand symbols
        if (index < 27)
        {
            if (leftSymbolSlot != null && index < leftHandSymbols.Length)
            {
                leftSymbolSlot.sprite = leftHandSymbols[index];
                leftSymbolSlot.enabled = true;
            }
        }
        // Right-hand symbols
        else
        {
            int rightIndex = index - 27;
            if (rightSymbolSlot != null && rightIndex < rightHandSymbols.Length)
            {
                rightSymbolSlot.sprite = rightHandSymbols[rightIndex];
                rightSymbolSlot.enabled = true;
            }
        }
    }

    /// <summary>
    /// Hides the UI symbols (use when no note is active).
    /// </summary>
    public void ClearSymbols()
    {
        if (leftSymbolSlot != null) leftSymbolSlot.enabled = false;
        if (rightSymbolSlot != null) rightSymbolSlot.enabled = false;
        if (background != null) background.enabled = false;
    }
}
