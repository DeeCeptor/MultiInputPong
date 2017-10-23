using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class BallMovement : NetworkBehaviour
{

    // Create a network message to sync only the ball
    public class BallMessage : MessageBase
    {
        public Vector3 position;
        public Vector2 velocity;
    }


    public void SetupClient()
    {
        
    }

    public void OnConnectedToServer()
    {
        
    }


    void Start () 
	{
		
	}
	

	void Update () 
	{
		
	}
}
