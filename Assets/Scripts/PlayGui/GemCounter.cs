using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemCounter : MonoBehaviour
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

    public void SetGemCount(int current, int total)
    {
        if (total == 0)
        {
            text.text = "";
        }
        else
        {
            text.text = current + "/" + total;
        }
    }
}
