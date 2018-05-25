using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public bool fixed_up = false;

	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
        if (!fixed_up)
            Move();
	}
    void FixedUpdate()
    {
        if (fixed_up)
            Move();
    }

    public void Move()
    {
        this.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
