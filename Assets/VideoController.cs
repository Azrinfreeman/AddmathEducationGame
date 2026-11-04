using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Transform startButton;
    public Transform stopButton;

    void Start()
    {
        // GameObject.Find skips inactive objects, and in most AR scenes the "video" object starts switched off.
        if (!videoPlayer)
            foreach (var candidate in FindObjectsByType<VideoPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (candidate.gameObject.name == "video" && candidate.gameObject.scene == gameObject.scene) { videoPlayer = candidate; break; }
        // Find the buttons by name anywhere under this object. The old lookup (second child, then its first and
        // second child) threw when a scene nested them differently, as ARIsipadu does (Canvas/Panel/2/Play).
        if (!startButton) startButton = FindDeep(transform, "Play");
        if (!stopButton) stopButton = FindDeep(transform, "Stop");
        if (videoPlayer != null) videoPlayer.loopPointReached += EndReached;
        else Debug.LogWarning("VideoController: no object named 'video' with a VideoPlayer in " + gameObject.scene.name, this);
    }

    void OnDestroy()
    {
        if(videoPlayer != null) videoPlayer.loopPointReached -= EndReached;
    }

    void EndReached(VideoPlayer vp)
    {
        if (startButton) startButton.gameObject.SetActive(true);
        if (stopButton) stopButton.gameObject.SetActive(false);
    }

    // Depth-first search that includes inactive objects (the Play button starts disabled).
    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            var found = FindDeep(child, name);
            if (found) return found;
        }
        return null;
    }
}
