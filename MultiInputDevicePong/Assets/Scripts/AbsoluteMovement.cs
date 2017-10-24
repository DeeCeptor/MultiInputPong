using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RavingBots.MultiInput;

[RequireComponent(typeof(Player))]
public class AbsoluteMovement : MonoBehaviour 
{
    Player player;
    Rigidbody2D physics;

    AudioSource kick_sound;

    // Stores inputs. Used for adding input delay
    Queue<Vector2> input_queue = new Queue<Vector2>();

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

    private const float kick_force_multiplier = 2000f;//2000f

    Vector2 cur_input;  // From the current fixedupdate frame

    public bool adjust_sensitivity = false;


    void Awake () 
	{
        player = this.GetComponent<Player>();
        physics = this.GetComponent<Rigidbody2D>();
        kick_sound = this.GetComponent<AudioSource>();

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


    void FixedUpdate () 
	{
        KinematicMovement();
    }
    public void KinematicMovement()
    {
        // Using the drawing tablet

        
        ///////////////////////////////////////////////////////////
        // INPUT


        /*
        // Adjust sensitivity
        float mouse_wheel = player.input[InputCode.MouseWheel].Value;
        if (adjust_sensitivity && mouse_wheel != 0)
        {
            Adjust_Sensitivity(mouse_wheel);
        }
        */


        // Get current device input
        cur_input = Vector2.zero;
        if (allow_x_movement)
        {
            cur_input.x = Input.mousePosition.x;
        }
        if (allow_y_movement)
            cur_input.y = Input.mousePosition.y;

        // Place input in our queue
        input_queue.Enqueue(cur_input);
        if (input_queue.Count > GlobalSettings.InputDelayFrames)
        {
            cur_input = input_queue.Dequeue();
        }
        else
            cur_input = Vector2.zero;



        ////////////////////////////////////////////////////////
        // Don't move if there's no input
        if (cur_input == Vector2.zero)
            return;

        Vector3 potential_position = Camera.main.ScreenToWorldPoint( cur_input );

        if (!allow_y_movement)
            potential_position.y = this.transform.position.y;
        if (!allow_x_movement)
            potential_position.x = this.transform.position.x;

        
        // Keep the player within view of the screen
        potential_position = new Vector2(
            Mathf.Clamp(potential_position.x, CameraRect.arena_rect.xMin, CameraRect.arena_rect.xMax),
            Mathf.Clamp(potential_position.y, CameraRect.arena_rect.yMin, CameraRect.arena_rect.yMax));

        // Add force to move us where we should be
        //physics.AddForce(potential_position);
        physics.MovePosition(potential_position);
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

    }


    public void ResetInputQueues()
    {
        input_queue.Clear();
    }
}
