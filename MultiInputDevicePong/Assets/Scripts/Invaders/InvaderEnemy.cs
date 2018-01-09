using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Specific invader enemy. Checks for collision with player bullet
public class InvaderEnemy : MonoBehaviour
{


	void Start ()
    {
		
	}


    void Update ()
    {
		
	}

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player Bullet")
            HitByPlayerBullet(collision.gameObject);
    }

    public void HitByPlayerBullet(GameObject murdering_bullet)
    {
        // Record some stuff

        Destroy(murdering_bullet);
        Destroy(this.gameObject);
    }
    */
}
