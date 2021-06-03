using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class BoardData
{
    public static int[][] CurrentBoard;


    public static int[][] IsNewBoard;

    public static int[][] MoveNumBoard;
    public static int[][] DeleteAfterMoveBoard;


    public static int MovesCount;
    public static int Score;

    private static bool _fixPut;

    public static float StartTime;

    public static void Init(bool fixPut)
    {
        _fixPut = fixPut;


        InitBoard();
    }


    private static void InitBoard()
    {
        StartTime = Time.time;

        // Debug.Log("Board Init");
        CurrentBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        IsNewBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        DeleteAfterMoveBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };


        RandPut();
        RandPut();
        Score = 0;
        BoardView.StartCreatingAnimation();
    }

    public static void Reset()
    {
        MovesCount = 0;
        InitBoard();
    }


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
        var p = _fixPut ? 2 : Util.RandomWithWeight(Config.PanelRatioMap);

        var emptyIndices = GetEmptyIndices(CurrentBoard);
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        var randIdx = _fixPut ? emptyIndices[0] : emptyIndices[Random.Range(0, emptyIndices.Count)];
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
        CurrentBoard[row][col] = panelNum;
        IsNewBoard[row][col] = 1;
    }


    private static (bool, int[][], int[][], int[][], int[][]) CalcMove(int[][] rows)
    {
        var isMove = false;
        var moveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var deleteAfterMoveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var mergedBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var isNewBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var rowCount = 0;
        foreach (var row in rows)
        {
            var moveRow = Enumerable.Repeat(0, 4).ToArray(); // 移動するマス数。rowに対応
            var deleteAfterMoveRow = Enumerable.Repeat(0, 4).ToArray(); // 移動後削除フラグ。moveRowに対応
            var mergedRow = Enumerable.Repeat(0, 4).ToArray(); // マージ後状態
            var isNewRow = Enumerable.Repeat(0, 4).ToArray(); //マージフラグ。mergedに対応。1がマージあり

            var colCount = 0;
            var mergedColCount = 0;

            var deleteIdxPool = 0;
            var before = 0; // マージ確認用 1つ前のempty以外のnumの値
            foreach (var num in row)
            {
                // empty 
                if (num == 0)
                {
                    moveRow[colCount] = 0;
                    colCount++;
                    continue;
                }

                if (before == 0) //before考慮不要
                {
                    before = num;
                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    deleteIdxPool = colCount;
                    // mergedColCount++;
                    // mergedへのAddは保留
                }
                else if (before == num) // beforeと同パネル -> マージ
                {
                    moveRow[colCount] = colCount - mergedColCount;
                    deleteAfterMoveRow[colCount] = 1;
                    deleteAfterMoveRow[deleteIdxPool] = 1;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    mergedRow[mergedColCount] = num * 2;
                    Score += num * 2;

                    isNewRow[mergedColCount] = 1;
                    mergedColCount++;
                    before = 0;
                }
                else // beforeと別パネル
                {
                    mergedRow[mergedColCount] = before;
                    mergedColCount++;

                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    // mergedColCount++;
                    // mergedへのAddは保留
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

        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }


    public delegate int[][] ConvertBoard(int[][] board);

    private static (bool, int[][], int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard,
        Util.ConvertBoard convertFunc,
        Util.ConvertBoard reverseFunc)
    {
        // int[][] tmpMergedBoard;
        // int[][] tmpMoveBoard;
        // int[][] tmpIsNewBoard;
        var convertedBoard = convertFunc(jagBoard);
        var (isMove, tmpMoveBoard, tmpDeleteAfterMoveBoarD, tmpMergedBoard, tmpIsNewBoard) = CalcMove(convertedBoard);

        var mergedBoard = reverseFunc(tmpMergedBoard);
        var moveBoard = reverseFunc(tmpMoveBoard);
        var deleteAfterMoveBoard = reverseFunc(tmpDeleteAfterMoveBoarD);
        var isNewBoard = reverseFunc(tmpIsNewBoard);
        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }

    private static (bool, int[][], int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard, Direction direction)
    {
        return CalcMoveWithConvert(jagBoard, direction.ConvertFunc, direction.ReverseFunc);
    }

    // public static int[][] MergedBoard;

    public static bool Move(Direction direction)
    {
        IsNewBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };

        Util.ListDebugLog("board: ", CurrentBoard);

        bool isMove;
        (isMove, MoveNumBoard, DeleteAfterMoveBoard, CurrentBoard, IsNewBoard) =
            CalcMoveByDirection(CurrentBoard, direction);
        if (!isMove)
        {
            return false;
        }

        MovesCount++;
        return true;
    }

    public static bool CheckFinish()
    {
        var rotateBoard = Util.RotateBoardClockwise(CurrentBoard);
        for (var i = 0; i < 4; i++)
        {
            var row = CurrentBoard[i];
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