using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GlobalSettings
{
    public enum Input_Device_Type { Mouse, Drawing_Tablet, Touchscreen, Controller };
    public static Input_Device_Type current_input_device = Input_Device_Type.Mouse;
    public static Queue<Input_Device_Type> order_of_device_types = new Queue<Input_Device_Type>();
    public static string device_orderings = "None";     // Record this to store in CSV
    public static int num_devices_switched = 0;     // 1 is the first device, 2 is second, etc. Be sure to reset this if if head back to the Trial scene

    // 0 means no input delay. 5 means 5 fixedupdates will pass before the input is applied
    public static int InputDelayFrames
    {
        get { return input_delay; }
        set
        {
            if (value != InputDelayFrames)
            {
                // Reset the input queues of all players because the inputqueue length has changed
                foreach (Player p in ScoreManager.score_manager.players)
                {
                    if (p.GetComponent<SingleMouseMovement>() != null)
                        p.GetComponent<SingleMouseMovement>().ResetInputQueues();
                    if (p.GetComponent<AbsoluteMovement>() != null)
                        p.GetComponent<AbsoluteMovement>().ResetInputQueues();
                    if (p.GetComponent<QueueMouseClicks>() != null)
                        p.GetComponent<QueueMouseClicks>().ResetInputQueues();
                    if (p.GetComponent<AimingMovement>() != null)
                        p.GetComponent<AimingMovement>().ResetInputQueues();
                }
                Debug.Log("Changing local latency to (ms): " + input_delay);
            }
            input_delay = value;
        }
    } 
    private static int input_delay = 0;

    public const int input_delay_increment = 10;
    public const int ms_per_second = 1000;

    public static int[] participant_ids = new int[8];


    // Returns true if a valid participant ID has been set for this player number
    public static bool ValidParticipantID(int player_num)
    {
        return participant_ids != null && participant_ids.Length >= player_num ? true : false;
    }
    // Returns a participant ID. Returns -1 if no ID was found
    public static int GetParticipantId(int player_num)
    {
        if (participant_ids != null && participant_ids.Length >= player_num)
            return participant_ids[player_num];
        else
            return -1;
    }
}


public class LocalSettings : MonoBehaviour 
{
	void Start () 
	{
        QualitySettings.vSyncCount = 0;    // Limits it to ~60fps
        Application.targetFrameRate = 120;
        Debug.Log("Setting frame rate to: " + Application.targetFrameRate + ", and vSync to: " + QualitySettings.vSyncCount);
    }

    void Update()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu")
            return;

        //Debug.Log(Application.targetFrameRate + " " + QualitySettings.vSyncCount);
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            GlobalSettings.InputDelayFrames += GlobalSettings.input_delay_increment;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
            GlobalSettings.InputDelayFrames = Mathf.Max(GlobalSettings.InputDelayFrames - GlobalSettings.input_delay_increment, 0);    // Can't reduce input delay any more
    }
}
