using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LikertScaleQuestion : SurveyPanel
{
    public int min_number = 1;
    public int max_number = 7;

    void Update () 
	{
        for (int x = min_number; x <= max_number; x++)
        {
            if (Input.GetKeyDown("" + x)
                || Input.GetKeyDown("[" + x + "]"))
            {
                SubmitInfo("" + x);
                break;
            }
        }
    }
}
