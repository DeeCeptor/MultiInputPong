using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeamPongRecord : Round_Record
{
    public int total_bounces = 0;      // How many times did the ball bounce off the paddle? The higher score, the better
    public int player_1_bounces, player_2_bounces;  // Times  bounces off the paddle of each player. 1 = Top of screen, 0 = Bottom of screen
    public int total_misses;          // Each time the ball slips past is a miss. Want a low score (0 is the best possible score)
    public int player_1_misses, player_2_misses;  // Which player missed the ball? 1 = Top of screen, 0 = Bottom of screen
    public float percent_bounces_missed;        // Having a high percentage is bad. 100% means all were missed. 50% means half were missed
    public float avg_dist_missed_by;     // Distance from the ball to the paddle when the ball entered 'end zone' (player screwed up)
    public float bounces_per_miss;      // total_bounces / total_misses
    public float paddle_width, ball_radius, ball_speed, distance_between_players;
    public float min_ball_tat;  // Ball turn around time; how much time the player has to move before the ball reaches one end. distance (-radius/2) / ball_speed
                                // SHORTEST POSSIBLE TIME IS A STRAIGHT LINE. Most shots won't be a straight line, so they will have ever so slightly longer to react (+0.01/0.02ms)
    public List<float> time_needed_on_misses = new List<float> ();      // Records how much time they had on each miss
    public float avg_time_needed_on_miss;   // When a ball is missed (and score), this is the average amount of time the player had to react
    public float total_screen_width;
    public float paddle_takes_percent_of_screen;


    public override string ToString()
    {
        return base.ToString() + "," + total_bounces + "," + player_1_bounces + "," + player_2_bounces
            + "," + total_misses + "," + player_1_misses + "," + player_2_misses + "," + avg_dist_missed_by + "," + bounces_per_miss + "," + percent_bounces_missed
            + "," + Round_Record.ListToString<float>(time_needed_on_misses) + "," + avg_time_needed_on_miss + "," + min_ball_tat
            + "," + paddle_width + "," + ball_radius + "," + ball_speed + "," + distance_between_players
            + "," + total_screen_width + "," + paddle_takes_percent_of_screen;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",total_bounces,player_1_bounces,player_2_bounces"
            + ",total_misses,player_1_misses,player_2_misses,avg_dist_missed_by,bounces_per_miss,percent_bounces_missed"
            + ",time_needed_on_misses,avg_time_needed_on_miss,min_ball_tat"
            + ",paddle_width,ball_radius,ball_speed,distance_between_players"
            + ",total_screen_width, paddle_takes_percent_of_screen";
    }
}


public class TeamPong : Trial 
{
    public Transform position_to_spawn_ball;
    public List<Transform> positions_to_spawn_player = new List<Transform>();
    public Text timer_text;
    public TeamPongRecord current_round_record;
    public LineRenderer starting_ball_path;
    Vector2 initial_ball_velocity;

    public TextAsset pong_settings;
    public List<float> ball_speeds = new List<float>();
    float current_ball_speed_of_round;  // Gotten from text file
    public List<float> distances_between_players = new List<float>();

    int shot_counter = 0;   // Used to alternate which direction the ball is being launched each time


    public override void StartTrial()
    {
        PopulatePongSettings();
        base.StartTrial();
    }
    public void PopulatePongSettings()
    {
        if (pong_settings == null)
            return;

        ball_speeds.Clear();
        string[] splits = { "\n" };
        string[] str_vals = pong_settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            // Ball speed, distance between players
            string[] items = s.Split(',');
            ball_speeds.Add(float.Parse(items[0]));
            distances_between_players.Add(float.Parse(items[1]));
        }
        Debug.Log("Done loading pong settings", this.gameObject);
    }


    public override void StartRound()
    {
        ScoreManager.score_manager.ResetScore();

        current_ball_speed_of_round = ball_speeds[current_round];
        Ball.ball.max_speed = current_ball_speed_of_round;
        Ball.ball.Reset(Vector2.zero);

        base.StartRound();
        round_running = false;

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = new Vector2(0, -distances_between_players[current_round] / 2);
        positions_to_spawn_player[0].transform.position = ScoreManager.score_manager.players[0].transform.position;
        ScoreManager.score_manager.players[1].transform.position = new Vector2(0, distances_between_players[current_round] / 2);
        positions_to_spawn_player[1].transform.position = ScoreManager.score_manager.players[1].transform.position;
        ScoreManager.score_manager.players[1].transform.Rotate(new Vector3(0, 0, 180));     // Flip top player around 180 so collider is properly aligned

        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        ScoreManager.score_manager.players[1].GetComponent<SingleMouseMovement>().ResetKicks();

        Ball.ball.SetCollisions(false);
        SetPlayerColliders(true);


        // Get the random velocity of ball
        initial_ball_velocity = GetNewRandomBallVelocity();
        starting_ball_path.gameObject.SetActive(true);
        starting_ball_path.SetPosition(1, initial_ball_velocity.normalized * 99f);

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.participant_id = GlobalSettings.GetParticipantId(0) + "." + GlobalSettings.GetParticipantId(1);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Pong specific calculations
        current_round_record.ball_radius = Ball.ball.GetComponent<CircleCollider2D>().radius * Ball.ball.transform.localScale.x;
        current_round_record.paddle_width = ScoreManager.score_manager.players[0].transform.localScale.x;
        current_round_record.distance_between_players = Mathf.Abs(ScoreManager.score_manager.players[0].transform.position.y) * 2;
        current_round_record.ball_speed = current_ball_speed_of_round;

        // Distance between players - (radius of ball * 2, because radius is half with the diameter, so 2 radii gives a full length of ball)
        // Maybe don't need radius * 2 if we only care when the ball's center position passes the paddle
        float total_distance_needed = current_round_record.distance_between_players;// - (current_round_record.ball_radius * 2);
        current_round_record.min_ball_tat = total_distance_needed / current_ball_speed_of_round;
        current_round_record.total_screen_width = CameraRect.camWidth;
        current_round_record.paddle_takes_percent_of_screen = current_round_record.paddle_width / current_round_record.total_screen_width;

        Debug.Log("Min tat: " + current_round_record.min_ball_tat 
            + " dist needed: " + total_distance_needed 
            + " dist between players: " + current_round_record.distance_between_players
            + " ball speed: " + current_ball_speed_of_round);

        StopShootAfterGoal();
        StartCoroutine(StartRoundIn());
    }
    IEnumerator StartRoundIn()
    {
        Ball.ball.Reset(Vector2.zero);

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
        Debug.Log("Starting round");
        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        // Allow player movement
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
    }
    public void ResetAndShootBall(Vector2 position)
    {
        Ball.ball.Reset(position);
        Ball.ball.physics.velocity = initial_ball_velocity;
        starting_ball_path.gameObject.SetActive(false);
        SetPlayerColliders(true);
        Ball.ball.GetComponent<PongBall>().time_of_last_collision = Time.time;
    }


    public override void FinishRound()
    {
        current_round_record.bounces_per_miss = (float) current_round_record.total_bounces / (float) (current_round_record.total_misses == 0 ? 1 : current_round_record.total_misses);

        if (current_round_record.total_misses != 0)
        {
            current_round_record.percent_bounces_missed = (float) current_round_record.total_misses / (float) (current_round_record.total_misses + current_round_record.total_bounces);
        }
        base.FinishRound();
    }
    public Vector2 GetNewRandomBallVelocity()
    {
        shot_counter++;
        return new Vector2(UnityEngine.Random.Range(-0.3f, 0.3f), shot_counter % 2 == 0 ? 0.5f : -0.5f);
    }

    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new TeamPongRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Average how much we missed by
        foreach (TeamPongRecord r in round_results)
        {
            if (r.avg_dist_missed_by != 0)
                r.avg_dist_missed_by = r.avg_dist_missed_by / r.total_misses == 0 ? 1 : r.total_misses;

            foreach (float time_needed in r.time_needed_on_misses)
            {
                r.avg_time_needed_on_miss += time_needed;
            }
            if (r.avg_time_needed_on_miss != 0)
                r.avg_time_needed_on_miss = r.avg_time_needed_on_miss / r.time_needed_on_misses.Count;
        }

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }


    // Pong ball entered end zone, finish round
    public void GoalScored(string goal_name = "none")
    {
        if (!trial_running)
            return;

        base.GoalScored();

        float missed_by = 0;
        // Figure out which net was just scored on
        if (goal_name.Contains("Top"))  // Player 2
        {
            current_round_record.player_2_misses++;
            missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[1].transform.position);
        }
        else if (goal_name.Contains("Bottom"))  // Player 1
        {
            current_round_record.player_1_misses++;
            missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[0].transform.position);
        }
        // Shouldn't happen, check just in case
        else
        {
            Debug.LogError("NO NET NAMED " + goal_name);
        }

        // Calculate how much the ball missed by
        current_round_record.avg_dist_missed_by += missed_by;
        current_round_record.total_misses += 1;

        // How much time did they have to react from when ball the other paddle (or from starting position in middle, from which there is a line showing the trajectory)
        float time_they_to_had_react = Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision();
        current_round_record.time_needed_on_misses.Add(time_they_to_had_react);

        Debug.Log(goal_name + "Ball missed by " + missed_by + ", total misses: " + current_round_record.total_misses);
        Debug.Log("They had " + time_they_to_had_react + " seconds to react to that shot. TaT: " + current_round_record.min_ball_tat + " Cur time: " + Time.time + " Last collision time: " + Ball.ball.GetComponent<PongBall>().time_of_last_collision);

        start_beep.Play();
        StopShootAfterGoal();
        ShootAfterGoal = StartCoroutine(StopThenShootBall());
    }
    Coroutine ShootAfterGoal;
    IEnumerator StopThenShootBall()
    {
        initial_ball_velocity = GetNewRandomBallVelocity();
        starting_ball_path.gameObject.SetActive(true);
        starting_ball_path.SetPosition(1, initial_ball_velocity * 100);
        Ball.ball.Reset(Vector2.zero);

        // Wait a second, then shoot the ball
        float time_left = 1.0f;
        while (time_left > 0)
        {
            time_left -= Time.deltaTime;
            yield return 0;
        }

        // Shoot ball
        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
    }
    public void StopShootAfterGoal()
    {
        if (ShootAfterGoal != null)
            StopCoroutine(ShootAfterGoal);
    }


    public override void Start () 
	{
        //StartTrial();
	}


    bool ball_was_below_before = false;
	public override void Update () 
	{
        base.Update();

        // Monitor where ball is, turn off collisions if it's passed the players
        if (this.trial_running &&
            (Ball.ball.transform.position.y <= ScoreManager.score_manager.players[0].transform.position.y
            || Ball.ball.transform.position.y >= ScoreManager.score_manager.players[1].transform.position.y))
        {
            /*
            if (!ball_was_below_before)
            {

            }*/
            ball_was_below_before = true;
            SetPlayerColliders(false);
        }
        else
            ball_was_below_before = false;
	}

    public void SetPlayerColliders(bool enabled)
    {
        foreach (Player p in ScoreManager.score_manager.players)
        {
            p.GetComponent<Collider2D>().enabled = enabled;
        }
    }
}
