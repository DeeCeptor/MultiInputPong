using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RavingBots.MultiInput;
using System;
using System.Linq;

public class MultiMouseManager : MonoBehaviour 
{
    public static MultiMouseManager mouse_manager;
    public bool players_can_join = true;
    public GameObject player_prefab;

    private InputState _inputState;

    private List<IDevice> joined_devices = new List<IDevice>();

    private static readonly IList<InputCode> InterestingAxes;

    // we want to check all axes *but* few analog ones
    // that might trigger false positives or be otherwise
    // inconvenient (like mouse movement or gamepad sticks)
    static MultiMouseManager()
    {
        InterestingAxes = InputStateExt
            .AllAxes
            .Where(axis => !IsUninteresting(axis))
            .ToList();
    }

    private static bool IsUninteresting(InputCode axis)
    {
        switch (axis)
        {
            case InputCode.MouseLeft:
                return false;
            default:
                return true;
        }
    }

    void Awake () 
	{
        mouse_manager = this;
        _inputState = GetComponent<InputState>();

    }


    void FixedUpdate () 
	{
        if (!players_can_join)
            return;

        IDevice new_device;
        InputCode code;
        if (FindInput(out new_device, out code) && new_device != null)
        {
            SpawnPlayer(new_device);
        }
        
	}
    public void SpawnPlayer(IDevice device)
    {
        // Add the new device
        joined_devices.Add(device);

        // Join a player
        GameObject go = (GameObject)(Instantiate(player_prefab));

        // Figure out name by checking plaer id's
        int participant_id = GlobalSettings.GetParticipantId(joined_devices.Count - 1);
        if (participant_id != 0)
        {
            go.GetComponent<Player>().player_id = participant_id;
            go.transform.name = "" + participant_id;
        }
        else
        {
            go.GetComponent<Player>().player_id = joined_devices.Count;
            go.transform.name = "" + joined_devices.Count;
        }

        go.GetComponent<Player>().input = device;
    }


    public bool IsAssigned(IDevice device)
    {
        return joined_devices.Contains(device);
    }


    private bool FindInput(out IDevice outDevice, out InputCode outCode)
    {
        Func<IDevice, IVirtualAxis, bool> predicate = (device, axis) => {
            if (IsAssigned(device))
            {
                return false;
            }

            // ReSharper disable once InvertIf
            if (!axis.Code.IsMouse())
            {
                return false;
            }

            return axis.IsDown;
        };

        IVirtualAxis outAxis;
        if (_inputState.FindFirst(out outDevice, out outAxis, predicate, InterestingAxes))
        {
            outCode = outAxis.Code;
            return true;
        }

        outDevice = null;
        outCode = InputCode.None;
        return false;
    }
}
