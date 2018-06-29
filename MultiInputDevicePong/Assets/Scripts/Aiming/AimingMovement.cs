using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RavingBots.MultiInput;

[RequireComponent(typeof(Player))]
public class AimingMovement : MonoBehaviour 
{
    public static AimingMovement aiming_movement;

    Player player;
    Rigidbody2D physics;

    AudioSource kick_sound;

    // Stores inputs. Used for adding input delay
    Queue<Vector2> input_queue = new Queue<Vector2>();
    Queue<bool> click_queue = new Queue<bool>();

    // How can the player move?
    public bool allow_x_movement = true;
    public bool allow_y_movement = true;

    // What sensitivity are we currently using? Between min_sensitivity and max_sensitivity, 1 starts at 1
    // Can be altered using mouse wheel
    public float sensitivity = 1f;
    private const float min_sensitivity = 0.2f;
    private const float max_sensitivity = 2.0f;
    private const float sensitivity_increment = 0.1f;
    private const float speed_factor = 0.01f;   // Multiplied by sensitivity to get how far we move this frame
    private const float relative_speed_sensitivity = 8.0f;

    private const float kick_force_multiplier = 2000f;//2000f


    public bool adjust_sensitivity = false;

    float halved_player_width = 0;

    public LayerMask click_layermask;



    void Awake () 
	{
        aiming_movement = this;
        player = this.GetComponent<Player>();
        physics = this.GetComponent<Rigidbody2D>();
        kick_sound = this.GetComponent<AudioSource>();

        EdgeCollider2D collider = this.GetComponent<EdgeCollider2D>();
        if (collider != null)
        {
            halved_player_width = this.transform.localScale.x / 2f;
        }
        /*
        if (allow_x_movement && !allow_y_movement)
            physics.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        */
    }


    float rotation_target;

    public void Adjust_Sensitivity(float adjustment)
    {
        sensitivity = Mathf.Clamp(sensitivity + (adjustment * sensitivity_increment), min_sensitivity, max_sensitivity);
        Debug.Log("New sensitivity: " + sensitivity);
    }


    public void Clicked()
    {
        // Raycast to check if we hit a target
        Collider2D hit = Physics2D.OverlapPoint(this.transform.position, click_layermask);
        if (hit != null)
        {
            Debug.Log("Clicked on " + hit.transform.name);

            if (hit.tag == "Invader")
            {
                // Report a successful click, start next trial
                TwoDAimingTrial.aiming_trial.ClickedOnTarget(hit.gameObject, this.gameObject);
            }
            else if (hit.tag == "Net")
            {
                TwoDAimingTrial.aiming_trial.ReturnedToMiddle();
            }
        }
        if (Trial.trial.trial_running)
            TwoDAimingTrial.aiming_trial.current_round_record.num_clicks++;
    }

    int clicks_this_update = 0;
    Vector2 mouse_pos;
    void FixedUpdate () 
	{
        KinematicMovement();
        UpdateClicks();
    }
    private void Update()
    {
        clicks_this_update = 0;     // 11 clicks during this update?
        mouse_pos = Input.mousePosition;
        if (TwoDAimingTrial.aiming_trial.round_running)
        {
            // Get the current position
            Vector3 current_pos = new Vector3(this.transform.position.x, this.transform.position.y, 1);
            TwoDAimingTrial.aiming_trial.current_round_record.travel_path_vector.Add(current_pos);

            // Record it for posterity's sake
            TwoDAimingTrial.aiming_trial.current_round_record.travel_path_x.Add(current_pos.x);
            TwoDAimingTrial.aiming_trial.current_round_record.travel_path_y.Add(current_pos.y);
        }
    }
    public void UpdateClicks()
    {
        QueueCurrentClicks();

        bool cur_clicked = false;
        // Dequeue input we should use this frame
        if (click_queue.Count > GlobalSettings.InputDelayFrames)
        {
            cur_clicked = click_queue.Dequeue();
        }

        // Check if we should issue a click this frame
        if (cur_clicked)
            Clicked();
    }
    public void QueueCurrentClicks()
    {
        bool clicked_this_frame = false;

        // Record any clicks that happened this frame
        if (clicks_this_update == 0)
        {
            // Check what device we're using
            if (GlobalSettings.current_input_device == GlobalSettings.Input_Device_Type.Controller)
            {
                if (Input.GetButtonDown("Submit"))
                {
                    clicks_this_update++;
                    clicked_this_frame = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    
                    clicks_this_update++;
                    clicked_this_frame = true;
                }
            }
        }

        click_queue.Enqueue(clicked_this_frame);
    }
    public void KinematicMovement()
    {
        QueueCurrentInput();

        Vector2 cur_input = Vector2.zero;
        // Dequeue input we should use this frame
        if (input_queue.Count > GlobalSettings.InputDelayFrames)
        {
            cur_input = input_queue.Dequeue();
        }

        ////////////////////////////////////////////////////////
        // Don't move if there's no input
        if (cur_input == Vector2.zero)
            return;

        Vector3 potential_position = MoveToNewPosition(cur_input);

        if (!allow_y_movement)
            potential_position.y = this.transform.position.y;
        if (!allow_x_movement)
            potential_position.x = this.transform.position.x;

        // Keep the player within view of the screen
        potential_position = new Vector2(
            Mathf.Clamp(potential_position.x, CameraRect.arena_rect.xMin + halved_player_width, 
                CameraRect.arena_rect.xMax - halved_player_width),
            Mathf.Clamp(potential_position.y, CameraRect.arena_rect.yMin + halved_player_width, 
                CameraRect.arena_rect.yMax - halved_player_width));

        // Add force to move us where we should be
        //physics.AddForce(potential_position);
        //physics.MovePosition(potential_position);
        //physics.position = potential_position;
        this.transform.position = potential_position;
    }
    // Takes the current input, and queues it for later use
    public void QueueCurrentInput()
    {
        Vector2 cur_input = Vector2.zero;

        // Check what device we're using
        // RELATIVE MOVEMENT/POSITIONING:   CONTROLLER JOYSTICK
        if (GlobalSettings.current_input_device == GlobalSettings.Input_Device_Type.Controller)
        {
            // create magnitude deadzone by limiting the range of stick from 0 to set gravity deadzone 
            Vector2 stick_input = new Vector2(Input.GetAxis("ControllerX"), Input.GetAxis("ControllerY"));
            if (stick_input.magnitude < 0.19f)//configurator.gravity_deadzone)
            {
                stick_input.x = 0;
                stick_input.y = 0;
            }

            if (allow_x_movement)
            {
                cur_input.x = stick_input.x * Time.fixedDeltaTime * relative_speed_sensitivity;
            }
            if (allow_y_movement)
                cur_input.y = stick_input.y * Time.fixedDeltaTime * relative_speed_sensitivity;
        }
        // DIRECT MOVEMENT/POSITIONING:     MOUSE/TOUCHSCREEN/DRAWING TABLET
        else
        {
            if (allow_x_movement)
            {
                cur_input.x = Input.mousePosition.x;
            }
            if (allow_y_movement)
                cur_input.y = Input.mousePosition.y;
        }

        // Place input in our queue
        input_queue.Enqueue(cur_input);
    }
    // Must either use absolute positioning (mouse) or relational positioning (controller joystick)
    public Vector2 MoveToNewPosition(Vector2 cur_input)
    {
        Vector2 new_pos;
        // RELATIVE MOVEMENT (controller)
        if (GlobalSettings.current_input_device == GlobalSettings.Input_Device_Type.Controller)
        {
            new_pos = (Vector2)this.transform.position + cur_input;
        }
        // ABSOLUTE POSITION/MOVEMENT   MOUSE/TOUCHSCREEN/DRAWING TABLET
        else
        {
            new_pos = Camera.main.ScreenToWorldPoint(cur_input);
        }

        return new_pos;
    }


    public void EnableCollisions(bool enable)
    {
        this.GetComponent<Collider2D>().enabled = enable;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Invader")
        {
            TwoDAimingTrial.aiming_trial.current_round_record.target_reentry++;
            Debug.Log("Entered target for " + TwoDAimingTrial.aiming_trial.current_round_record.target_reentry + " time");
        }
    }


    public void ResetInputQueues()
    {
        input_queue.Clear();
        click_queue.Clear();
    }
}
