using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloPongLikeTeamRecord : Round_Record
{
    public int total_bounces, player_bounces, wall_bounces;      // How many times did the ball bounce off the paddle? The top wall? The higher score, the better
    public int unskilled_bounces, skilled_bounces;      // How many total times after the initial trajectory shot has it bounced off the paddle?
    public int skilled_bounces_minus_misses;            // Best way to measure actual performance. Must take into account later rounds are faster, which could result in more bounces!
    public int highest_skilled_bounce_streak;    // The best run of how many total times after the initial trajectory shot has it bounced off the paddle?
    public List<float> time_needed_on_skilled_bounces = new List<float>();
    public float avg_time_needed_on_skilled_bounces;
    public List<float> time_needed_on_unskilled_bounces = new List<float>();
    public float avg_time_needed_on_unskilled_bounces;
    public List<float> time_needed_on_all_bounces = new List<float>();
    public float avg_time_needed_on_all_bounces;
    public int total_misses;          // Each time the ball slips past is a miss. Want a low score (0 is the best possible score)
    public int unskilled_misses, skilled_misses;        // How many times the players missed the ball when the trajectory was shown. Trajectory is shown during the first shot of the round, and after every miss
    public float avg_dist_missed_by;    // Distance from the ball to the paddle when the ball entered 'end zone' (player screwed up)
    public float total_bounces_per_miss, unskilled_bounces_per_miss, skilled_bounces_per_miss;      // bounces / misses
    public float percent_bounces_missed;        // Having a high percentage is bad. 100% means all were missed. 50% means half were missed
    public float percent_skilled_bounces_missed;    // total_misses / skiled_bounces + total_misses. Having a high percentage is bad. 100% means were missed. 60% means 3/5ths were missed
    public float paddle_width, ball_radius, ball_speed, distance_between_players;
    public float min_ball_tat;  // Ball turn around time; how much time the player has to move before the ball reaches one end. distance (-radius/2) / ball_speed
                                // SHORTEST POSSIBLE TIME IS A STRAIGHT LINE. Most shots won't be a straight line, so they will have ever so slightly longer to react (+0.01/0.02ms)
    public List<float> time_needed_on_skilled_misses = new List<float>();      // Records how much time they had on each miss
    public float avg_time_needed_on_skilled_miss;   // When a ball is missed (and score), this is the average amount of time the player had to react
    public List<float> time_needed_on_unskilled_misses = new List<float>();      // Records how much time they had on each miss
    public float avg_time_needed_on_unskilled_miss;   // When a ball is missed (and score), this is the average amount of time the player had to react
    public List<float> time_needed_on_all_misses = new List<float>();      // Records how much time they had on each miss
    public float avg_time_needed_on_all_miss;   // When a ball is missed (and score), this is the average amount of time the player had to react
    public float total_screen_width;
    public float paddle_takes_percent_of_screen;


    public override string ToString()
    {
        return base.ToString() + "," + total_bounces + "," + player_bounces + "," + wall_bounces + "," + unskilled_bounces + "," + skilled_bounces + "," + skilled_bounces_minus_misses + "," + highest_skilled_bounce_streak
            + "," + Round_Record.ListToString<float>(time_needed_on_skilled_bounces) + "," + avg_time_needed_on_skilled_bounces
            + "," + Round_Record.ListToString<float>(time_needed_on_unskilled_bounces) + "," + avg_time_needed_on_unskilled_bounces
            + "," + Round_Record.ListToString<float>(time_needed_on_all_bounces) + "," + avg_time_needed_on_all_bounces 
            + "," + total_misses + "," + unskilled_misses + "," + skilled_misses
            + "," + avg_dist_missed_by + "," + total_bounces_per_miss + "," + unskilled_bounces_per_miss + "," + skilled_bounces_per_miss + "," + percent_bounces_missed + "," + percent_skilled_bounces_missed
            + "," + Round_Record.ListToString<float>(time_needed_on_skilled_misses) + "," + avg_time_needed_on_skilled_miss
            + "," + Round_Record.ListToString<float>(time_needed_on_unskilled_misses) + "," + avg_time_needed_on_unskilled_miss
            + "," + Round_Record.ListToString<float>(time_needed_on_all_misses) + "," + avg_time_needed_on_all_miss
            + "," + min_ball_tat + "," + paddle_width + "," + ball_radius + "," + ball_speed + "," + distance_between_players
            + "," + total_screen_width + "," + paddle_takes_percent_of_screen;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",total_bounces,player_bounces,wall_bounces,unskilled_bounces,skilled_bounces,skilled_bounces_minus_misses,highest_skill_streak"
            + ",time_needed_on_skilled_bounces,avg_time_needed_on_skilled_bounces"
            + ",time_needed_on_unskilled_bounces,avg_time_needed_on_unskilled_bounces"
            + ",time_needed_on_all_bounces,avg_time_needed_on_all_bounces"
            + ",total_misses,unskilled_misses,skilled_misses"
            + ",avg_dist_missed_by,total_bounces_per_miss,unskilled_bounces_per_miss,skilled_bounces_per_miss,percent_bounces_missed,percent_skilled_bounces_missed"
            + ",time_needed_on_skilled_misses,avg_time_needed_on_skilled_misses"
            + ",time_needed_on_unskilled_misses,avg_time_needed_on_unskilled_misses"
            + ",time_needed_on_all_misses,avg_time_needed_on_all_misses"
            + ",min_ball_tat,paddle_width,ball_radius,ball_speed,distance_between_players"
            + ",total_screen_width, paddle_takes_percent_of_screen";
    }
}


public class SoloPongLikeTeamPong : Trial
{
    public Transform position_to_spawn_ball;
    public Transform player_net;
    public Transform opposite_wall;
    public Text timer_text;
    public SoloPongLikeTeamRecord current_round_record;
    public LineRenderer starting_ball_path;
    Vector2 initial_ball_velocity;

    public TextAsset pong_settings;
    public List<float> ball_speeds = new List<float>();
    float current_ball_speed_of_round;  // Gotten from text file
    public List<float> distances_between_players = new List<float>();

    int shot_counter = 0;   // Used to alternate which direction the ball is being launched each time

    // Count skilled and unskilled bounces, streaks etc
    public int bounces_since_last_reset;


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



    public void RecordPlayerBounce()
    {
        current_round_record.player_bounces++;
        current_round_record.time_needed_on_all_bounces.Add(Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision());

        // Figure out if we're on a streak, or if this the initial trajectory bounce
        if (bounces_since_last_reset > 0)
        {
            current_round_record.skilled_bounces++;
            current_round_record.time_needed_on_skilled_bounces.Add(Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision());

            if (bounces_since_last_reset > current_round_record.highest_skilled_bounce_streak)
                current_round_record.highest_skilled_bounce_streak = bounces_since_last_reset;
        }
        else
        {
            current_round_record.unskilled_bounces++;
            current_round_record.time_needed_on_unskilled_bounces.Add(Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision());
        }

        bounces_since_last_reset++;
    }


    public override void StartRound()
    {
        ScoreManager.score_manager.ResetScore();

        bounces_since_last_reset = 0;
        current_ball_speed_of_round = ball_speeds[current_round];
        Ball.ball.max_speed = current_ball_speed_of_round;
        Ball.ball.Reset(Vector2.zero);

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = new Vector2(0, -distances_between_players[current_round] / 2);

        base.StartRound();
        round_running = false;

        // Get the random velocity of ball
        initial_ball_velocity = GetNewRandomBallVelocity();
        starting_ball_path.gameObject.SetActive(true);
        starting_ball_path.SetPosition(1, initial_ball_velocity.normalized * 99f);

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
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

        player_net.transform.position = ScoreManager.score_manager.players[0].transform.position;
        opposite_wall.transform.position = new Vector2(0, (distances_between_players[current_round] / 2) + current_round_record.ball_radius);

        // Turn on colliders
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        Ball.ball.SetCollisions(false);
        SetPlayerColliders(true);

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
        Ball.ball.GetComponent<PongBall>().time_of_last_collision = Time.time;

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
        bounces_since_last_reset = 0;

        Ball.ball.Reset(position);
        Ball.ball.physics.velocity = initial_ball_velocity;
        starting_ball_path.gameObject.SetActive(false);
        SetPlayerColliders(true);
    }


    public override void FinishRound()
    {
        current_round_record.total_bounces_per_miss = (float)current_round_record.player_bounces / (float) (current_round_record.total_misses == 0 ? 1 : current_round_record.total_misses);
        current_round_record.unskilled_bounces_per_miss = (float)current_round_record.unskilled_bounces / (float)(current_round_record.unskilled_misses == 0 ? 1 : current_round_record.total_misses);
        current_round_record.skilled_bounces_per_miss = (float)current_round_record.skilled_bounces / (float)(current_round_record.skilled_misses == 0 ? 1 : current_round_record.total_misses);
        current_round_record.skilled_bounces_minus_misses = current_round_record.skilled_bounces - current_round_record.total_misses;

        if (current_round_record.total_misses != 0)
        {
            current_round_record.percent_bounces_missed = (float)current_round_record.total_misses / (float)(current_round_record.total_misses + current_round_record.player_bounces);
            current_round_record.percent_skilled_bounces_missed = (float)current_round_record.total_misses / (float)(current_round_record.total_misses + current_round_record.skilled_bounces);
        }

        base.FinishRound();
    }
    public Vector2 GetNewRandomBallVelocity()
    {
        shot_counter++;
        return new Vector2(UnityEngine.Random.Range(-0.3f, 0.3f), -0.5f);
    }

    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloPongLikeTeamRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Average how much we missed by
        foreach (SoloPongLikeTeamRecord r in round_results)
        {
            // Bounces
            foreach (float time_needed in r.time_needed_on_skilled_bounces)
            {
                r.avg_time_needed_on_skilled_bounces += time_needed;
            }
            if (r.avg_time_needed_on_skilled_bounces != 0)
                r.avg_time_needed_on_skilled_bounces = r.avg_time_needed_on_skilled_bounces / r.time_needed_on_skilled_bounces.Count;

            foreach (float time_needed in r.time_needed_on_unskilled_bounces)
            {
                r.avg_time_needed_on_unskilled_bounces += time_needed;
            }
            if (r.avg_time_needed_on_unskilled_bounces != 0)
                r.avg_time_needed_on_unskilled_bounces = r.avg_time_needed_on_unskilled_bounces / r.time_needed_on_unskilled_bounces.Count;

            foreach (float time_needed in r.time_needed_on_all_bounces)
            {
                r.avg_time_needed_on_all_bounces += time_needed;
            }
            if (r.avg_time_needed_on_all_bounces != 0)
                r.avg_time_needed_on_all_bounces = r.avg_time_needed_on_all_bounces / r.time_needed_on_all_bounces.Count;


            // Misses
            if (r.avg_dist_missed_by != 0)
                r.avg_dist_missed_by = r.avg_dist_missed_by / r.total_misses;

            foreach (float time_needed in r.time_needed_on_skilled_misses)
            {
                r.avg_time_needed_on_skilled_miss += time_needed;
            }
            if (r.avg_time_needed_on_skilled_miss != 0)
                r.avg_time_needed_on_skilled_miss = r.avg_time_needed_on_skilled_miss / r.time_needed_on_skilled_misses.Count;

            foreach (float time_needed in r.time_needed_on_unskilled_misses)
            {
                r.avg_time_needed_on_unskilled_miss += time_needed;
            }
            if (r.avg_time_needed_on_unskilled_miss != 0)
                r.avg_time_needed_on_unskilled_miss = r.avg_time_needed_on_unskilled_miss / r.time_needed_on_unskilled_misses.Count;

            foreach (float time_needed in r.time_needed_on_all_misses)
            {
                r.avg_time_needed_on_all_miss += time_needed;
            }
            if (r.avg_time_needed_on_all_miss != 0)
                r.avg_time_needed_on_all_miss = r.avg_time_needed_on_all_miss / r.time_needed_on_all_misses.Count;
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

        // Calculate how much the ball missed by
        float missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[0].transform.position);
        current_round_record.avg_dist_missed_by += missed_by;
        float time_they_to_had_react = Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision();

        current_round_record.total_misses++;
        current_round_record.time_needed_on_all_misses.Add(time_they_to_had_react);

        if (bounces_since_last_reset > 0)
        {
            // Skilled miss
            current_round_record.skilled_misses++;
            current_round_record.time_needed_on_skilled_misses.Add(time_they_to_had_react);
        }
        else
        {
            // Unskilled miss
            current_round_record.unskilled_misses++;
            current_round_record.time_needed_on_unskilled_misses.Add(time_they_to_had_react);
        }

        Debug.Log("They had " + time_they_to_had_react + " seconds to react to that shot. TaT: " + current_round_record.min_ball_tat + " Cur time: " + Time.time + " Last collision time: " + Ball.ball.GetComponent<PongBall>().time_of_last_collision);
        Debug.Log(goal_name + "Ball missed by (distance) " + missed_by + ", total misses: " + current_round_record.total_misses);

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
        Ball.ball.GetComponent<PongBall>().time_of_last_collision = Time.time;

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


    public override void Start()
    {
        //StartTrial();
    }


    bool ball_was_below_before = false;
    public override void Update()
    {
        base.Update();

        /*
        // TESTING, just move to where ball is if it's about to score
        if (this.trial_running && Ball.ball.transform.position.y <= ScoreManager.score_manager.players[0].transform.position.y + current_round_record.ball_radius)
            ScoreManager.score_manager.players[0].GetComponent<Rigidbody2D>().MovePosition(
                new Vector2(Ball.ball.transform.position.x, ScoreManager.score_manager.players[0].transform.position.y));
        */

        // Monitor where ball is, turn off collisions if it's passed the players
        if (this.trial_running && (Ball.ball.transform.position.y <= ScoreManager.score_manager.players[0].transform.position.y))
        {
            /*
            if (!ball_was_below_before)
            {
                // How much time did they have to react from when ball the other paddle (or from starting position in middle, from which there is a line showing the trajectory)
                float time_they_to_had_react = Ball.ball.GetComponent<PongBall>().TimeSinceLastCollision();
                Debug.Log("They had " + time_they_to_had_react + " seconds to react to that shot. TaT: " + current_round_record.min_ball_tat + " Cur time: " + Time.time + " Last collision time: " + Ball.ball.GetComponent<PongBall>().time_of_last_collision);
                current_round_record.time_needed_on_misses.Add(time_they_to_had_react);
            }
            */
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
