using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressButton : SurveyPanel
{
    public string key_to_press;

    void Update ()
    {
        if (Input.GetKeyDown(key_to_press))
            Next();
	}
}
