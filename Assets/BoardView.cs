using UnityEngine;

public static class BoardView
{
    private static GameObject _canvas;
    private static Vector2[][] _posArray;


    private static readonly NumPanel NumPanel = new NumPanel();


    private static int _countMoveFrames;

    private static int _countCreateFrames;

    public static Direction DirectionInAnimation;

    public static GameObject[][] Instances;

    private static Vector2 _cellSize;
    private static Vector2 _spacing;


    public static float StartTime;

    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        _cellSize = cellSize;
        _spacing = spacing;


        _canvas = GameObject.Find("Canvas");

        InitPosArray(parentPos);

        InitBoard();
    }

    private static void InitBoard()
    {
        Instances = new[]
        {
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
        };
    }


    private static void InitPosArray(Vector2 parentPos)
    {
        var topLeftPos = new Vector2(
            parentPos.x - (_cellSize.x + _spacing.x) * 1.5f,
            parentPos.y - (_cellSize.y + _spacing.y) * 1.5f
        );
        _posArray = new Vector2[4][];
        for (int i = 0; i < 4; i++)

        {
            _posArray[i] = new Vector2[4];
        }

        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            {
                var vec = new Vector2(
                    topLeftPos.x + (_cellSize.x + _spacing.x) * col,
                    topLeftPos.y + (_cellSize.y + _spacing.y) * row
                );
                row = 3 - row;
                _posArray[row][col] = vec;
            }
        }

        Util.ListDebugLog("PosArray", _posArray);
    }


    public static void MovingAnimation()
    {
        int[][] tmpBoard = new int[4][];
        GameObject[][] tmpInstances = new GameObject[4][];
        // マージ済みインスタンスは削除
        if (Config.MoveFrames == _countMoveFrames)
        {
            for (var row = 0; row < 4; row++)
            {
                tmpBoard[row] = new int[4];
                tmpInstances[row] = new GameObject[4];
                for (var col = 0; col < 4; col++)
                {
                    tmpBoard[row][col] = 0;
                    tmpInstances[row][col] = null;
                    var moveNum = BoardData.MoveNumBoard[row][col];
                    var instance = Instances[row][col];
                    // tmpBoard[row][col] = ReferenceEquals(instance, null) ? 0 : int.Parse(instance.name[0].ToString());
                    // 移動せずマージする場合はオブジェクト削除
                    if (moveNum == 0 && BoardData.DeleteAfterMoveBoard[row][col] == 1)
                    {
                        Object.Destroy(instance);
                        Instances[row][col] = null;

                        // _instances[row][col] = null;
                    }
                }
            }
        }

        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var moveSquare = BoardData.MoveNumBoard[row][col];
                if (moveSquare == 0)
                {
                    continue;
                }

                // 最終フレームかつ削除するパネルなら削除してcontinue
                if (Config.MoveFrames == _countMoveFrames && BoardData.DeleteAfterMoveBoard[row][col] == 1)
                {
                    Object.Destroy(Instances[row][col]);
                    Instances[row][col] = null;
                    continue;
                }


                var distance = moveSquare * (_cellSize.x + _spacing.x) / (Config.MoveFrames + 1);
                Debug.Log("col: " + col + ", row: " + row + ", Move Frames: " + Config.MoveFrames + ", countFrames: " +
                          _countMoveFrames);
                var (distanceX, distanceY) = DirectionInAnimation.Get2dDistance(distance);
                Instances[row][col].transform.Translate(distanceX, distanceY, 0);
            }
        }

        if (Config.MoveFrames == _countMoveFrames)
        {
            // int[][] putCheckBoard = new int[4][];

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var moveSquare = BoardData.MoveNumBoard[row][col];

                    var (nextRow, nextCol) = DirectionInAnimation.GETNext(moveSquare, row, col);

                    if (Instances[row][col] == null) continue;
                    tmpInstances[nextRow][nextCol] = Instances[row][col];
                }
            }

            Instances = tmpInstances;
        }
    }


    public static void ContinueMovingAnimation()
    {
        _countMoveFrames++;

        // finish animation
        // 移動したinstanceを削除
        MovingAnimation();
        if (_countMoveFrames != Config.MoveFrames)
        {
            return;
        }

        _countMoveFrames = 0;
        // Status = StatusWaitingInput;

        // MergedBoard に従って画面更新
        BoardData.RandPut();

        // isNewBoardに従って新規作成アニメーション
        StartCreatingAnimation();
    }

    private static void CreatingAnimation()
    {
        Util.JagListDebugLog("IsNewBoard", BoardData.IsNewBoard);
        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var isNewSquare = BoardData.IsNewBoard[row][col];
                if (isNewSquare == 0)
                {
                    continue;
                }

                Debug.Log("row:" + row + ", col:" + col + ", isNewSquare:" + isNewSquare);
                const float scalePerFrame = 0.1f;
                var scale = 1f;
                if (_countCreateFrames <= Config.CreateFrames / 2)
                {
                    scale += (_countCreateFrames + 1) * scalePerFrame;
                }
                else
                {
                    scale += (Config.CreateFrames - _countCreateFrames - 1) * scalePerFrame;
                }

                // var (row, col) = NumToIndex(idx);
                if (_countCreateFrames == 0)
                {
                    var panelNum = BoardData.CurrentBoard[row][col];
                    if (panelNum == 0)
                    {
                        Debug.Log("---- ERROR empty panel put ----");
                    }

                    var p = NumPanel.NumPanelMap[panelNum];
                    // Debug.Log("Put");
                    var instance = Object.Instantiate(p, _posArray[row][col], Quaternion.identity);
                    instance.transform.SetParent(_canvas.transform);
                    BoardData.CurrentBoard[row][col] = panelNum;
                    BoardData.IsNewBoard[row][col] = 1;
                    Instances[row][col] = instance;
                }

                Debug.Log(_countCreateFrames);
                Instances[row][col].transform.localScale = new Vector3(scale, scale, 1);
                Instances[row][col].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public static void StartCreatingAnimation()

    {
        GameManager.Status = GameManager.StatusInCreateAnimation;
        CreatingAnimation();
    }

    public static void ContinueCreatingAnimation()
    {
        _countCreateFrames++;
        // Debug.Log("CountMoveFrames: " + CountMoveFrames);
        if (_countCreateFrames != Config.CreateFrames)
        {
            CreatingAnimation();
            return;
        }

        // finish animation
        _countCreateFrames = 0;

        var isFinish = BoardData.CheckFinish();
        GameManager.Status = isFinish ? GameManager.StatusFinish : GameManager.StatusWaitingInput;
    }

    public static void Reset()
    {
        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                Object.Destroy(Instances[row][col]);
            }
        }

        InitBoard();
    }
}