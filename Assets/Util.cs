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
        catch (ArgumentException e)
        {
            log=null;
        }
            
        Debug.Log(log);
    }
}