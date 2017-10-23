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


// Base class. New survey panels should inherit from this class
public class SurveyPanel : MonoBehaviour
{
    public GameObject object_to_activate_next;
    public string name_of_info;

    public virtual void SubmitInfo(string value)
    {
        ExtraRecordItem r = new ExtraRecordItem();
        r.name = name_of_info;
        r.value = value;
        Trial.trial.AddSurveyResultsToRecords(r);
        Next();
    }


    private void OnEnable()
    {
        // Pause the game when this panel is brought up
        Time.timeScale = 0;
    }

    public void Next()
    {
        // Activate the next object if there is one
        if (object_to_activate_next != null)
        {
            object_to_activate_next.SetActive(true);
        }
        // No object to activate, simply resume game
        else
        {
            Time.timeScale = 1.0f;

            // Disable the parent
            this.GetComponentInParent<Canvas>().gameObject.SetActive(false);
        }

        this.gameObject.SetActive(false);
    }

}