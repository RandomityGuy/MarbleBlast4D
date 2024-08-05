using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerComponent : MonoBehaviour
{
    Text text;

    private void Awake()
    {
        text = gameObject.GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTime(float t)
    {
        text.text = Util.FormatTimer(t);
    }
}
