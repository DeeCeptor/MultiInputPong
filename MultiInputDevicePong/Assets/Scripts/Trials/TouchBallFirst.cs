using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TouchBallFirstRecord: Round_Record
{
    public bool scored = false;


    public override string ToString()
    {
        return base.ToString() + "," + scored;
    }
}


public class TouchBallFirst : Trial
{
    public Transform position_to_spawn_ball;
    public Transform position_to_spawn_player_1;
    public Transform position_to_spawn_player_2;
    public Text round_countdown;
    //public List<SoloKickIntoNetRecord> round_results = new List<SoloKickIntoNetRecord>();  // Each round is an entry in this list
    //bool successful_this_round = false;
    public TouchBallFirstRecord current_round_record;

    public override void StartTrial()
    {
        if (ScoreManager.score_manager.players.Count <= 1)
        {
            Debug.Log("Not enough players");
            return;
        }

        base.StartTrial();

        //ScoreManager.score_manager.CmdReset();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().disable_collider_after_kick = true;
    }


    public override void StartRound()
    {
        base.StartRound();
        round_running = false;

        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            ScoreManager.score_manager.SpawnBall(position_to_spawn_ball.transform.localPosition);
        }
        else
        {
            // Ball position
            Ball.ball.Reset(position_to_spawn_ball.transform.position);
        }

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Put players in correct spot
        ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player_1.transform.position;
        ScoreManager.score_manager.players[1].transform.position = position_to_spawn_player_2.transform.position;
        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        ScoreManager.score_manager.players[1].GetComponent<SingleMouseMovement>().ResetKicks();
        // Make players can't move
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = false;
        ScoreManager.score_manager.players[1].GetComponent<SingleMouseMovement>().ResetKicks();

        Ball.ball.SetCollisions(false);

        StartCoroutine(StartRoundIn());
    }
    IEnumerator StartRoundIn()
    {
        float time_countdown = 3.0f;
        round_countdown.gameObject.SetActive(true);
        int prev_time = 0;
        while (time_countdown > 0)
        {
            time_countdown -= Time.deltaTime;
            int new_time = (int)time_countdown + 1;
            round_countdown.text = "" + new_time;
            if (new_time != prev_time)
                timer_beeps.Play();
            prev_time = new_time;
            yield return null;
        }
        round_countdown.gameObject.SetActive(false);


        // Ball velocity
        Ball.ball.physics.velocity = new Vector2(0, -10);
        // Allow player movement
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new TouchBallFirstRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

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
