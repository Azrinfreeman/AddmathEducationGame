using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    // Decorates the existing Button: persistent actions and serialized references stay intact.
    [RequireComponent(typeof(Button))]
    public sealed class KidButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        public AdventureTheme theme;
        private Button button;
        private Vector3 rest;
        private Vector3 target;
        private bool selected;
        private bool pressed;
        private Outline focus;

        private void Awake()
        {
            button = GetComponent<Button>();
            rest = transform.localScale;
            target = rest;
            focus = GetComponent<Outline>();
            button.onClick.AddListener(PlayTap);
        }
        private void PlayTap() { UIFeedback.Tap(theme); }
        private void OnDestroy() { if (button) button.onClick.RemoveListener(PlayTap); }
        private void OnEnable() { if (button) Restore(); }
        private void OnDisable() { if (button) Restore(); }
        private void Restore() { pressed = false; target = rest; transform.localScale = rest; }
        private void Update()
        {
            if (!button.IsInteractable()) { Restore(); return; }
            if (AdventureTheme.ReduceMotion) { transform.localScale = rest; return; }
            float duration = theme ? theme.releaseSeconds : .2f;
            transform.localScale = Vector3.Lerp(transform.localScale, target,
                1f - Mathf.Exp(-Time.unscaledDeltaTime * 6f / duration));
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (!button.IsInteractable()) return;
            pressed = true;
            target = rest * (theme ? theme.pressedScale : .95f);
        }
        public void OnPointerUp(PointerEventData e) { pressed = false; target = rest * (selected ? 1.025f : 1f); }
        public void OnPointerExit(PointerEventData e) { pressed = false; target = rest; }
        public void OnSelect(BaseEventData e) { selected = true; if (!pressed) target = rest * 1.025f; if (focus) focus.effectDistance = new Vector2(5, -5); }
        public void OnDeselect(BaseEventData e) { selected = false; target = rest; if (focus) focus.effectDistance = new Vector2(2, -3); }
        public void OnSubmit(BaseEventData e) { if (button.IsInteractable()) transform.localScale = rest * .95f; }
    }
}
