using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpaceInvadersRecord : Round_Record
{
    public override string ToString()
    {
        return base.ToString() + ",";
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",";
    }
}


public class SpaceInvaders : Trial
{
    public TextAsset settings;
    public SpaceInvadersRecord current_round_record;



    public override void StartTrial()
    {
        PopulateSettings();
        base.StartTrial();
    }
    public void PopulateSettings()
    {
        if (settings == null)
            return;

        /*
        ball_speeds.Clear();
        string[] splits = { "\n" };
        string[] str_vals = settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            // Ball speed, distance between players
            string[] items = s.Split(',');
            ball_speeds.Add(float.Parse(items[0]));
            distances_between_players.Add(float.Parse(items[1]));
        }
        */
        Debug.Log("Done loading settings", this.gameObject);
    }


    public override void StartRound()
    {
        // Create enemies

        // Reset player position

        // Reset scores

        base.StartRound();

        // Create new round record
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Set current round features
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SpaceInvadersRecord();
    }


    public override void FinishRound()
    {
        // Record any more necessary round data

        base.FinishRound();
    }


    public override void FinishTrial()
    {
        base.FinishTrial();
    }


    public override void Start()
    {

    }


    public override void Update()
    {
        base.Update();
    }
}
