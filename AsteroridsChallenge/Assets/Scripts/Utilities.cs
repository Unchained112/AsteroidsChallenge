using UnityEngine;

public class Utilities
{
    private static Vector2 boundary = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));

    public static Vector2 WrapPosition(Vector2 pos)
    {
        Vector2 wraped_pos = pos;
        if (pos.x > boundary.x)
        {
            wraped_pos = new Vector2(-boundary.x, pos.y);
        }
        if (pos.x < -boundary.x)
        {
            wraped_pos = new Vector2(boundary.x, pos.y);
        }
        if (pos.y > boundary.y)
        {
            wraped_pos = new Vector2(pos.x, -boundary.y);
        }
        if (pos.y < -boundary.y)
        {
            wraped_pos = new Vector2(pos.x, boundary.y);
        }
        return wraped_pos;
    }

    public static Vector2 GetBoundary() { return boundary; }
}
