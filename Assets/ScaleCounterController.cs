using UnityEngine;

public class ScaleCounterController : MonoBehaviour
{
    public Scale scale1;
    public Scale scale2;

    public int count = 0;

    public Transform winPanel;
    public Transform losePanel;

    private bool decided;
    private string recordedPlacement = "";   // placement already reported online
    public event System.Action PlacementChanged;
    private int displayedCount = -1;

    void Start()
    {
       scale1 = transform.GetChild(0).GetComponent<Scale>();
       scale2 = transform.GetChild(1).GetComponent<Scale>();

       winPanel = transform.parent.transform.parent.GetChild(11).GetComponent<Transform>();
       losePanel = transform.parent.transform.parent.GetChild(12).GetComponent<Transform>();
    }

    // counts how many balls are currently on a scale
    public void Checking(){
        count = 0;
        BallController balls = BallController.instance;
        if(balls == null || balls.Balls == null) return;
        for(int i = 0; i < balls.num && i < balls.Balls.Length; i++){
            if(balls.Balls[i]){
                count++;
            }
        }
    }

    public bool AllPlaced(){
        Checking();
        return BallController.instance != null && BallController.instance.num > 0 && count == BallController.instance.num;
    }

    public void Confirming(){
        if(!AllPlaced()) return;

        // Report once per distinct placement, even if Confirming() is called again for the same balls.
        bool balanced = scale1.weight == scale2.weight;
        string placement = scale1.weight + "|" + scale2.weight;
        if(placement != recordedPlacement){
            recordedPlacement = placement;
            MathFun.OnlineData.OnlineResults.Outcome(MathFun.OnlineData.OnlineResults.SceneActivity("game_balance_"), balanced);
        }

        if(balanced){
            winPanel.gameObject.SetActive(true);
            var celebration = winPanel.GetComponent<FunMath.UI.ResultCelebration>();
            if (celebration) celebration.Play();
        }else{
            losePanel.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // evaluate once when all balls are placed; re-arm if the player removes one
        bool placed = AllPlaced();
        if (displayedCount != count)
        {
            displayedCount = count;
            PlacementChanged?.Invoke();
        }
        if(placed && !decided){
            decided = true;
            Confirming();
        }else if(!placed){
            decided = false;
            recordedPlacement = "";   // balls were removed: the next full placement is a new attempt
        }
    }
}
