using System;
using UnityEngine;

public static class BoardExtension
{
    public static string ToStr(this int[][] board)
    {
            var boardStr = "";
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    try
                    {
                        boardStr += board[i][j];
                    }
                    catch (NullReferenceException)
                    {
                        Util.JagListDebugLog("####### ERROR _board ######## i: " + i + ", j: " + j + ", board",
                            board);
                    }
                }
    
                boardStr += "\n";
            }

            return boardStr;
    }
    public static string ToStr(this GameObject[][] board)
    {
            var boardStr = "";
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    try
                    {
                        boardStr += board[i][j].name[0];
                    }
                    catch (NullReferenceException)
                    {
                        // Util.JagListDebugLog("####### ERROR _board ######## i: " + i + ", j: " + j + ", board",
                        //     board);
                    }
                }
    
                boardStr += "\n";
            }

            return boardStr;
    }
}