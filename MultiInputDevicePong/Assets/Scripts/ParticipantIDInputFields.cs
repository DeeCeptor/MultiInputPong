using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantIDInputFields : MonoBehaviour 
{
    public int participant_index;

	void Start () 
	{
        if (GlobalSettings.participant_ids[participant_index] == 0)
            return;

        this.GetComponent<InputField>().text = "" + GlobalSettings.participant_ids[participant_index];
    }


    public void ValueChanged(string new_val)
    {
        int result;
        Int32.TryParse(new_val, out result);
        GlobalSettings.participant_ids[participant_index] = result;
    }
}