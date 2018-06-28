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

    // Path measures
    public float movement_error;
    public float movement_offset;
    public float movement_variability;
    public int task_axis_crossing;
    public int target_reentry = -1;     // These are RE-ENTRIES. Will be -1 if they failed to enter the target at all, and 0 if they entered and never left the target area
    public int move_direction_change;   // Movement changes TOWARDS target
    public int ortho_move_direction_change; // Movement direction changes perpendicular to target
    // Distance between middle and nearest edge of target
    // Distance between starting mouse position and edge of target

    // Recorded at the END OF A ROUND
    // Round time is how long it took them to click it
    public Vector2 mouse_position_at_click;

    // NOT OUTPUT TO FILE
    public List<Vector3> travel_path_vector = new List<Vector3>();  // Vector3s to facility linerenderer debugging


    public override string ToString()
    {
        return base.ToString() + "," + device + "," + num_clicks + "," + target_radius + "," + target_center.x + "," + target_center.y + "," + mouse_position_at_click.x + 
            "," + mouse_position_at_click.y + "," + start_mouse_pos.x + "," + start_mouse_pos.y + "," + distance_between_starting_mouse_and_target + "," + distance_between_middle_and_target 
            + "," + movement_error + "," + movement_offset + "," + movement_variability + "," + task_axis_crossing + "," + target_reentry + "," + move_direction_change + "," + ortho_move_direction_change
            + "," + random_seed + "," + Round_Record.ListToString<float>(travel_path_x) + "," + Round_Record.ListToString<float>(travel_path_y);
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",device,num_clicks,target_radius,target_center_x,target_center_y,mouse_position_at_click_x" +
            ",mouse_position_at_click_y,start_mouse_pos_x,start_mouse_pos_y,distance_between_starting_mouse_and_target,distance_between_middle_and_target" +
            ",movement_error,movement_offset,movement_variability,task_axis_crossing,target_reentry,move_direction_change,ortho_move_direction_change" +
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

    public bool draw_cursor_path;
    public LineRenderer cursor_path;
    public LineRenderer task_axis_line;

    TwoDAimingTrialRecord prev_round_record;
    int prev_lag = 0;
    int prev_practice = 0;
    private const int unassigned_value = -99;

    public override void Awake()
    {
        aiming_trial = this;
        Cursor.visible = false;
        //UnityEngine.Random.InitState(random_seed);

        task_axis_line.sortingLayerName = "Text";
        cursor_path.sortingLayerName = "Text";

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


        ///////////////////////////////////////////////////////////////////////////////
        // Record path measures

        // Task axis (line from starting cursor position to final click position
        Vector2 task_axis_start = current_round_record.start_mouse_pos;     // Task axis in form of two points the line passes through
        // Should we calculate from optimal path, or path that they took?
        Vector2 task_axis_end = current_round_record.target_center;//current_round_record.mouse_position_at_click;
        Vector2 task_axis_heading = task_axis_end - task_axis_start;
        Vector2 task_axis_direction = task_axis_heading / task_axis_heading.magnitude;  // Normalized task axis direction

        Line task_axis_line = new Line(task_axis_start.x, task_axis_start.y, task_axis_end.x, task_axis_end.y);
        var go = new GameObject("start");
        go.transform.position = new Vector2((float)task_axis_start.x, (float)task_axis_start.y);
        go = new GameObject("end");
        go.transform.position = new Vector2((float)task_axis_end.x, (float)task_axis_end.y);

        // Determine line equation of task axis
        float m = (task_axis_end.y - task_axis_start.y) / (task_axis_end.x - task_axis_start.x);
        float b = task_axis_start.y - (m * task_axis_start.x);
        // Can now use equation: y = m*cur_x + b

        List<float> distances_from_task_axis = new List<float>();
        float sum_distance_from_task_axis = 0;
        float abs_distance_from_task_axis = 0;
        // Calculate distance from each point in travel path from task axis
        foreach (Vector2 p in current_round_record.travel_path_vector)
        {
            float dist_to_task_axis = DistanceFromPointToLine(p, task_axis_start, task_axis_end);
            if (p.y < m * p.x + b)    // Check if we're above or below the line
                dist_to_task_axis = -dist_to_task_axis;     // If we're below, set our distance to be negative
            distances_from_task_axis.Add(dist_to_task_axis);
            sum_distance_from_task_axis += dist_to_task_axis;
            abs_distance_from_task_axis += Mathf.Abs(dist_to_task_axis);
        }
        int n = distances_from_task_axis.Count;     // Number of points we're sampling

        // Calculate measures
        float movement_offset = sum_distance_from_task_axis / n;    // Mean distance from task axis
        float movement_error = abs_distance_from_task_axis / n;     // Absolute mean distance from task axis
        // Movement variability: standard deviation of of distances from task axis
        float sum = 0;
        foreach (float dist in distances_from_task_axis)
        {
            sum += Mathf.Pow(dist - movement_offset, 2);    // Movement offset is mean of distances from axis
        }
        float movement_variability = Mathf.Sqrt(sum / (n - 1));


        Vector2 prev_point = current_round_record.travel_path_vector[0];
        Vector2 cur_point = current_round_record.travel_path_vector[0];
        Vector2 previous_relative_dir = Vector2.zero;
        Vector2 relative_direction = Vector2.zero;
        float cross_product = unassigned_value;
        float prev_cross_product = unassigned_value;
        float angle = 0;
        float prev_angle = 0;
        int quadrant = unassigned_value;   // 1-2
        int prev_quadrant = unassigned_value;
        int signed_quadrant = unassigned_value;
        int prev_signed_quadrant = unassigned_value;
        int num_angled_movement_dir_changes = 0;
        for (int x = 1; x < current_round_record.travel_path_vector.Count; x++)
        {
            prev_point = cur_point;
            cur_point = current_round_record.travel_path_vector[x];

            // Check and see if the two points are the same position. If so, don't both calculating anything
            // Don't use the first couple of points since they're too close to the origin
            if (prev_point != cur_point && x > 3)
            {
                // Task axis crossing: how many times did the path cross the path axis? Check each pair of points (forming a line) if they crossed the task axis
                // Check if these two finite lines (task axis, current point & previous point cross
                // https://stackoverflow.com/a/46045077/2471482
                Point intersection = LineIntersection.FindIntersection(task_axis_line, new Line(prev_point.x, prev_point.y, cur_point.x, cur_point.y), finite_line_length: true);
                float dist_from_start = Vector2.Distance(task_axis_start, new Vector2((float)intersection.x, (float)intersection.y));
                if ((intersection.x != default(double) || intersection.y != default(double))// Null means no intersection was found
                   && dist_from_start > 0.1f)    // Check if we're far enough from the start, since it sometimes detects a crossing right off the bat
                {
                    current_round_record.task_axis_crossing++;
                    var got = new GameObject("Task Axis Crossing");
                    got.transform.position = new Vector2((float)intersection.x, (float)intersection.y);
                    // Found point intersection
                    Debug.Log("Intersection: " + cur_point.x + "," + cur_point.y 
                        + ", distance between two used points: " + Vector2.Distance(prev_point, cur_point) + ", P1: " + prev_point + ", P2:" + cur_point
                        + ",distance between start and inter: " + dist_from_start);
                }



                // movement direction change 
                // Follow: https://stackoverflow.com/a/51034671/2471482
                // To find needed points, you can detect sign change of cross product between axis and current curve direction (orientation/handedness test).
                Vector2 direction = cur_point - prev_point;
                prev_cross_product = cross_product;
                // Calculate the cross-product between our two directions
                cross_product = direction.x * task_axis_heading.y - direction.y * task_axis_heading.x;
                if (Mathf.Sign(cross_product) != Mathf.Sign(prev_cross_product) && prev_cross_product != unassigned_value)
                {
                    /*
                    var got = new GameObject("Movement Direction Change");
                    got.transform.position = new Vector2(cur_point.x, cur_point.y);
                    Debug.Log("Direction change. Prev cross-prod: " + prev_cross_product + ", current cross-prod: " + cross_product);
                    */
                    current_round_record.move_direction_change++;
                }




                // orthogonal movement direction change 
                // Get degree angle between task axis direction and current direction (prev_point - cur_point)
                // Check if the quadrant we are in changes (e.g. 0-90, 90-180, 180-270, 270-360)
                prev_angle = angle;
                angle = Vector2.Angle(direction, task_axis_heading);    // Returns an unsigned angle
                prev_quadrant = quadrant;
                // Assign a quadrant to this angle. Check if we're pointing forward (0-90) or backwards (90-180). If we changed, then we have an orthogonal movement direction change
                if (angle >= 0 && angle < 90)
                    quadrant = 1;
                else if (angle >= 90 && angle < 180)
                    quadrant = 2;
                if (prev_quadrant != quadrant && prev_quadrant != unassigned_value)
                {
                    var got = new GameObject("Orthogonal Movement Direction Change");
                    got.transform.position = new Vector2(cur_point.x, cur_point.y);
                    Debug.Log("Orthogonal Direction change. Prev quad: " + prev_quadrant + ", current cross-prod: " + quadrant);
                    current_round_record.ortho_move_direction_change++;
                }



                // TEST movement direction change w/o using cross-product
                // movement direction change 
                // Get degree angle between task axis direction and current direction (prev_point - cur_point)
                // Check if the quadrant we are in changes (e.g. 0-90, 90-180, 180-270, 270-360)
                float signed_angle = Vector2.SignedAngle(direction, task_axis_heading);    // Returns an unsigned angle
                prev_signed_quadrant = signed_quadrant;
                // Assign a quadrant to this angle. Check if we're pointing forward (0-90) or backwards (90-180). If we changed, then we have an orthogonal movement direction change
                if (signed_angle >= 0)
                    signed_quadrant = 1;
                else
                    signed_quadrant = -1;
                if (prev_signed_quadrant != signed_quadrant && prev_signed_quadrant != unassigned_value)
                {
                    num_angled_movement_dir_changes++;
                }
            }
        }


        // Re-entry shold be handled by a trigger

        current_round_record.movement_offset = movement_offset;
        current_round_record.movement_error = movement_error;
        current_round_record.movement_variability = movement_variability;
        Debug.Log("task-axis-crossings: " + current_round_record.task_axis_crossing + 
            ", movement dir change: " + current_round_record.move_direction_change + ", angled move dir changes: " + num_angled_movement_dir_changes + 
            ", ortho move dir change: " + current_round_record.ortho_move_direction_change +
            ", target re-entries: " + current_round_record.target_reentry +
            ", movement_offset: " + movement_offset + ", movement_error: " + movement_error + ", movement_variability: " + movement_variability + ", from number of points: " + n);
        /////////////////////////////////////////////////////////////////////////////////////


        GoBackToMiddle();
    }

    // Get the direction of our movement (comparing current point and previous point), and compare against direction to task axis
    public Vector2 RelativeDirection(Vector2 cur_point, Vector2 prev_point, Vector2 task_dir)
    {
        Vector2 heading = prev_point - cur_point;
        float distance = heading.magnitude;
        Vector2 direction = heading / distance; // This is now the normalized direction
        return (task_dir - direction).normalized;
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
        // Spawn a target a set distance from the player/cursor, in a random direction
        float distance = target_distances[current_round];
        Vector2 target_pos = GetRandomPosition(distance) + (Vector2) AimingMovement.aiming_movement.transform.position;
        Debug.Log(target_pos + " initial target position", this.gameObject);
        // Check if target pos is within screen bounds, getting a new random position until it is within the screen-bounds
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

        if (draw_cursor_path)
        {
            task_axis_line.SetPositions(new Vector3[] { current_round_record.start_mouse_pos, target_pos });
        }

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

            // TARGET RADIUS, DISTANCE TO TARGET
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

        current_round_record.start_mouse_pos = AimingMovement.aiming_movement.transform.position;

        SpawnNewTarget();

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
        base.FinishRound();
    }


    // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
    // Distance from a point to a line defined by two points that the line passes through
    private float DistanceFromPointToLine(Vector2 point, Vector2 line_1, Vector2 line_2)
    {
        return Mathf.Abs( (line_2.y - line_1.y) * point.x - (line_2.x - line_1.x) * point.y + line_2.x * line_1.y - line_2.y * line_1.x) 
            / Mathf.Sqrt( Mathf.Pow(line_2.y - line_1.y, 2) + Mathf.Pow(line_2.x - line_1.x, 2) );
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

        if (round_running)
        {
            if (draw_cursor_path)
            {
                cursor_path.positionCount = current_round_record.travel_path_vector.Count;
                cursor_path.SetPositions(current_round_record.travel_path_vector.ToArray());
            }
        }

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
