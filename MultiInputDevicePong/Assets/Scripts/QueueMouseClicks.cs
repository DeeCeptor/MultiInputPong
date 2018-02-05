using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueMouseClicks : MonoBehaviour
{
    Queue<bool> left_mouse_input_queue = new Queue<bool>();
    public bool cur_left_mouse_held_down = false;


    void Start ()
    {
		
	}


    void FixedUpdate ()
    {
        PollLeftMouse();
    }


    public void PollLeftMouse()
    {
        // Place input in our queue
        left_mouse_input_queue.Enqueue(Input.GetMouseButton(0));
        if (left_mouse_input_queue.Count > GlobalSettings.InputDelayFrames)
        {
            cur_left_mouse_held_down = left_mouse_input_queue.Dequeue();
        }
        else
            cur_left_mouse_held_down = false;
    }


    public void ResetInputQueues()
    {
        left_mouse_input_queue.Clear();
    }
}
