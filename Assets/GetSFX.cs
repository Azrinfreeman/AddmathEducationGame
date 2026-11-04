using UnityEngine;
using UnityEngine.UI;

public class GetSFX : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat("sfx", 1f);
    }
}
