using TMPro;
using UnityEngine;

namespace FunMath.UI
{
    public sealed class ProfileFormLanguage : MonoBehaviour
    {
        public TMP_Text namePlaceholder;
        public TMP_Dropdown gender;

        private void Start()
        {
            bool english = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            namePlaceholder.text = english ? "Your name" : "Nama kamu";
            namePlaceholder.fontStyle = FontStyles.Normal;
            string[] labels = english ? new[] { "Prefer not to say", "Boy", "Girl" } : new[] { "Tidak dinyatakan", "Lelaki", "Perempuan" };
            for (int i = 0; i < gender.options.Count && i < labels.Length; i++) gender.options[i].text = labels[i];
            gender.RefreshShownValue();
        }
    }
}
