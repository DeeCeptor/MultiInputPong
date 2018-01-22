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
            DestroyThisBullet();
    }

    public void HitInvader(GameObject invader)
    {
        // Record some stuff

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

    public void DestroyThisBullet()
    {
        BulletPooler.DespawnThisBullet(this.gameObject);
    }
}
