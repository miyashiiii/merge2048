public abstract class Direction
{
    public virtual Util.ConvertBoard ConvertFunc => Util.NoRotate;
    public virtual Util.ConvertBoard ReverseFunc => Util.NoRotate;

    public abstract (float, float) Get2dDistance(float distance);
    public abstract (int, int) GETNext(int moveSquare, int row, int col);
    public static readonly Up Up = new Up();
    public static readonly Down Down = new Down();
    public static readonly Left Left = new Left();
    public static readonly Right Right = new Right();
}

public class Up : Direction
{
    public override Util.ConvertBoard ConvertFunc => Util.RotateBoardAnticlockwise;
    public override Util.ConvertBoard ReverseFunc => Util.RotateBoardClockwise;

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
    public override Util.ConvertBoard ConvertFunc => Util.RotateBoardClockwise;
    public override Util.ConvertBoard ReverseFunc => Util.RotateBoardAnticlockwise;

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
    public override Util.ConvertBoard ConvertFunc => Util.NoRotate;
    public override Util.ConvertBoard ReverseFunc => Util.NoRotate;

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
    public override Util.ConvertBoard ConvertFunc => Util.FlipBoard;
    public override Util.ConvertBoard ReverseFunc => Util.FlipBoard;

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