using System.Collections.Generic;

public static class Config
{
    public static readonly Dictionary<int, float> PanelRatioMap = new Dictionary<int, float>
    {
        {2, 0.9f},
        {4, 0.1f},
        {8, 0f},
    };

    public const int MoveFrames = 3;
    public const int CreateFrames = 4;
}