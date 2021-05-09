using System.Collections.Generic;
using UnityEngine;

internal class PanelManager
{
    public readonly Dictionary<int, GameObject> Panels;

    public PanelManager()
    {
        Panels = new Dictionary<int, GameObject>()
        {
            {0, GetPanel()},
            {2, GetPanel(2)},
            {4, GetPanel(4)},
            {8, GetPanel(8)},
            {16, GetPanel(16)},
            {32, GetPanel(32)},
            {64, GetPanel(64)},
            {128, GetPanel(128)},
            {256, GetPanel(256)},
            {512, GetPanel(512)},
            {1024, GetPanel(1024)},
            {2048, GetPanel(2048)},
            {4096, GetPanel(4096)},
            {8192, GetPanel(8192)},
        };
    }

    private static GameObject GetPanel(int value = 0)
    {
        // Debug.Log("value:"+value);
        var prefabName = value == 0 ? "empty" : value.ToString();
        var obj = (GameObject) Resources.Load("prefabs/" + prefabName);
        return obj;
    }
}