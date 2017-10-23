using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloKickStationaryIntoNetRecord : Round_Record
{
    public int ball_position = 0;   // Where did the ball spawn?
    public int scored = 0;
    // Accuracy: (positive means above, negative means below net)
    //          90
    // +-180            0
    //          -90
    public float accuracy;  // Measurement of angle of miss. The larger the value, the bigger the miss. 0 means it went in

    public SoloKickStationaryIntoNetRecord()
    {

    }


    public override string ToString()
    {
        return base.ToString() + "," + ball_position + "," + scored + "," + accuracy;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",ball_position,scored,accuracy";
    }
}


public class SoloKickStationaryIntoNet : Trial 
{
    public List<Transform> positions_to_spawn_ball = new List<Transform>();
    public Transform position_to_spawn_player;
    public Text timer_text;
    public SoloKickStationaryIntoNetRecord current_round_record;

    Vector2 starting_ball_pos = Vector2.zero;
    float top_of_net_arc;
    float bottom_of_net_arc;
    float middle_of_net_arc;

    public override void StartTrial()
    {
        base.StartTrial();
    }

    int ball_pos_counter = 0;
    public override void StartRound()
    {
        base.StartRound();
        SetCurrentBallPosition();
        round_running = false;

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.ball_position = ball_pos_counter;
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            GameObject go = ScoreManager.score_manager.SpawnBall(positions_to_spawn_ball[0].transform.localPosition);
            starting_ball_pos = positions_to_spawn_ball[0].transform.localPosition;
        }
        else
        {
            // Get position
            // Ball position
            starting_ball_pos = positions_to_spawn_ball[ball_pos_counter].transform.position;
            Ball.ball.Reset(starting_ball_pos);
        }

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player.transform.position;
        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = false;
        Ball.ball.SetCollisions(false);

        StartCoroutine(StartRoundIn());


        // Figure out arc of ball
        // Get next top and bottom positions
        Net n = GameObject.FindGameObjectWithTag("Net").GetComponent<Net>();
        top_of_net_arc = AngleBetweenVector2(positions_to_spawn_ball[ball_pos_counter].transform.position, n.top_of_net.transform.position);
        bottom_of_net_arc = AngleBetweenVector2(positions_to_spawn_ball[ball_pos_counter].transform.position, n.bottom_of_net.transform.position);
        middle_of_net_arc = AngleBetweenVector2(positions_to_spawn_ball[ball_pos_counter].transform.position, (n.bottom_of_net.transform.position + n.top_of_net.transform.position) / 2);
        Debug.Log("Angle to enter goal: " + top_of_net_arc + " : " + bottom_of_net_arc 
            + ", bal start pos: "  + Ball.ball.transform.position 
            + ", top of net pos: " + n.top_of_net.transform.position
            + ", middle of net pos: " + (n.bottom_of_net.transform.position + n.top_of_net.transform.position) / 2
            + ", bot of net pos: " + n.bottom_of_net.transform.position);
    }
    IEnumerator StartRoundIn()
    {
        float time_countdown = 3.0f;
        timer_text.gameObject.SetActive(true);
        int prev_time = 0;
        while (time_countdown > 0)
        {
            time_countdown -= Time.deltaTime;

            int new_time = (int)time_countdown + 1;
            timer_text.text = "" + new_time;
            if (new_time != prev_time)
                timer_beeps.Play();
            prev_time = new_time;
            yield return null;
        }
        timer_text.gameObject.SetActive(false);

        // Can collide with ball
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
    }


    public void SetCurrentBallPosition()
    {
        ball_pos_counter = current_round % positions_to_spawn_ball.Count;
        Debug.Log("Ball pos " + ball_pos_counter);
    }


    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }
    public void SetAccuracy(Vector2 position_hit)
    {
        if (!this.trial_running)
            return;

        float angle_to_hit = AngleBetweenVector2(positions_to_spawn_ball[ball_pos_counter].transform.position, position_hit);

        float difference_to_top = angle_to_hit - top_of_net_arc;
        float difference_to_bot = angle_to_hit - bottom_of_net_arc;
        float difference_to_middle_of_net = angle_to_hit - middle_of_net_arc;
        float smallest_diff = Mathf.Min( Mathf.Abs(difference_to_top), Mathf.Abs(difference_to_middle_of_net), Mathf.Abs(difference_to_bot) );
        current_round_record.accuracy = difference_to_middle_of_net;

        // Calculate difference between angle_to_hit and the arc of the goal
        if (angle_to_hit > bottom_of_net_arc && angle_to_hit < top_of_net_arc)
        {
            Debug.Log("It went in!");
        }
        else
            Debug.Log("Missed!");

        Debug.Log("Angle hit: " + angle_to_hit 
                + " smallest dif: " + smallest_diff 
                + ", dif to top: " + difference_to_top 
                + ", dif to mid: " + difference_to_middle_of_net 
                + ", dif to bot: " + difference_to_bot , this.gameObject);
    }

    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloKickStationaryIntoNetRecord();
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


    public override void GoalScored()
    {
        if (!trial_running)
            return;

        base.GoalScored();
        current_round_record.scored = 1;
        SetAccuracy(Ball.ball.transform.position);

        if (trial_running)
            NextRound();
    }


    public override void Start () 
	{
        //StartTrial();
	}
	

	public override void Update () 
	{
        base.Update();
	}
}
