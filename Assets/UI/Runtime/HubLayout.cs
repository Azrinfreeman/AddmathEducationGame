using UnityEngine;

namespace FunMath.UI
{
    [ExecuteAlways]
    public sealed class HubLayout : MonoBehaviour
    {
        public RectTransform owl;
        public RectTransform activities;
        [Tooltip("Four primary activity signs, with supporting routes in the footer.")]
        public bool playgroundMainMenu;
        private bool? previous;
        private void Update()
        {
            var canvas = GetComponentInParent<Canvas>();
            bool wide = ((RectTransform)canvas.transform).rect.width > ((RectTransform)canvas.transform).rect.height;
            if (previous == wide) return;
            previous = wide;
            if (playgroundMainMenu)
            {
                owl.anchorMin = wide ? new Vector2(.055f,.3f) : new Vector2(.35f,.66f);
                owl.anchorMax = wide ? new Vector2(.33f,.72f) : new Vector2(.65f,.79f);
                activities.anchorMin = wide ? new Vector2(.38f,.23f) : new Vector2(.07f,.24f);
                activities.anchorMax = wide ? new Vector2(.94f,.76f) : new Vector2(.93f,.65f);
                owl.offsetMin = owl.offsetMax = activities.offsetMin = activities.offsetMax = Vector2.zero;
                return;
            }
            owl.anchorMin = wide ? new Vector2(.05f,.23f) : new Vector2(.3f,.64f);
            owl.anchorMax = wide ? new Vector2(.36f,.72f) : new Vector2(.7f,.78f);
            activities.anchorMin = wide ? new Vector2(.4f,.23f) : new Vector2(.06f,.23f);
            activities.anchorMax = wide ? new Vector2(.94f,.72f) : new Vector2(.94f,.62f);
            owl.offsetMin = owl.offsetMax = activities.offsetMin = activities.offsetMax = Vector2.zero;
        }
    }
}
