using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FunMath.UI
{
    /// <summary>Owns a reversible pause; restores the previous clock on every exit.</summary>
    public sealed class GamePause : MonoBehaviour
    {
        public GameObject pausePanel;
        public UnityEngine.UI.Button pauseControl;
        public string menuScene = "GameMenu";
        private float previousTimeScale;
        private GameObject previousFocus;
        private bool paused;
        private bool leaving;
        private bool ready;
        public bool IsPaused => paused;

        private System.Collections.IEnumerator Start()
        {
            // Intro panels are enabled by the legacy Start methods on the first frame.
            yield return null;
            ready = true;
        }

        private void Update()
        {
            var owner = ModalInputOwner.ForScene(gameObject.scene);
            if (owner && owner.HasModal) return;
            bool requested = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            requested |= Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            if (!requested) return;
            if (paused) Resume(); else Pause();
        }

        public void Pause()
        {
            if (!ready || paused || leaving || !pausePanel || GameModalFocus.VisibleCount > 0) return;
            previousTimeScale = Time.timeScale;
            previousFocus = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            // A held rocket direction must not survive opening the menu.
            if (ButtonController.instance)
            {
                ButtonController.instance.isLeft = false;
                ButtonController.instance.isRight = false;
            }
            paused = true;
            Time.timeScale = 0;
            var scope = pausePanel.GetComponentInChildren<ModalInputScope>(true);
            if (scope) { scope.allowBack = true; scope.closeRequested = Resume; }
            pausePanel.SetActive(true);
        }

        public void Resume()
        {
            if (!paused) return;
            paused = false;
            Time.timeScale = previousTimeScale;
            if (pausePanel) pausePanel.SetActive(false);
            if (EventSystem.current && previousFocus && previousFocus.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousFocus);
            else if (EventSystem.current && pauseControl && pauseControl.gameObject.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(pauseControl.gameObject);
        }

        public void Replay() { Leave(SceneManager.GetActiveScene().name); }
        public void BackToGames() { Leave(menuScene); }

        private void Leave(string sceneName)
        {
            if (leaving) return;
            leaving = true;
            Resume();
            SceneManager.LoadScene(sceneName);
        }

        private void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        private void OnApplicationPause(bool appPaused) { if (appPaused) Pause(); }
        private void OnDisable() { Resume(); }
    }
}
