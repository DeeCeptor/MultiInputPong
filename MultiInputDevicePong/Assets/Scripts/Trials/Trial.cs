using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Round_Record
{
    public int trial_id;     // What task are we doing?
    public string participant_id;   // . separated list of participants being used
    public int round_number;
    public int ms_input_lag_of_round;
    public float round_time_taken = 0;
    public int practice_round = 0;     //  0 false: not a practice round, 1 true: is a practice round
    public int num_rounds_since_survey;      // Is this the 1st round after a survey? The 2nd? 3rd?
    //public int noticed_lag = 0;        //  0 false, 1 true
    public List<ExtraRecordItem> survey_questions = new List<ExtraRecordItem> ();   // Noticed lag, competence, internal external, etc

    public virtual new string ToString()
    {
        return trial_id + "," + participant_id + "," + round_number + "," + practice_round + "," + num_rounds_since_survey + "," + ms_input_lag_of_round + "," + round_time_taken;
    }
    public virtual string ExtrasToString()
    {
        string return_val = "";
        foreach (ExtraRecordItem r in survey_questions)
        {
            return_val += "," + r.value;
        }
        return return_val;
    }


    public virtual string FieldNames()
    {
        return "trial_id,participant_id,round_number,practice_round,num_rounds_since_survey,input_lag,time_for_round";
    }
    public virtual string ExtrasFieldNames()
    {
        string return_val = "";
        foreach (ExtraRecordItem r in survey_questions)
        {
            return_val += "," + r.name;
        }
        return return_val;
    }


    public static string ListToString<T>(List<T> a_list)
    {
        List<string> strings = new List<string> ();
        for (int x = 0; x < a_list.Count; x++)
        {
            strings.Add(a_list[x].ToString());
        }

        if (a_list.Count > 0)
            return String.Join(":", strings.ToArray());
        else
            return "0";
    }
}


public class ExtraRecordItem
{
    public string name;
    public string value;
}


// Base class of a trial
// Trial consists of multiple rounds being executed over and over
public class Trial : MonoBehaviour 
{
    public static Trial trial;

    public List<Round_Record> round_results = new List<Round_Record>();

    public int trial_id = 0;    // What task are we doing? Stationary kick into net is 1, moving kick into net is 2, etc
    public int number_players_required = 1;
    public int current_round = -1;   // Current round
    public int total_rounds = 0;    // How many rounds are we aiming for?
    public float time_for_current_round;    // In seconds, elapsed time of current trial
    public float total_time_for_trial;  // In seconds, how long this trial has lasted
    public bool trial_running = false;
    public bool round_running = false;
    public bool enforce_time_limit = false;     // Does this trial have a time limit for each round? Ex: each round is 3 seconds
    public float time_limit = 3.0f;     // Time limit for the current round
    public bool half_time_for_practice_rounds = false;
    float default_time_limit;

    public List<GameObject> survey_objects_to_activate = new List<GameObject>();

    public TextAsset input_delay_values;        // One value per line
    public List<int> input_delay_per_round = new List<int>();   // Read from a text file (input_delay_values)
    public bool[] practice_rounds;      // Read from a text file, which rounds are practice (don't count for data)
    public bool[] survey_rounds;        // Read from a text file, which rounds should launch a survey after they're done?

    public string text_file_name = "Results.txt";

    public AudioSource timer_beeps;
    public AudioSource start_beep;
    public Text round_timer;
    public Text practice_round_text;        // Tells the user if this is a practice round
    public bool to_menu_after_trial = false;


    public virtual void Awake()
    {
        trial = this;
        practice_rounds = new bool[total_rounds];
        survey_rounds = new bool[total_rounds];
        default_time_limit = time_limit;
        PopulateInputDelays();
    }
    public virtual void Start()
    {

    }


    // Input delay files are .txt files separated by newlines
    public void PopulateInputDelays()
    {
        if (input_delay_values == null)
            return;

        input_delay_per_round.Clear();
        string[] splits = { "\n" };
        string[] str_vals = input_delay_values.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        // Go through each line
        int round_number = 0;
        foreach (string s in str_vals)
        {
            string[] items = s.Split(',');

            // Get the input delay
            input_delay_per_round.Add(int.Parse(items[0]));

            if (items.Length > 1 && total_rounds > round_number)
            {
                // Get the practice round
                if (items[1].Contains("Practice"))
                    practice_rounds[round_number] = true;
                // Get the survey
                if (items[1].Contains("Survey"))
                    survey_rounds[round_number] = true;
            }

            round_number++;
        }

        Debug.Log("Done loading input delay values", this.gameObject);
    }


    public virtual void StartTrial()
    {
        UnityEngine.Random.InitState(999);

        ResetTrial();
        MultiMouseManager.mouse_manager.players_can_join = false;
        trial_running = true;
        NextRound();
    }


    // A trial consists of multiple rounds
    public virtual void StartRound()
    {
        round_running = true;

        // Set the alotted input delay for this round
        if (input_delay_per_round.Count > current_round)
        {
            GlobalSettings.InputDelayFrames = input_delay_per_round[current_round];
            
            Debug.Log("Setting input delay to: " + input_delay_per_round[current_round], this.gameObject);
        }

        if (practice_round_text != null)
            practice_round_text.gameObject.SetActive(IsCurrentRoundRoundPractice());

        if (IsCurrentRoundRoundPractice() && half_time_for_practice_rounds)
        {
            time_limit = default_time_limit / 2f;
        }
        else
            time_limit = default_time_limit;

        Debug.Log("Starting round " + current_round);
    }
    public virtual void StopRound()
    {
        trial_running = false;
    }


    public virtual void NextRound()
    {
        if (current_round >= 0)
            FinishRound();

        ResetBetweenRounds();

        current_round++;
        if (current_round < total_rounds)
        {
            // Wait to start so survey has a moment to pause
            StartCoroutine(WaitToStartRound());
        }
        else
        {
            StartCoroutine(WaitToFinishTrial());
        }
    }
    IEnumerator WaitToStartRound()
    {
        round_running = false;
        yield return new WaitForSeconds(0.01f);
        StartRound();
    }
    IEnumerator WaitToFinishTrial()
    {
        yield return new WaitForSeconds(0.03f);

        FinishTrial();
    }
    // Record anything we need to before resetting
    public virtual void FinishRound()
    {
        round_results[current_round].round_time_taken = time_for_current_round;
        round_results[current_round].round_number = current_round + 1;
        round_results[current_round].trial_id = trial_id;
        //round_results[current_round].num_rounds_since_survey = practice_rounds_per_survey > 0 ? current_round % survey_every_x_rounds : 0;
        round_results[current_round].practice_round = IsCurrentRoundRoundPractice() ? 1 : 0;

        // Should we bring up the survey window?
        if (survey_rounds[current_round])
        {
            ActivateSurvey();
            ScoreManager.score_manager.ResetScore();
        }
    }


    public bool IsCurrentRoundRoundPractice()
    {
        return practice_rounds[current_round];
    }


    // Bring up the survey window, pausing the game
    public virtual void ActivateSurvey()
    {
        Debug.Log("Activating survey", this.gameObject);
        foreach (GameObject g in survey_objects_to_activate)
        {
            g.SetActive(true);
        }
    }
    public virtual void AddSurveyResultsToRecords(ExtraRecordItem r)
    {
        // Add to the last X trial records that this survey applied to
        for (int x = current_round; x > 0; x--)
        {
            if (x != current_round && survey_rounds[x - 1])
                break;

            round_results[x - 1].survey_questions.Add(r);
        }
    }


    public virtual void ResetBetweenRounds()
    {
        total_time_for_trial += time_for_current_round;
        time_for_current_round = 0;
    }
    
    
    public virtual void FinishTrial()
    {        
        // Reset everything
        ResetTrial();

        Debug.Log("Finished trial");
        round_running = false;
        trial_running = false;
        Time.timeScale = 1.0f;

        if (to_menu_after_trial)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }


    public virtual void ResetTrial()
    {
        total_time_for_trial = 0;
        time_for_current_round = 0;
    }


    public virtual void GoalScored()
    {

    }


    public void CreateTextFile()
    {
        string[] results = new string[round_results.Count];
        for (int x = 0; x < round_results.Count; x++)
        {
            results[x] = round_results[x].ToString() + round_results[x].ExtrasToString();
        }
        string path = Application.dataPath + "/" + text_file_name;    // Application.persistentDataPath

        string text = "";

        // If file doesn't exist, add the header line
        if (!System.IO.File.Exists(path))
            text = round_results[round_results.Count - 1].FieldNames() + round_results[round_results.Count - 1].ExtrasFieldNames() + "\n" + text;  // Top line contains column names
        else
            text += "\n";

        text += string.Join(Environment.NewLine, results);
        // Append results onto end of file
        Debug.Log("Saving results to: " + path + ", " + text, this.gameObject);
        System.IO.File.AppendAllText(path, text);
    }


    public virtual void Update () 
	{
        if (trial_running)
        {
            total_time_for_trial += Time.deltaTime;

            if (round_running)
            {
                time_for_current_round += Time.deltaTime;

                if (enforce_time_limit)
                {
                    if (time_for_current_round >= time_limit)
                        NextRound();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && !trial_running && ScoreManager.score_manager.players.Count == number_players_required)
                StartTrial();
        }
	}


    Rect gui_rect = new Rect(Screen.width - 200, Screen.height - 50, 200, 50);
    private void OnGUI()
    {
        if (round_timer != null)
            round_timer.text = "" + (int)(time_limit - time_for_current_round);

        if (ScoreManager.debug_view || (Application.isEditor))
        {
            if (Time.timeScale <= 0 && !trial_running)
                return;

            string display_string = "";
            display_string += "Round: " + (current_round + 1);

            if (enforce_time_limit)
                display_string += "\nTime remaining: " + (int)(time_limit - time_for_current_round);

            GUI.Label(gui_rect, display_string);
        }
    }
}
