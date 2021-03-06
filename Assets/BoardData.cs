using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class BoardData
{
    private static List<int> GetEmptyIndices(int[][] board)
    {
        return GetIndices(0, board);
    }

    private static List<int> GetIndices(int p, int[][] board)
    {
        var result = new List<int>();
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                if (board[i][j] == p)
                {
                    result.Add(i * 4 + j);
                }
            }
        }

        return result;
    }


    public static void RandPut()
    {
        // select panel
        var p = Config.fixPut ? 2 : Util.RandomWithWeight(Config.PanelRatioMap);

        var emptyIndices = GetEmptyIndices(GameManager.CurrentBoard);
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        var randIdx = Config.fixPut ? emptyIndices[0] : emptyIndices[Random.Range(0, emptyIndices.Count)];
        PUT(p, randIdx);
    }


    private static (int, int) NumToIndex(int n)
    {
        var row = n / 4;
        var col = n % 4;
        return (row, col);
    }

    // PUT
    private static void PUT(int panelNum, int idx)
    {
        var (row, col) = NumToIndex(idx);
        Debug.Log("[PUT] idx: " + idx + ", row" + row + ", col" + col);
        GameManager.CurrentBoard[row][col] = panelNum;
        GameManager.IsNewBoard[row][col] = 1;
    }


    private static (bool, int, int[][], int[][], int[][], int[][]) CalcMove(int[][] rows)
    {
        var isMove = false;
        var score = 0;
        var moveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var deleteAfterMoveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var mergedBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var isNewBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var rowCount = 0;
        foreach (var row in rows)
        {
            var moveRow = Enumerable.Repeat(0, 4).ToArray(); // ????????????????????????row?????????
            var deleteAfterMoveRow = Enumerable.Repeat(0, 4).ToArray(); // ???????????????????????????moveRow?????????
            var mergedRow = Enumerable.Repeat(0, 4).ToArray(); // ??????????????????
            var isNewRow = Enumerable.Repeat(0, 4).ToArray(); //?????????????????????merged????????????1??????????????????

            var colCount = 0;
            var mergedColCount = 0;

            var deleteIdxPool = 0;
            var before = 0; // ?????????????????? 1?????????empty?????????num??????
            foreach (var num in row)
            {
                // empty 
                if (num == 0)
                {
                    moveRow[colCount] = 0;
                    colCount++;
                    continue;
                }

                if (before == 0) //before????????????
                {
                    before = num;
                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    deleteIdxPool = colCount;
                    // merged??????Add?????????
                }
                else if (before == num) // before??????????????? -> ?????????
                {
                    moveRow[colCount] = colCount - mergedColCount;
                    deleteAfterMoveRow[colCount] = 1;
                    deleteAfterMoveRow[deleteIdxPool] = 1;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    mergedRow[mergedColCount] = num * 2;
                    score += num * 2;

                    isNewRow[mergedColCount] = 1;
                    mergedColCount++;
                    before = 0;
                }
                else // before???????????????
                {
                    mergedRow[mergedColCount] = before;
                    mergedColCount++;

                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    // mergedColCount++;
                    // merged??????Add?????????
                    before = num;
                    deleteIdxPool = colCount;
                }


                colCount++;
            }


            mergedRow[mergedColCount] = before;

            moveBoard[rowCount] = moveRow;
            deleteAfterMoveBoard[rowCount] = deleteAfterMoveRow;
            mergedBoard[rowCount] = mergedRow;
            isNewBoard[rowCount] = isNewRow;

            rowCount++;
        }

        return (isMove, score, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }


    private static (bool, int, int[][], int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard,
        Util.ConvertBoard convertFunc,
        Util.ConvertBoard reverseFunc)
    {
        var convertedBoard = convertFunc(jagBoard);
        var (isMove, score, tmpMoveBoard, tmpDeleteAfterMoveBoarD, tmpMergedBoard, tmpIsNewBoard) =
            CalcMove(convertedBoard);

        var mergedBoard = reverseFunc(tmpMergedBoard);
        var moveBoard = reverseFunc(tmpMoveBoard);
        var deleteAfterMoveBoard = reverseFunc(tmpDeleteAfterMoveBoarD);
        var isNewBoard = reverseFunc(tmpIsNewBoard);
        return (isMove, score, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }

    public static (bool, int, int[][], int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard,
        Direction direction)
    {
        return CalcMoveWithConvert(jagBoard, direction.ConvertFunc, direction.ReverseFunc);
    }


    public static bool CheckFinish()
    {
        var rotateBoard = Util.RotateBoardClockwise(GameManager.CurrentBoard);
        for (var i = 0; i < 4; i++)
        {
            var row = GameManager.CurrentBoard[i];
            if (row.Contains(0)) return false;
            var col = rotateBoard[i];
            if (col.Contains(0)) return false;

            if (row[0] == row[1] || row[1] == row[2] || row[2] == row[3] ||
                col[0] == col[1] || col[1] == col[2] || col[2] == col[3]
            )
            {
                return false;
            }
        }

        return true;
    }
}