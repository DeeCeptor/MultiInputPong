using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FollowObject : MonoBehaviour
{
    public Transform object_to_follow;
    public float lerp_rate = 0.9f;   // Between 0 and 1, closer to 1 the faster it lerps to target
    public bool player_object = false;

	void Start ()
    {
        if (object_to_follow == null)
        {
            Debug.LogError("Object to follow is null", this.gameObject);
            Destroy(this.gameObject);
            return;
        }

        this.transform.localScale = object_to_follow.transform.localScale;

        // Set same sprite, disable original's sprite
        this.GetComponent<SpriteRenderer>().sprite = object_to_follow.GetComponentInChildren<SpriteRenderer>().sprite;
        object_to_follow.GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
	

    public void Setup(Transform obj_to_follow, float lerp_speed, bool is_player)
    {
        lerp_rate = lerp_speed;
        object_to_follow = obj_to_follow;
        this.transform.name = obj_to_follow.name + " Graphics";
        player_object = is_player;
    }


	void Update ()
    {
        if (!object_to_follow)
        {
            Destroy(this.gameObject);
            return;
        }

        // Set position and rotation based on object we're following
        float lerp_speed = lerp_rate;
        /*
        // If this is a player, and we're close to the ball, lerp more quickly
        if (player_object && Vector2.Distance(object_to_follow.transform.position, Ball.ball.transform.position) < 1f)
            lerp_speed = 0.99f;
        */
        // Use delta time? Or fixed update?
        this.transform.position = Vector3.Lerp(this.transform.position, object_to_follow.transform.position, lerp_speed);
        //this.transform.position = Vector3.Lerp(this.transform.position, object_to_follow.transform.position, lerp_rate * Time.deltaTime);
        this.transform.rotation = object_to_follow.transform.rotation;
	}
}
