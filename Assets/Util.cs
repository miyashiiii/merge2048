using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Util
{
    public static T RandomWithWeight<T>(Dictionary<T, float> map)
    {
        // valueの値を合計
        var total = map.Sum(elem => elem.Value);

        // totalの範囲内でランダムに値取得
        var randomPoint = Random.value * total;


        foreach (var elem in map)
        {
            if (randomPoint < elem.Value)
            {
                return elem.Key;
            }

            randomPoint -= elem.Value;
        }

        return map.First().Key;
    }

    public static void ListDebugLog<T>(string tag, IEnumerable<T> l)
    {
        var log = tag + ": " + string.Join(", ", l.Select(obj => obj.ToString()));
        Debug.Log(log);
    }
    public static void JagListDebugLog<T>(string tag, T[][] l)
    {
        string log ;
        try
        {

            log = tag + ": " + string.Join(",  \\ ", l.Select(obj =>
                string.Join(", ", obj.Select(o => o.ToString()))));
        }
        catch (ArgumentException )
        {
            log=null;
        }
            
        Debug.Log(log);
    }
    public static int[][] FlipBoard(int[][] board)
    {
        var flipped = new int[4][];
        for (var i = 0; i < 4; i++)
        {
            flipped[i] = board[i].Reverse().ToArray();
        }

        return flipped;
    }

    public static int[][] RotateBoardClockwise(int[][] board)
    {
        return RotateBoard90(board, clockwise: true);
    }

    public static int[][] RotateBoardAnticlockwise(int[][] board)
    {
        return RotateBoard90(board, clockwise: false);
    }

    public static int[][] RotateBoard90(int[][] board, bool clockwise)
    {
        const int rows = 4;
        const int cols = 4;
        var rotated = new int[4][];
        for (var j = 0; j < cols; j++)
        {
            rotated[j] = new int[4];
        }

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                if (clockwise)
                {
                    rotated[j][rows - i - 1] = board[i][j];
                }
                else
                {
                    rotated[cols - j - 1][i] = board[i][j];
                }
            }
        }

        return rotated;
    }

    public static int[][] NoRotate(int[][] board)
    {
        return board;
    }
 
}