using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class AvatarPicker : MonoBehaviour
    {
        [Serializable]
        public sealed class Choice
        {
            public string id;
            public Button button;
            public GameObject selectedMark;
        }

        public AvatarCatalog catalog;
        public Choice[] choices;
        public TMP_Text caption;
        public string SelectedId { get; private set; } = "owl";
        public event Action<string> SelectionChanged;

        private void Awake()
        {
            foreach (var choice in choices)
            {
                var id = choice.id;
                choice.button.onClick.AddListener(() => Select(id));
            }
            Refresh();
        }

        public void Select(string id)
        {
            var avatar = catalog ? catalog.Find(id) : null;
            if (avatar == null) return;
            SelectedId = avatar.id;
            Refresh();
            SelectionChanged?.Invoke(SelectedId);
        }

        private void Refresh()
        {
            foreach (var choice in choices) if (choice.selectedMark) choice.selectedMark.SetActive(choice.id == SelectedId);
            var avatar = catalog.Find(SelectedId);
            if (caption && avatar != null)
                caption.text = (PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? "Selected: " + avatar.english : "Pilihan: " + avatar.malay);
        }
    }
}
