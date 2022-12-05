using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Scale : MonoBehaviour
{
    public int weight;

    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = weight.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "item"){
            weight += other.gameObject.GetComponent<WeightScale>().weight;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.tag == "item"){
             weight -= other.gameObject.GetComponent<WeightScale>().weight;
        }
    }
}
