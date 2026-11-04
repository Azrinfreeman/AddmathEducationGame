using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>Fades a visible game dialog and confines selection to its actions.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameModalFocus : MonoBehaviour
    {
        public Button firstAction;
        public static int VisibleCount { get; private set; }
        private GameObject previousFocus;
        private CanvasGroup group;
        private float elapsed;
        private ModalInputOwner owner;
        private ModalInputScope scope;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCount() { VisibleCount = 0; }

        private void OnEnable()
        {
            VisibleCount++;
            elapsed = 0;
            group = GetComponent<CanvasGroup>();
            group.alpha = 0;
            previousFocus = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            owner = ModalInputOwner.ForScene(gameObject.scene);
            if (owner)
            {
                scope = GetComponent<ModalInputScope>();
                if (scope) { scope.firstControl = firstAction; scope.Bind(owner); }
            }
            if (!owner) SelectFirst();
        }

        private void SelectFirst()
        {
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(firstAction && firstAction.IsInteractable() ? firstAction.gameObject : null);
        }

        private void LateUpdate()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / .2f);
            group.alpha = 1 - Mathf.Pow(1 - t, 3);
            if (owner) return;
            if (!EventSystem.current) return;
            var selected = EventSystem.current.currentSelectedGameObject;
            if (!selected || !selected.transform.IsChildOf(transform)) SelectFirst();
        }

        private void OnDisable()
        {
            VisibleCount = Mathf.Max(0, VisibleCount - 1);
            if (!owner && EventSystem.current && previousFocus && previousFocus.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousFocus);
        }
    }
}
