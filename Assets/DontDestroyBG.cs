using UnityEngine;

public class DontDestroyBG : MonoBehaviour
{
    public static DontDestroyBG instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // Persist only the audio hierarchy, not its scene-specific parent.
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
