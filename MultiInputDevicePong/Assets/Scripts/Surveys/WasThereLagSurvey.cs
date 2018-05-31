using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasThereLagSurvey : SurveyPanel
{
	void Update () 
	{
        // Check for 'Y' or 'N' press
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SubmitInfo("1");
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            SubmitInfo("0");
        }
    }
}