using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class PositionBoundaryOffscreen : MonoBehaviour
{
    BoxCollider2D box;
    public enum Side_Of_Screen { Left, Right, Top, Bottom };
    public Side_Of_Screen Side;

    void Start ()
    {
        box = this.GetComponent<BoxCollider2D>();
        AdjustPositionByScreen();
    }


    public void AdjustPositionByScreen()
    {
        Vector3 minScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        Vector2 new_position = Vector2.zero;
        switch (Side)
        {
            case Side_Of_Screen.Left:
                new_position = new Vector2(minScreenBounds.x - box.bounds.extents.x, 0);
                break;
            case Side_Of_Screen.Right:
                new_position = new Vector2(maxScreenBounds.x + box.bounds.extents.x, 0);
                break;
            case Side_Of_Screen.Bottom:
                new_position = new Vector2(0, minScreenBounds.y - box.bounds.extents.y);
                break;
            case Side_Of_Screen.Top:
                new_position = new Vector2(0, maxScreenBounds.y + box.bounds.extents.y);
                break;
        }

        this.transform.position = new_position;
    }
}
