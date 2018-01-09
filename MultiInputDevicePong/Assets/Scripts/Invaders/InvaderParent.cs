using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Overall script for movement of all invaders. 
// Moves invaders left and right, and checks if there's no more children (invaders are destroyed) the round is over
public class InvaderParent : MonoBehaviour
{
    public static InvaderParent invader_parent;

    public Vector3 start_position;

	void Awake ()
    {
        invader_parent = this;
        start_position = this.transform.position;
    }


    void Update ()
    {
		if (this.transform.childCount <= 0)
        {
            AllInvadersDefeated();
        }

        // Move left and right
        this.transform.position = start_position + Vector3.right * Mathf.Sin(Time.time);
	}


    public void AllInvadersDefeated()
    {
        Debug.Log("All invaders defeated");
    }
}
