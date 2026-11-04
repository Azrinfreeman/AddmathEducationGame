using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>Registers an existing dialog without replacing its buttons or visibility callbacks.</summary>
    [DisallowMultipleComponent]
    public sealed class ModalInputScope : MonoBehaviour
    {
        public ModalInputOwner owner;
        public Selectable firstControl;
        public bool allowBack = true;
        public UnityEvent back = new UnityEvent();
        public System.Action closeRequested;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (!owner) owner = ModalInputOwner.ForScene(gameObject.scene);
            if (owner) owner.Push(this);
        }
        public void Bind(ModalInputOwner value)
        {
            if (owner == value) return;
            if (owner) owner.Pop(this);
            owner = value;
            if (owner && isActiveAndEnabled) owner.Push(this);
        }
        public void Close()
        {
            if (!allowBack) return;
            if (closeRequested != null) closeRequested();
            else if (back.GetPersistentEventCount() > 0) back.Invoke();
            else gameObject.SetActive(false);
        }
        private void OnDisable() { if (owner) owner.Pop(this); }
        private void OnDestroy() { if (owner) owner.Pop(this); }
    }
}
