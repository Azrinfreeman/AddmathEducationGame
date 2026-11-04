using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FunMath.UI
{
    [RequireComponent(typeof(Button)), DefaultExecutionOrder(-100)]
    public sealed class ParentGate : MonoBehaviour
    {
        public ParentGateDialog dialogPrefab;
        private UnityEvent original;
        private ParentGateDialog pending;
        private void Awake()
        {
            var button = GetComponent<Button>();
            original = button.onClick;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(Request);
        }
        private void Request()
        {
            if (pending) return;
            if (!dialogPrefab) { Debug.LogError("Parent gate dialog is missing.", this); return; }
            pending = Instantiate(dialogPrefab);
            pending.Open(() => { if (this && isActiveAndEnabled) original.Invoke(); }, ModalInputOwner.ForScene(gameObject.scene));
        }
    }
}
