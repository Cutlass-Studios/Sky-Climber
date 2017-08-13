using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PressLogic : MonoBehaviour {

    private ColorBlock fb;
    private ColorBlock cb;
    private bool on = false;

	// Use this for initialization
	void Start () {
        fb = gameObject.GetComponent<Button>().colors;
    }
	
	// Update is called once per frame
	void Update () {
        cb = fb;
        if (on)
        {
            cb.highlightedColor = new Color(231, 0, 0, 0.4f);
            cb.pressedColor = new Color(231, 0, 0, 0.4f);
            cb.normalColor = new Color(231, 0, 0, 0.4f);
        }
        else
        {
            cb.highlightedColor = new Color(255, 255, 255, 0.4f);
            cb.pressedColor = new Color(255, 255, 255, 0.4f);
            cb.normalColor = new Color(255, 255, 255, 0.4f);
        }
        gameObject.GetComponent<Button>().colors = cb;
        on = false;
    }

    public void pressed()
    {
        on = true;    
    }
}
