using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool shot_by_player = false;
    public float time_of_creation;


    void Start () {
		
	}


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Invader")
            HitInvader(collision.gameObject);
        else if (collision.gameObject.tag == "Player")
            HitPlayer(collision.gameObject);
        else if (collision.gameObject.tag == "Wall")
            HitWall();
    }


    public void HitInvader(GameObject invader)
    {
        // Record some stuff
        SpaceInvaders.space_invaders.current_round_record.num_invaders_hit++;
        SpaceInvaders.space_invaders.current_round_record.time_of_invader_hit.Add(SpaceInvaders.space_invaders.time_for_current_round);
        SpaceInvaders.space_invaders.current_round_record.x_pos_of_invader_at_hit.Add(invader.transform.position.x);
        SpaceInvaders.space_invaders.current_round_record.x_pos_of_bullet_at_invader_hit.Add(this.transform.position.x);
        InvaderParent.invader_parent.point_sound.Play();

        Destroy(invader);

        // Record this!
        ScoreManager.score_manager.BlueScored(1);

        DestroyThisBullet();
    }

    public void HitPlayer(GameObject player)
    {
        // Destroy player
        player.GetComponent<PlayerSpaceInvaders>().GotHit(this);
        float lifespan = Time.time - time_of_creation;
        Debug.Log("Lifespan of bullet: " + lifespan + " seconds");

        DestroyThisBullet();
    }
    public void HitWall()
    {
        if (shot_by_player)
            SpaceInvaders.space_invaders.current_round_record.player_missed_shots++;

        DestroyThisBullet();
    }
    public void DestroyThisBullet()
    {
        if (shot_by_player)
            SpaceInvaders.space_invaders.current_round_record.num_finished_shots++;

        float lifespan = Time.time - time_of_creation;
        shot_by_player = false;
        BulletPooler.DespawnThisBullet(this.gameObject);
    }
}
