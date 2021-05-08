using UnityEngine;

internal static class PanelManager
{
    private static readonly Panel Empty = new Panel();
    private static readonly Panel P2 = new Panel(2);
    private static readonly Panel P4 = new Panel(4);
    private static readonly Panel P8 = new Panel(8);
    private static readonly Panel P16 = new Panel(16);
    private static readonly Panel P32 = new Panel(32);
    private static readonly Panel P64 = new Panel(64);
    private static readonly Panel P128 = new Panel(128);
    private static readonly Panel P256 = new Panel(256);
    private static readonly Panel P512 = new Panel(512);
    private static readonly Panel P1024 = new Panel(1024);
    private static readonly Panel P2048 = new Panel(2048);
    private static readonly Panel P4096 = new Panel(4096);
    private static readonly Panel P8192 = new Panel(8192);

    private static readonly Panel[] Panels = new Panel[]
        {Empty, P2, P4, P8, P16, P32, P64, P128, P256, P512, P1024, P2048, P4096, P8192};

    public static void SetNext()
    {
        P2.Next = P4;
        P4.Next = P8;
        P8.Next = P16;
        P16.Next = P32;
        P32.Next = P64;
        P64.Next = P128;
        P128.Next = P256;
        P256.Next = P512;
        P512.Next = P1024;
        P1024.Next = P2048;
        P2048.Next = P4096;
        P4096.Next = P8192;
    }

    public static Panel GetPanelByValue(int value)
    {
        foreach (var p in Panels)
        {
            if (p.Value == value)
            {
                return p;
            }
        }

        return Empty;
    }

    public class Panel : Object
    {
        public readonly GameObject Obj;
        public Panel Next;
        public readonly int Value;


        public Panel(int value = 0)
        {
            // Debug.Log("value:"+value);
            var prefabName = value == 0 ? "empty" : value.ToString();
            Obj = (GameObject) Resources.Load("prefabs/" + prefabName);
            Debug.Log("value:" + value + ",type:" + Obj.GetType());
            Value = value;
            // Debug.Log("value:"+value+" ok");
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}