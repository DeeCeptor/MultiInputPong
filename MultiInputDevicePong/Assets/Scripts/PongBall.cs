using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBall : MonoBehaviour 
{
    Rigidbody2D physics;
    public float max_x_angle = 0.75f;
    float bounce_score_cooldown = 0.1f;
    public float time_of_last_collision;


	void Start () 
	{
        physics = this.GetComponent<Rigidbody2D>();

        // Start the ball moving
        physics.velocity = new Vector2(0.5f, 0.5f) * Ball.ball.max_speed;
    }


    void Update () 
	{
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HitPlayer(collision);
        }
    }
    public void HitPlayer(Collider2D collision)
    {
        if (TimeSinceLastCollision() < bounce_score_cooldown || !Trial.trial.trial_running)
            return;

        collision.gameObject.GetComponent<AudioSource>().Play();
        float contact_x = this.transform.position.x;
        Vector2 new_dir = Vector2.zero;

        // Add a score
        if (Trial.trial is SoloPong)
        {
            SoloPong pong = (SoloPong)Trial.trial;
            pong.current_round_record.total_bounces += 1;
            ScoreManager.score_manager.blue_score += 1;

            new_dir = new_dir = HitPlayerPaddle(contact_x, collision);
        }
        else if (Trial.trial is TeamPong)
        {
            TeamPong pong = (TeamPong)Trial.trial;
            pong.current_round_record.total_bounces += 1;
            ScoreManager.score_manager.blue_score += 1;

            if (collision.gameObject.GetComponent<Player>().player_id == ScoreManager.score_manager.players[0].player_id)
                pong.current_round_record.player_1_bounces++;
            else
                pong.current_round_record.player_2_bounces++;

            new_dir = HitPlayerPaddle(contact_x, collision);
            Debug.Log("TeamPong");

            if (WillHitOtherWall(new_dir))
                new_dir.x = -new_dir.x;
        }
        else if (Trial.trial is SoloPongLikeTeamPong)
        {
            SoloPongLikeTeamPong pong = (SoloPongLikeTeamPong)Trial.trial;
            pong.current_round_record.total_bounces += 1;

            // Check if hit player, or hit wall
            Player p = collision.gameObject.GetComponent<Player>();
            if (p != null && p.player_id == ScoreManager.score_manager.players[0].player_id)
            {
                // Hit player
                pong.RecordPlayerBounce();

                ScoreManager.score_manager.blue_score += 1;
                new_dir = HitPlayerPaddle(contact_x, collision);
                //Debug.Log("SoloPongLikeTeamPong hit player " + collision.gameObject.name);
            }
            else
            {
                // Hit wall
                pong.current_round_record.wall_bounces++;

                // Get a random new direction
                float signed_normalized_difference = Random.Range(-1.0f, 1.0f);
                new_dir = new Vector2(signed_normalized_difference * max_x_angle, -Mathf.Sign(physics.velocity.y) * 0.5f).normalized;
                //Debug.Log("SoloPongLikeTeamPong hit wall " + collision.gameObject.name);
            }

            if (WillHitOtherWall(new_dir))
                new_dir.x = -new_dir.x;
        }

        RecordBounce("Hit something.");
        physics.velocity = new_dir * Ball.ball.max_speed;
    }
    public Vector2 HitPlayerPaddle(float contact_x, Collider2D collision)
    {
        // Hit the player paddle, figure out where we contacted the paddle
        float difference = Mathf.Abs(Mathf.Abs(collision.gameObject.transform.position.x) - Mathf.Abs(contact_x));
        float sign = collision.gameObject.transform.position.x > contact_x ? -1.0f : 1.0f;
        float signed_difference = sign * difference;
        float signed_normalized_difference = Mathf.Clamp(signed_difference / (collision.gameObject.transform.localScale.x / 2), -1.0f, 1.0f);

        // https://gamedev.stackexchange.com/questions/4253/in-pong-how-do-you-calculate-the-balls-direction-when-it-bounces-off-the-paddl
        // Add slight angle to ball based on where we collided with paddle
        return new Vector2(signed_normalized_difference * max_x_angle, -Mathf.Sign(physics.velocity.y) * 0.5f).normalized;
    }
    public bool WillHitOtherWall(Vector2 new_direction)
    {
        // Do a raycast see if this intersects left or right wall
        RaycastHit2D r = Physics2D.Raycast(this.transform.position, new_direction, 30, LayerMask.GetMask(new string[] { "Walls" }));

        // If it does, simply inverse X direction
        if (r.transform != null && (r.transform.name.Contains("Left") || r.transform.name.Contains("Right")))
        {
            Debug.LogWarning("Changing dir, hit " + r.transform.name, this.gameObject);
            return true;
        }
        else
            return false;
    }


    public float TimeSinceLastCollision()
    {
        return Time.time - time_of_last_collision;
    }


    public void RecordBounce(string prepend_msg)
    {
        if (!prepend_msg.Contains("Wall"))
            Debug.Log(prepend_msg + " Time since last collision " + (Time.time - time_of_last_collision), this.gameObject);
        time_of_last_collision = Time.time;
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            RecordBounce("Hit wall.");
        }
    }
    /*
    public void HitPlayer(Collision2D collision)
    {
        Debug.Log("Hit player" + collision.gameObject.name);
        if (time_since_last_bounce < bounce_score_cooldown)
            return;

        if (Trial.trial.trial_running && time_since_last_bounce >= bounce_score_cooldown)
        {
            time_since_last_bounce = 0;

            // Add a score
            if (Trial.trial.trial_running && Trial.trial is SoloPong)
            {
                SoloPong pong = (SoloPong)Trial.trial;
                pong.current_round_record.paddle_bounces += 1;

                ScoreManager.score_manager.blue_score += 1;
            }
            else if (Trial.trial.trial_running && Trial.trial is TeamPong)
            {
                TeamPong pong = (TeamPong)Trial.trial;
                pong.current_round_record.total_bounces += 1;

                ScoreManager.score_manager.blue_score += 1;

                if (collision.gameObject.GetComponent<Player>().player_id == ScoreManager.score_manager.players[0].player_id)
                {
                    pong.current_round_record.player_1_bounces++;
                }
                else
                    pong.current_round_record.player_2_bounces++;
            }
        }



        float contact_x = collision.contacts[0].point.x;
        Debug.Log(collision.contacts[0].point.x + " , position " + this.transform.position.x);
        // Figure out where we contacted the paddle
        float difference = Mathf.Abs(Mathf.Abs(collision.gameObject.transform.position.x) - Mathf.Abs(contact_x));
        float sign = collision.gameObject.transform.position.x > contact_x ? -1.0f : 1.0f;
        float signed_difference = sign * difference;
        float signed_normalized_difference = Mathf.Clamp(signed_difference / (collision.gameObject.transform.localScale.x / 2), -1.0f, 1.0f);

        // https://gamedev.stackexchange.com/questions/4253/in-pong-how-do-you-calculate-the-balls-direction-when-it-bounces-off-the-paddl
        // Add slight angle to ball based on where we collided with paddle
        Vector2 new_dir = new Vector2(signed_normalized_difference * max_x_angle, 0.5f).normalized;
        physics.velocity = new_dir * Ball.ball.max_speed;

        collision.gameObject.GetComponent<AudioSource>().Play();
    }
    */


    public float DistanceFromBall(Vector2 position)
    {
        return Mathf.Abs(Ball.ball.transform.position.x - position.x);
    }
}
