using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{


    void Start () {
		
	}


    void Update () {
		
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
        /*
        SpaceInvaders.space_invaders.current_round_record.num_invaders_hit++;
        SpaceInvaders.space_invaders.current_round_record.time_of_invader_hit.Add(SpaceInvaders.space_invaders.time_for_current_round);
        SpaceInvaders.space_invaders.current_round_record.x_pos_of_invader_at_hit.Add(invader.transform.position.x);
        SpaceInvaders.space_invaders.current_round_record.x_pos_of_bullet_at_invader_hit.Add(invader.transform.position.x);
        */
        Destroy(invader);

        // Record this!
        ScoreManager.score_manager.BlueScored(1);

        DestroyThisBullet();
    }

    public void HitPlayer(GameObject player)
    {
        // Destroy player
        player.GetComponent<PlayerSpaceInvaders>().GotHit(this);

        DestroyThisBullet();
    }

    public void HitWall()
    {
        DestroyThisBullet();
    }
    public void DestroyThisBullet()
    {
        BulletPooler.DespawnThisBullet(this.gameObject);
    }
}
