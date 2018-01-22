using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Overall script for movement of all invaders. 
// Moves invaders left and right, and checks if there's no more children (invaders are destroyed) the round is over
public class InvaderParent : MonoBehaviour
{
    public static InvaderParent invader_parent;
    public Vector3 start_position;
    CameraRect camera_rect;
    int initial_child_count;


	void Awake ()
    {
        invader_parent = this;
        start_position = this.transform.position;
        initial_child_count = this.transform.childCount;
    }
    private void Start()
    {

    }


    float age;
    void Update ()
    {
		if (this.transform.childCount <= 0)
        {
            AllInvadersDefeated();
        }


        // Move left and right
        this.transform.position = start_position + (Vector3.right * Mathf.Sin(age)) * ((float)initial_child_count - CameraRect.camera_rect.width) / 2f;

        age += Time.deltaTime;
    }


    public void AllInvadersDefeated()
    {
        Debug.Log("All invaders defeated");

        // Finish round
        SpaceInvaders.space_invaders.NextRound();

        Destroy(this.gameObject);
    }
}
