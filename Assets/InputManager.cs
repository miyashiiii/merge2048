using UnityEngine;

public class InputManager
{
    private Vector3 _touchStartPos;
    private Vector3 _touchEndPos;



    public Direction GetInput()
    {
        Debug.Log("game status: " + GameManager.Status);

        Direction direction = null;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _touchStartPos = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y,
                Input.mousePosition.z);
        }

        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _touchEndPos = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y,
                Input.mousePosition.z);
            direction = GetFlickDirection();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Direction.Up;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Direction.Down;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Direction.Left;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Direction.Right;
        }

        // Debug.Log(direction);
        return direction;
    }


    Direction GetFlickDirection()
    {
        var directionX = _touchEndPos.x - _touchStartPos.x;
        var directionY = _touchEndPos.y - _touchStartPos.y;

        if (Mathf.Abs(directionY) < Mathf.Abs(directionX))
        {
            if (30 < directionX)
            {
                return Direction.Right;
            }

            if (-30 > directionX)
            {
                return Direction.Left;
            }
        }
        else if (Mathf.Abs(directionX) < Mathf.Abs(directionY))
        {
            if (30 < directionY)
            {
                return Direction.Up;
            }

            if (-30 > directionY)
            {
                return Direction.Down;
            }
        }

        return null;
    }
}