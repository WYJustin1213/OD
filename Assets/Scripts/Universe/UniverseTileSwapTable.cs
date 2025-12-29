using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Universe/Tile Swap Table")]
public class UniverseTileSwapTable : ScriptableObject
{
    [System.Serializable]
    public struct TileSwap
    {
        public TileBase u1;
        public TileBase u2;
    }

    public List<TileSwap> swaps = new();

    public Dictionary<TileBase, TileBase> BuildMapToU1()
    {
        var dict = new Dictionary<TileBase, TileBase>();
        foreach (var s in swaps)
            if (s.u2 != null && s.u1 != null) dict[s.u2] = s.u1;
        return dict;
    }

    public Dictionary<TileBase, TileBase> BuildMapToU2()
    {
        var dict = new Dictionary<TileBase, TileBase>();
        foreach (var s in swaps)
            if (s.u1 != null && s.u2 != null) dict[s.u1] = s.u2;
        return dict;
    }
}
