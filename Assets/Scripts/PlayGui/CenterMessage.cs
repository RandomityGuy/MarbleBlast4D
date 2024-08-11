using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterMessage : MonoBehaviour
{
    Text text;
    string curMsg;

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

    public void SetMessage(string msg)
    {
        if (curMsg == msg) return;
        curMsg = msg;
        text.text = msg;
    }
}
