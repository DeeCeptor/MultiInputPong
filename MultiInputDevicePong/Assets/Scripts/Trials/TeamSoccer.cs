using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeamSoccerRecord : Round_Record
{
    public int blue_score;
    public int red_score;


    public TeamSoccerRecord()
    {

    }


    public override string ToString()
    {
        return base.ToString() + "," + blue_score + "," + red_score;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",blue_score,red_score";
    }
}


public class TeamSoccer : Trial 
{
    public Transform position_to_spawn_ball;
    //public List<TeamSoccerRecord> round_results = new List<TeamSoccerRecord>();  // Each round is an entry in this list
    public Text timer_text;
    public TeamSoccerRecord current_round_record;

    //public Text round_timer;    // Displays how much time is left in the round

    float pre_round_timer = 3.0f;

    public override void StartTrial()
    {
        base.StartTrial();

        ScoreManager.score_manager.CmdReset();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().disable_collider_after_kick = true;
    }


    public override void StartRound()
    {
        base.StartRound();
        round_running = false;

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);

        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            ScoreManager.score_manager.SpawnBall(position_to_spawn_ball.transform.localPosition);
        }
        else
            // Ball position
            Ball.ball.Reset(position_to_spawn_ball.transform.position);

        round_timer.text = "";
        round_timer.gameObject.SetActive(true);
        round_timer.enabled = true;

        ScoreManager.score_manager.EnablePlayerCollisions(false);

        // Get input lag for current round
        // Will need a bunch of participant ID's for this one
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Put player in correct spot
        //ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player.transform.position;
        // Ensure player has a collider enabled
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = false;
        ScoreManager.score_manager.ResetScore();

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
        Ball.ball.physics.velocity = Vector2.zero;
        // Allow player movement
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;

        start_beep.Play();
        round_running = true;
        ScoreManager.score_manager.EnablePlayerCollisions(true);
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new TeamSoccerRecord();
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

        current_round_record.red_score = ScoreManager.score_manager.red_score;
        current_round_record.blue_score = ScoreManager.score_manager.blue_score;

        // Reset ball position
        Ball.ball.Reset(Vector2.zero);

        // Disable collisions with the ball
        Ball.ball.SetCollisions(false);

        StartCoroutine(WaitAfterGoal());
    }
    IEnumerator WaitAfterGoal()
    {
        float time_countdown = 2.0f;
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

        start_beep.Play();
        Ball.ball.SetCollisions(true);
    }


    public override void Start()
    {
        //StartTrial();
    }


    public override void Update()
    {
        base.Update();

        if (trial_running)
        {
            round_timer.text = "" + (int) (time_limit - time_for_current_round);
        }
    }
}
