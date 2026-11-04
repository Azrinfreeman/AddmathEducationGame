using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>
    /// Back button for the AR scenes. It presses the scene's original exit button, so the same confirmation
    /// dialog and scene unload run as before. On Android the system Back key does the same.
    /// </summary>
    public sealed class ARBackButton : MonoBehaviour
    {
        public Button legacyExit;
        public GameObject confirmDialog;
        [Tooltip("Overlay that is hidden while the exit dialog is open.")]
        public CanvasGroup overlay;

        public void Press()
        {
            if (!legacyExit || (confirmDialog && confirmDialog.activeInHierarchy)) return;
            legacyExit.onClick.Invoke();
        }

        private void Update()
        {
            if (overlay)
            {
                bool dialogOpen = confirmDialog && confirmDialog.activeInHierarchy;
                overlay.alpha = dialogOpen ? 0 : 1;
                overlay.blocksRaycasts = !dialogOpen;
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Press();
        }
    }
}
