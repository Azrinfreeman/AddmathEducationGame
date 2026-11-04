using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace FunMath.UI
{
    /// <summary>Scene-local input and focus ownership. Only the top visible scope receives input.</summary>
    [DefaultExecutionOrder(-200), DisallowMultipleComponent]
    public sealed class ModalInputOwner : MonoBehaviour
    {
        [Tooltip("Main content groups; modal roots must be outside these groups.")]
        public CanvasGroup[] content = Array.Empty<CanvasGroup>();
        public bool HasModal => stack.Count > 0;
        public event Action<bool> VisibilityChanged;
        private readonly List<Entry> stack = new List<Entry>();
        private readonly Dictionary<CanvasGroup, Vector2> saved = new Dictionary<CanvasGroup, Vector2>();
        private sealed class Entry { public ModalInputScope scope; public CanvasGroup group; public GameObject previous; public bool interactable; public bool raycasts; }

        public static ModalInputOwner ForScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var owner = root.GetComponentInChildren<ModalInputOwner>(true);
                if (owner) return owner;
            }
            return null;
        }

        public void Push(ModalInputScope scope)
        {
            if (stack.Exists(e => e.scope == scope)) return;
            var group = scope.GetComponent<CanvasGroup>();
            if (!group) group = scope.gameObject.AddComponent<CanvasGroup>();
            stack.Add(new Entry { scope = scope, group = group, previous = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null,
                interactable = group.interactable, raycasts = group.blocksRaycasts });
            Apply(); SelectTop(); VisibilityChanged?.Invoke(true);
        }
        public void Pop(ModalInputScope scope)
        {
            int index = stack.FindIndex(e => e.scope == scope);
            if (index < 0) return;
            var entry = stack[index]; bool top = index == stack.Count - 1;
            stack.RemoveAt(index);
            if (entry.group) { entry.group.interactable = entry.interactable; entry.group.blocksRaycasts = entry.raycasts; }
            Apply();
            if (top && EventSystem.current)
            {
                var previous = entry.previous;
                var control = previous ? previous.GetComponent<Selectable>() : null;
                if (control && control.IsActive() && control.IsInteractable()) EventSystem.current.SetSelectedGameObject(previous);
                else SelectTop();
            }
            VisibilityChanged?.Invoke(HasModal);
        }
        private void Apply()
        {
            if (HasModal)
            {
                foreach (var group in content)
                {
                    if (!group) continue;
                    if (!saved.ContainsKey(group)) saved[group] = new Vector2(group.interactable ? 1 : 0, group.blocksRaycasts ? 1 : 0);
                    group.interactable = group.blocksRaycasts = false;
                }
            }
            else
            {
                foreach (var pair in saved) if (pair.Key) { pair.Key.interactable = pair.Value.x > 0; pair.Key.blocksRaycasts = pair.Value.y > 0; }
                saved.Clear();
            }
            for (int i = 0; i < stack.Count; i++)
                if (stack[i].group) { stack[i].group.interactable = i == stack.Count - 1; stack[i].group.blocksRaycasts = i == stack.Count - 1; }
        }
        private void SelectTop()
        {
            if (!HasModal || !EventSystem.current) return;
            var scope = stack[stack.Count - 1].scope;
            var first = scope.firstControl;
            if (!first || !first.IsActive() || !first.IsInteractable())
                foreach (var candidate in scope.GetComponentsInChildren<Selectable>())
                    if (candidate.IsInteractable()) { first = candidate; break; }
            EventSystem.current.SetSelectedGameObject(first ? first.gameObject : null);
        }
        private void LateUpdate()
        {
            if (!HasModal) return;
            var scope = stack[stack.Count - 1].scope;
            bool cancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            cancel |= Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
            if (cancel) { scope.Close(); return; }
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (!selected || !selected.transform.IsChildOf(scope.transform)) SelectTop();
        }
        private void OnDisable()
        {
            foreach (var entry in stack) if (entry.group) { entry.group.interactable = entry.interactable; entry.group.blocksRaycasts = entry.raycasts; }
            stack.Clear(); Apply(); VisibilityChanged?.Invoke(false);
        }
    }
}
