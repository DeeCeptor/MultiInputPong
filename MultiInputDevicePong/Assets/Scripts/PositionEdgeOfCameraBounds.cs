using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Corner {  Top_Left, Top_Right, Bottom_Left, Bottom_Right, Top, Left, Bottom, Right }

public class PositionEdgeOfCameraBounds : MonoBehaviour 
{
    public Corner which_corner;

	void Awake () 
	{
        // Bottom left
        Vector3 minScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        // Top right
        Vector3 maxScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        switch (which_corner)
        {
            case Corner.Top_Left:
                this.transform.position = new Vector2(minScreenBounds.x, maxScreenBounds.y);
                break;
            case Corner.Top_Right:
                this.transform.position = maxScreenBounds;
                break;
            case Corner.Bottom_Left:
                this.transform.position = minScreenBounds;
                break;
            case Corner.Bottom_Right:
                this.transform.position = new Vector2(maxScreenBounds.x, minScreenBounds.y);
                break;
            case Corner.Top:
                this.transform.position = new Vector2(this.transform.position.x, maxScreenBounds.y);
                break;
            case Corner.Left:
                this.transform.position = new Vector2(minScreenBounds.x, this.transform.position.y);
                break;
            case Corner.Bottom:
                this.transform.position = new Vector2(this.transform.position.x, minScreenBounds.y);
                break;
            case Corner.Right:
                this.transform.position = new Vector2(maxScreenBounds.x, this.transform.position.y);
                break;
        }
    }


    void Update () 
	{
		
	}
}
