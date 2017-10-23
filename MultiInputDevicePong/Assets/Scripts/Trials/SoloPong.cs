using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloPongRecord : Round_Record
{
    public int total_bounces;       // How many times did the ball bounce off the paddle? The higher score, the better
    public int total_misses;        // Each time the ball slips past is a miss. Want a low score (0 is the best possible score)
    public float avg_missed_by;     // Distance from the ball to the paddle when the ball entered 'end zone' (player screwed up)
    public float bounces_per_miss;      // total_bounces / total_misses
    public float paddle_width, ball_radius, ball_speed, distance_to_top_wall;
    public float min_ball_tat;  // Ball turn around time; how much time the player has to move before the ball reaches one end. distance (-radius/2) / ball_speed
    public float total_screen_width;
    public float paddle_takes_percent_of_screen;


    public override string ToString()
    {
        return base.ToString() + "," + total_bounces + "," + total_misses + "," + avg_missed_by
            + "," + paddle_width + "," + ball_radius + "," + ball_speed + "," + distance_to_top_wall + "," + min_ball_tat
            + "," + total_screen_width + "," + paddle_takes_percent_of_screen;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",total_bounces,total_misses,avg_missed_by"
                        + ",paddle_width,ball_radius,ball_speed,distance_to_top_wall,ball_tat"
                        + ",total_screen_width,paddle_takes_percent_of_screen";
    }
}


public class SoloPong : Trial 
{
    public Transform position_to_spawn_ball;
    public Transform position_to_spawn_player;
    public Text timer_text;
    public SoloPongRecord current_round_record;

    float normal_ball_max_speed;


    public override void StartTrial()
    {
        normal_ball_max_speed = Ball.ball.max_speed;
        base.StartTrial();

        //ScoreManager.score_manager.CmdReset();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().disable_collider_after_kick = true;
    }


    public override void StartRound()
    {
        ScoreManager.score_manager.ResetScore();

        this.StopAllCoroutines();
        Ball.ball.max_speed = normal_ball_max_speed;
        Ball.ball.Reset(position_to_spawn_ball.transform.position);

        base.StartRound();
        round_running = false;

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player.transform.position;
        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        Ball.ball.SetCollisions(false);

        StartCoroutine(StartRoundIn());
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


        // Ball velocity
        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        // Allow player movement
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
        Ball.ball.max_speed = normal_ball_max_speed;
    }
    public void ResetAndShootBall(Vector2 position)
    {
        Ball.ball.Reset(position);
        // Set random x/y angle
        Ball.ball.physics.velocity = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), 0.5f);
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloPongRecord();
    }


    public override void FinishRound()
    {
        // Pong specific calculations
        current_round_record.ball_radius = Ball.ball.GetComponent<CircleCollider2D>().radius * Ball.ball.transform.localScale.x;
        current_round_record.paddle_width = ScoreManager.score_manager.players[0].transform.localScale.x;

        //asd
        // Orthographic size not orthographic size/2
        current_round_record.distance_to_top_wall = Camera.main.orthographicSize / 2 + Mathf.Abs(ScoreManager.score_manager.players[0].transform.position.y);
        current_round_record.ball_speed = normal_ball_max_speed;

        // Distance between players - (radius of ball * 4, because radius is half with the diameter, so 2 radii gives a full length of ball)
        float total_distance_needed = current_round_record.distance_to_top_wall - (current_round_record.ball_radius * 2);
        current_round_record.min_ball_tat = total_distance_needed / normal_ball_max_speed;
        current_round_record.total_screen_width = CameraRect.camWidth;
        current_round_record.paddle_takes_percent_of_screen = current_round_record.paddle_width / current_round_record.total_screen_width;

        current_round_record.bounces_per_miss = current_round_record.total_bounces / current_round_record.total_misses == 0 ? 0 : current_round_record.total_misses;

        ScoreManager.score_manager.ResetScore();

        base.FinishRound();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Average how much we missed by
        foreach (SoloPongRecord r in round_results)
        {
            if (r.avg_missed_by != 0)
                r.avg_missed_by = r.avg_missed_by / r.total_misses;
        }

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }


    // Pong ball entered end zone, finish round
    public override void GoalScored()
    {
        if (!trial_running)
            return;

        base.GoalScored();

        // Calculate how much the ball missed by
        float missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[0].transform.position);
        current_round_record.avg_missed_by += missed_by;
        current_round_record.total_misses += 1;
        Debug.Log("Ball missed by " + missed_by + ", total misses: " + current_round_record.total_misses);

        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        start_beep.Play();
        StartCoroutine(StopThenSpeedUpBall());
    }
    IEnumerator StopThenSpeedUpBall()
    {
        float f = 0.05f;
        while (f < 1)
        {
            Ball.ball.max_speed = normal_ball_max_speed * f;
            f += Time.deltaTime * 0.5f;
            yield return 1;
        }
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
