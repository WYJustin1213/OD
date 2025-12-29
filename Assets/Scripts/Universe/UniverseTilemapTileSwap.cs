using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class UniverseTilemapTileSwap : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private UniverseTileSwapTable swapTable;

    private Dictionary<TileBase, TileBase> _toU1;
    private Dictionary<TileBase, TileBase> _toU2;

    private void Reset()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }

        if (swapTable != null)
        {
            _toU1 = swapTable.BuildMapToU1();
            _toU2 = swapTable.BuildMapToU2();
        }
    }

    private void OnEnable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged += OnUniverseChanged;

        Apply(UniverseManager.Instance != null ? UniverseManager.Instance.CurrentUniverse : UniverseId.U1);
    }

    private void OnDisable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged -= OnUniverseChanged;
    }

    private void OnUniverseChanged(UniverseId oldU, UniverseId newU) => Apply(newU);

    private void Apply(UniverseId u)
    {
        if (swapTable == null) return;
        if (_toU1 == null || _toU2 == null)
        {
            _toU1 = swapTable.BuildMapToU1();
            _toU2 = swapTable.BuildMapToU2();
        }

        var bounds = tilemap.cellBounds;
        var positions = new List<Vector3Int>();
        var tiles = new List<TileBase>();

        foreach (var pos in bounds.allPositionsWithin)
        {
            var t = tilemap.GetTile(pos);
            if (t != null)
            {
                positions.Add(pos);
                tiles.Add(t);
            }
        }

        var map = (u == UniverseId.U2) ? _toU2 : _toU1;

        for (int i = 0; i < positions.Count; i++)
        {
            if (map.TryGetValue(tiles[i], out var replacement) && replacement != null)
                tilemap.SetTile(positions[i], replacement);
        }

        tilemap.RefreshAllTiles();
    }
}
