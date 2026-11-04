using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class AgeStepper : MonoBehaviour
    {
        public TMP_InputField input;
        public Button decrease;
        public Button increase;
        [Min(1)] public int minimum = 1;
        [Min(1)] public int maximum = 120;
        [Min(1)] public int initial = 7;
        private int value;

        private void Start()
        {
            value = Mathf.Clamp(int.TryParse(input.text, out int saved) ? saved : initial, minimum, maximum);
            decrease.onClick.AddListener(() => Change(-1));
            increase.onClick.AddListener(() => Change(1));
            Change(0);
        }

        public void Change(int delta)
        {
            value = Mathf.Clamp(value + delta, minimum, maximum);
            input.SetTextWithoutNotify(value.ToString());
            decrease.interactable = value > minimum;
            increase.interactable = value < maximum;
        }

        private void OnValidate()
        {
            minimum = Mathf.Max(1, minimum);
            maximum = Mathf.Max(minimum, maximum);
            initial = Mathf.Clamp(initial, minimum, maximum);
        }
    }
}
