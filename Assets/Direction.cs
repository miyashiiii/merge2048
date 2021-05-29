public abstract class Direction
{
    public virtual Board.ConvertBoard ConvertFunc => Util.NoRotate;
    public virtual Board.ConvertBoard ReverseFunc => Util.NoRotate;

    public abstract (float, float) Get2dDistance(float distance);
    public abstract (int, int) GETNext(int moveSquare, int row, int col);
    public static Up up = new Up();
    public static Down down = new Down();
    public static Left left = new Left();
    public static Right right = new Right();
}

public class Up : Direction
{
    public override Board.ConvertBoard ConvertFunc => Util.RotateBoardAnticlockwise;
    public override Board.ConvertBoard ReverseFunc => Util.RotateBoardClockwise;

    public override (float, float) Get2dDistance(float distance)
    {
        return (0, distance);
    }

    public override (int, int) GETNext(int moveSquare, int row, int col)
    {
        return (row - moveSquare, col);
    }

    public override string ToString()
    {
        return "up";
    }
}

public class Down : Direction
{
    public override Board.ConvertBoard ConvertFunc => Util.RotateBoardClockwise;
    public override Board.ConvertBoard ReverseFunc => Util.RotateBoardAnticlockwise;

    public override (float, float) Get2dDistance(float distance)
    {
        return (0, -distance);
    }

    public override (int, int) GETNext(int moveSquare, int row, int col)
    {
        return (row + moveSquare, col);
    }

    public override string ToString()
    {
        return "down";
    }
}

public class Left : Direction
{
    public override Board.ConvertBoard ConvertFunc => Util.NoRotate;
    public override Board.ConvertBoard ReverseFunc => Util.NoRotate;

    public override (float, float) Get2dDistance(float distance)
    {
        return (-distance, 0);
    }

    public override (int, int) GETNext(int moveSquare, int row, int col)
    {
        return (row, col - moveSquare);
    }

    public override string ToString()
    {
        return "left";
    }
}

public class Right : Direction
{
    public override Board.ConvertBoard ConvertFunc => Util.FlipBoard;
    public override Board.ConvertBoard ReverseFunc => Util.FlipBoard;

    public override (float, float) Get2dDistance(float distance)
    {
        return (distance, 0);
    }

    public override (int, int) GETNext(int moveSquare, int row, int col)
    {
        return (row, col + moveSquare);
    }

    public override string ToString()
    {
        return "right";
    }
}