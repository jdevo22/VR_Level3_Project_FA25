using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class ExampleNoteCaller : MonoBehaviour
{
    [Tooltip("Reference to the NoteDisplayManager in the scene")]
    public UI_Script noteDisplay;

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System path
        var kb = Keyboard.current;
        if (kb == null) return;

        for (int i = 0; i <= 9; i++)
        {
            // Digit row keys (Digit0..Digit9)
            Key digitKey = (Key)((int)Key.Digit0 + i);
            var digitControl = kb[digitKey];
            bool pressed = digitControl != null && digitControl.wasPressedThisFrame;

            // Also accept Numpad keys as a convenience (Numpad0..Numpad9)
            Key numpadKey = (Key)((int)Key.Numpad0 + i);
            var numpadControl = kb[numpadKey];
            if (!pressed && numpadControl != null && numpadControl.wasPressedThisFrame)
                pressed = true;

            if (pressed)
            {
                int rightIndex = 27 + i; // map 0..9 to right-hand 27..36
                noteDisplay.ShowNoteSymbol(rightIndex);
                Debug.Log($"Showing right-hand symbol index: {rightIndex}");
            }
        }
#else
        // Legacy Input Manager path
        for (int i = 0; i <= 9; i++)
        {
            KeyCode kc = (KeyCode)((int)KeyCode.Alpha0 + i);
            if (Input.GetKeyDown(kc))
            {
                int rightIndex = 27 + i;
                noteDisplay.ShowNoteSymbol(rightIndex);
                Debug.Log($"Showing right-hand symbol index: {rightIndex}");
            }
        }
#endif
    }
}
