using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Transform startButton;
    public Transform stopButton;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GameObject.Find("video").GetComponent<VideoPlayer>();
        startButton = transform.GetChild(1).transform.GetChild(0).GetComponent<Transform>();
        stopButton = transform.GetChild(1).transform.GetChild(1).GetComponent<Transform>();
        videoPlayer.loopPointReached += EndReached;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
       
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
        Debug.Log("stop");
    }



}
