using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles shooting bullets
public class PlayerSpaceInvaders : MonoBehaviour
{
    public GameObject bullets_to_use;


	void Start () {
		
	}


    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ShootBullet();
    }

    public void ShootBullet()
    {
        // Record a bullet was shot
        GameObject new_bullet =  Instantiate(bullets_to_use, this.transform.position, Quaternion.identity);
        new_bullet.GetComponent<Bullet>();

        new_bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 5, 0);
    }
}
