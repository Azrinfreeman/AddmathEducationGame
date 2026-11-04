using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>Read-only presentation of the existing per-player achievement keys.</summary>
    public sealed class BadgeShelf : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Entry
        {
            public string malayTitle;
            public string englishTitle;
            [TextArea] public string malayHint;
            [TextArea] public string englishHint;
            [Tooltip("Existing preference prefixes, without the player ID.")]
            public string[] progressKeys;
            public Button button;
            public TMP_Text status;
            public Image artwork;
        }

        public Entry[] entries;
        public GameObject details;
        public TMP_Text detailTitle;
        public TMP_Text detailBody;
        [Tooltip("Tint of collected stickers; unfinished stickers retain their artwork in a muted tint.")]
        public Color earnedTint = Color.white;
        public Color exploringTint = new Color(.65f, .72f, .74f, 1);
        private int selected = -1;
        private int language = -1;

        private void Start()
        {
            for (int i = 0; i < entries.Length; i++)
            {
                int index = i;
                entries[i].button.onClick.AddListener(() => Show(index));
            }
            Refresh();
        }

        private void Update()
        {
            if (language != PlayerPrefs.GetInt("EnglishLang", 0)) Refresh();
            // Migrated dialogs delegate Back and focus restoration to their scene owner.
            if (details.activeSelf && !details.GetComponent<ModalInputScope>() && Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused && entries != null) Refresh();
        }

        [ContextMenu("Refresh saved progress")]
        public void Refresh()
        {
            language = PlayerPrefs.GetInt("EnglishLang", 0);
            foreach (var entry in entries)
            {
                if (!entry.status) continue;
                int done = Completed(entry);
                if (entry.artwork) entry.artwork.color = done == entry.progressKeys.Length ? earnedTint : exploringTint;
                entry.status.text = done == entry.progressKeys.Length
                    ? (language == 1 ? "Earned!" : "Berjaya!")
                    : (language == 1 ? "Keep exploring" : "Terus mencuba") + "\n" + done + " / " + entry.progressKeys.Length;
            }
            if (selected >= 0 && details.activeSelf) Show(selected);
        }

        private static int Completed(Entry entry)
        {
            int player = PlayerPrefs.GetInt("PlayerID");
            int done = 0;
            foreach (string key in entry.progressKeys)
                if (PlayerPrefs.GetInt(key + player) == 1) done++;
            return done;
        }

        public void Show(int index)
        {
            selected = index;
            var entry = entries[index];
            detailTitle.text = language == 1 ? entry.englishTitle : entry.malayTitle;
            detailBody.text = (language == 1 ? entry.englishHint : entry.malayHint) + "\n\n" + Completed(entry) + " / " + entry.progressKeys.Length;
            details.SetActive(true);
        }

        public void Close() { details.SetActive(false); }
    }
}
