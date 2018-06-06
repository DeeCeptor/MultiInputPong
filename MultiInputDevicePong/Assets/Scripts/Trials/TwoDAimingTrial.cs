using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Should I record one row for EACH time they clicked on a target? or one row PER 

public class TwoDAimingTrialRecord : Round_Record
{
    public GlobalSettings.Input_Device_Type device;     // Independent variable, were they using the mouse, drawing tablet or touchscreen?

    // Recorded at the START OF A ROUND
    public Vector2 target_center;
    public float distance_between_starting_mouse_and_target;    // Center to center distance (not counting radius). Starting mouse position
    public float distance_between_middle_and_target;            // Center to center distance (not counting radius). Middle is (0,0)
    public Vector2 start_mouse_pos;
    public float target_radius; // Size of target area (radius, in-game units)
    public int num_clicks;
    public int random_seed;
    public List<float> travel_path_x = new List<float>();
    public List<float> travel_path_y = new List<float>();
    // Distance between middle and nearest edge of target
    // Distance between starting mouse position and edge of target

    // Recorded at the END OF A ROUND
    // Round time is how long it took them to click it
    public Vector2 mouse_position_at_click;


    public override string ToString()
    {
        return base.ToString() + "," + device + "," + num_clicks + "," + target_radius + "," + target_center.x + "," + target_center.y + "," + mouse_position_at_click.x + 
            "," + mouse_position_at_click.y + "," + start_mouse_pos.x + "," + start_mouse_pos.y + "," + distance_between_starting_mouse_and_target + "," + distance_between_middle_and_target 
            + "," + random_seed + "," + Round_Record.ListToString<float>(travel_path_x) + "," + Round_Record.ListToString<float>(travel_path_y);
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",device,num_clicks,target_radius,target_center_x,target_center_y,mouse_position_at_click_x" +
            ",mouse_position_at_click_y,start_mouse_pos_x,start_mouse_pos_y,distance_between_starting_mouse_and_target,distance_between_middle_and_target" +
            ",random_seed,travel_path_x,travel_path_y";
    }
}


public class TwoDAimingTrial : Trial
{
    public static TwoDAimingTrial aiming_trial;

    public Text timer_text;
    public TwoDAimingTrialRecord current_round_record;

    public TextAsset settings;

    public GameObject mouse_UI_screen;
    public GameObject drawing_tablet_UI_screen;
    public GameObject touchscreen_UI_screen;
    public GameObject controller_UI_screen;
    public GameObject survey_canvas;

    public GameObject go_to_middle;
    public GameObject target;
    public List<float> target_sizes = new List<float>();
    public List<float> target_distances = new List<float>();
    public int random_seed = 1234;

    TwoDAimingTrialRecord prev_round_record;
    int prev_lag = 0;
    int prev_practice = 0;


    public override void Awake()
    {
        aiming_trial = this;
        Cursor.visible = false;
        //UnityEngine.Random.InitState(random_seed);

        base.Awake();
    }


    public void ClickedOnTarget(GameObject target, GameObject mouse_pos)
    {
        // Record stuffs

        // Location of target
        // Mouse location
        current_round_record.mouse_position_at_click = mouse_pos.transform.position;
        // Time taken
        //
        target.SetActive(false);
        round_running = false;

        GoBackToMiddle();
    }
    // Player must click on middle to start the next round
    public void GoBackToMiddle()
    {
        go_to_middle.SetActive(true);
    }
    // Starts a new round (with a new round record)
    public void ReturnedToMiddle()
    {
        Debug.Log("Clicked on middle");
        go_to_middle.SetActive(false);

        // Start the trail if we haven't already
        if (!this.trial_running)
        {
            Debug.Log("Starting trial");
            StartTrial();
            return;
        }

        NextRound();
    }
    public void SpawnNewTarget()
    {
        // Get new target center postion
        // Spawn a target that ISN't OFF THE SCREEN
        /*Vector2 target_pos = new Vector2(
            UnityEngine.Random.Range(CameraRect.camera_settings.bottomleft.transform.position.x, CameraRect.camera_settings.topright.transform.position.x),
            UnityEngine.Random.Range(CameraRect.camera_settings.bottomleft.transform.position.y, CameraRect.camera_settings.topright.transform.position.y));*/

        // Spawn a target a set distance from the player, in a random direction
        // Check if target pos is within screen bounds
        float distance = target_distances[current_round];
        Vector2 target_pos = GetRandomPosition(distance) + (Vector2) AimingMovement.aiming_movement.transform.position;
        // NOT WORKING. MUST BE DISTANCE FROM MOUSE TOO
        // ENSURE IT ISNT OFF-SCREEN
        while (!CameraRect.camera_settings.PointWithinCameraBounds(target_pos))
        {
            Debug.Log(target_pos + " not within camera bounds", this.gameObject);
            target_pos = GetRandomPosition(distance) + (Vector2)AimingMovement.aiming_movement.transform.position;
        }

        target.transform.position = target_pos;
        current_round_record.target_center = target_pos;
        target.SetActive(true);

        // Set target size
        current_round_record.target_radius = target_sizes[current_round];
        target.transform.localScale = new Vector3(current_round_record.target_radius, current_round_record.target_radius, 1);

        current_round_record.distance_between_middle_and_target = Vector2.Distance(Vector2.zero, target_pos);

        Debug.Log("Spawned new target at " + target_pos);
    }
    public Vector2 GetRandomPosition(float radius)
    {
        return UnityEngine.Random.insideUnitCircle.normalized * radius;
    }

    public override void StartTrial()
    {
        PopulateSettings();
        base.StartTrial();
    }
    // Add target distances?
    public void PopulateSettings()
    {
        Debug.Log("start loading settings", this.gameObject);
        if (settings == null)
            return;

        //ball_speeds.Clear();
        string[] splits = { "\n" };
        string[] str_vals = settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            // Ball speed, distance between players
            string[] items = s.Split(',');

            target_sizes.Add(float.Parse(items[0]));
            target_distances.Add(float.Parse(items[1]));
            //ball_speeds.Add(float.Parse(items[0]));
            //distances_between_players.Add(float.Parse(items[1]));
        }
        Debug.Log("Done loading settings", this.gameObject);
    }


    // Called 3 times per participant, called when a Practice round starts
    public void SetInputDeviceType()
    {
        if (GlobalSettings.order_of_device_types.Count > 0)
        {
            GlobalSettings.current_input_device = GlobalSettings.order_of_device_types.Dequeue();
            Debug.Log("Current device is: " + GlobalSettings.current_input_device);
        }
        else
        {
            GlobalSettings.current_input_device = GlobalSettings.Input_Device_Type.Mouse;
            Debug.LogWarning("Warning, no input device order set, using " + GlobalSettings.current_input_device);
            return;
        }

        // Reinitialize random seed so EACH device case gets the same random numbers
        UnityEngine.Random.InitState(random_seed);

        // Bring up UI, to show them what to use, press ENTER when ready
        survey_canvas.SetActive(true);
        switch (GlobalSettings.current_input_device)
        {
            case GlobalSettings.Input_Device_Type.Mouse:
                mouse_UI_screen.SetActive(true);
                break;
            case GlobalSettings.Input_Device_Type.Drawing_Tablet:
                drawing_tablet_UI_screen.SetActive(true);
                break;
            case GlobalSettings.Input_Device_Type.Touchscreen:
                touchscreen_UI_screen.SetActive(true);
                break;
            case GlobalSettings.Input_Device_Type.Controller:
                controller_UI_screen.SetActive(true);
                break;
        }
    }

    public override void StartRound()
    {
        //ScoreManager.score_manager.ResetScore();
        // Put player in correct spot
        //ScoreManager.score_manager.players[0].transform.position = Vector2.zero;

        base.StartRound();

        round_running = false;

        // Add new round record to list
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];
        current_round_record.random_seed = random_seed;

        // IF this is a practice round, set the device type
        // May need to change this
        // NOT WORKING
        //String.Compare(other_terms_rounds[current_round].Trim(), "switch");
        if (other_terms_rounds[current_round] != "")
            //||other_terms_rounds[current_round] == "switch" || String.Equals("switch", other_terms_rounds[current_round], StringComparison.InvariantCultureIgnoreCase))  //( "switch" == other_terms_rounds[current_round])
        {
            Debug.Log("SWITCHING DEVICE:"+ other_terms_rounds[current_round]+":");
            SetInputDeviceType();
        }
        else
        {

        }
        current_round_record.device = GlobalSettings.current_input_device;

        // Reset random seed if we're switching lag levels
        if (prev_round_record == null 
            || prev_round_record.ms_input_lag_of_round != current_round_record.ms_input_lag_of_round 
            || (prev_round_record.practice_round == 1 && !IsCurrentRoundRoundPractice()) )
        {
            if (prev_round_record != null)
                Debug.Log("RESETTING RANDOM SEED, new lag: " + current_round_record.ms_input_lag_of_round + ", prev lag: " + prev_round_record.ms_input_lag_of_round
                + " practice: " + current_round_record.practice_round + " prev practice: " + prev_round_record.practice_round);
            UnityEngine.Random.InitState(random_seed);
        }

        SpawnNewTarget();

        current_round_record.start_mouse_pos = AimingMovement.aiming_movement.transform.position;
        current_round_record.distance_between_starting_mouse_and_target = Vector2.Distance(AimingMovement.aiming_movement.transform.position, current_round_record.target_center);

        //StartCoroutine(StartRoundIn());
        start_beep.Play();
        round_running = true;
    }
    IEnumerator StartRoundIn()
    {
        float time_countdown = 1.0f;
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

        // Allow player movement
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;
        start_beep.Play();

        round_running = true;
    }


    public override void FinishRound()
    {
        // Record stuff

        base.FinishRound();
    }

    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();
        prev_round_record = current_round_record;
        current_round_record = new TwoDAimingTrialRecord();
    }


    public override void FinishTrial()
    {
        // Calculate averages and stuff

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }

    
    public override void Start()
    {
        base.Start();
    }
    


    bool ball_was_below_before = false;
    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !trial_running)
            StartTrial();

        base.Update();
    }

    public void SetPlayerColliders(bool enabled)
    {
        foreach (Player p in ScoreManager.score_manager.players)
        {
            p.GetComponent<Collider2D>().enabled = enabled;
        }
    }
}
