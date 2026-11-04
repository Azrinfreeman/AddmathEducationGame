using UnityEngine;

namespace FunMath.UI
{
    public sealed class AvatarPortrait : MonoBehaviour
    {
        public AvatarCatalog catalog;
        private RectTransform artwork;
        private string currentId;

        public void Show(string id)
        {
            var avatar = catalog ? catalog.Find(id) : null;
            if (avatar == null || !avatar.artwork || currentId == avatar.id) return;
            currentId = avatar.id;
            if (artwork) Destroy(artwork.gameObject);
            artwork = Instantiate(avatar.artwork, transform);
            artwork.anchorMin = Vector2.zero;
            artwork.anchorMax = Vector2.one;
            artwork.offsetMin = artwork.offsetMax = Vector2.zero;
        }
    }
}
