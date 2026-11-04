using TMPro;
using UnityEngine;

namespace FunMath.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class BilingualLabel : MonoBehaviour
    {
        [TextArea] public string malay;
        [TextArea] public string english;
        private int previous = -1;
        private void Update()
        {
            int language = PlayerPrefs.GetInt("EnglishLang", 0);
            if (previous == language) return;
            previous = language;
            GetComponent<TMP_Text>().text = language == 1 ? english : malay;
        }
    }
}
