using System;
using UnityEngine;

namespace FunMath.UI
{
    [CreateAssetMenu(menuName = "Fun Math/Avatar Catalog")]
    public sealed class AvatarCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Avatar
        {
            [Tooltip("Stable saved ID. Do not rename after shipping.")]
            public string id;
            public string malay;
            public string english;
            public RectTransform artwork;
        }

        public Avatar[] avatars;

        public Avatar Find(string id)
        {
            if (avatars == null || avatars.Length == 0) return null;
            foreach (var avatar in avatars) if (avatar != null && avatar.id == id) return avatar;
            foreach (var avatar in avatars) if (avatar != null) return avatar;
            return null;
        }

        public string Read(int playerId) => Find(PlayerPrefs.GetString("PlayerAvatar_" + playerId, "owl"))?.id ?? "owl";
    }
}
