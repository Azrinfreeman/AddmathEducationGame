using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FunMath.UI
{
    /// <summary>Bridges each actual Button to the existing shared direction owner.</summary>
    public sealed class RocketDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ButtonController directions;
        public bool right;
        private bool pointerHeld;
        private bool wasHeld;

        public void OnPointerDown(PointerEventData data) { pointerHeld = true; SetHeld(true); }
        public void OnPointerUp(PointerEventData data) { pointerHeld = false; SetHeld(false); }

        private void Update()
        {
            bool selected = EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject;
            bool keyHeld = Keyboard.current != null && (Keyboard.current.spaceKey.isPressed || Keyboard.current.enterKey.isPressed);
            bool padHeld = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
            bool held = Time.timeScale > 0 && (pointerHeld || (selected && (keyHeld || padHeld)));
            if (held != wasHeld) SetHeld(held);
            if (Time.timeScale == 0) pointerHeld = false;
        }

        private void SetHeld(bool held)
        {
            wasHeld = held;
            if (!directions) return;
            if (right) directions.isRight = held; else directions.isLeft = held;
        }

        private void OnDisable() { pointerHeld = false; SetHeld(false); }
    }
}
