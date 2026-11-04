using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class MenuFocus : MonoBehaviour
    {
        public Transform activities;
        private IEnumerator Start()
        {
            yield return null; // Availability mirrors run after the old scene's Start methods.
            if (!EventSystem.current) yield break;
            var owner = ModalInputOwner.ForScene(gameObject.scene);
            if (owner && owner.HasModal) yield break;
            foreach (var button in activities.GetComponentsInChildren<Button>())
                if (button.IsInteractable()) { EventSystem.current.SetSelectedGameObject(button.gameObject); break; }
        }
    }
}
