using UnityEngine;
using System.Collections.Generic;

public static class GameManager
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;


    public static readonly Dictionary<int, float> PanelRatioMap = new Dictionary<int, float>
    {
        {2, 0.9f},
        {4, 0.1f},
        {8, 0f},
    };


    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing, bool fixPut)
    {
        BoardView.Init(parentPos, cellSize, spacing); //DataのInitより先に
        BoardData.Init(fixPut);
    }

    public static void Reset()
    {
        BoardView.Reset(); // Dataのリセットより先に
        BoardData.Reset();
    }


    public delegate int[][] ConvertBoard(int[][] board);


    public static void Move(Direction direction)
    {
        BoardData.Move(direction);

        if (Status == StatusFinish)
        {
            return;
        }


        BoardView.DirectionInAnimation = direction;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        Status = StatusInMovingAnimation;
    }
}