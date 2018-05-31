using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressButton : SurveyPanel
{
    public bool button_to_press = false;
    public string key_to_press;

    void Update ()
    {
        if (button_to_press)
        {
            if (Input.GetButtonDown(key_to_press))
                Next();
        }
        else
        {
            if (Input.GetKeyDown(key_to_press))
                Next();
        }
	}
}
