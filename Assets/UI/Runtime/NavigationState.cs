using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class NavigationState : MonoBehaviour
    {
        [Tooltip("Original scene control remains the owner of availability.")]
        public Button source;
        private Button view;
        private void Awake() { view = GetComponent<Button>(); }
        private void LateUpdate()
        {
            view.interactable = source && source.enabled && source.interactable && source.gameObject.activeInHierarchy;
        }
    }
}
