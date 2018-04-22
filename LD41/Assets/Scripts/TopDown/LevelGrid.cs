using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{

    [Serializable]
    public class ObjectTile
    {
        public char CharCode;
        public PoolInstance PoolInstance;
        public bool Blocking;
    }

    public ObjectTile[] TileDefinitions;
    public Dictionary<char, ObjectTile> TileDictionary;
    public string[][] Map = new string[7][];
    public float TileSize;
    public Vector2 TileOffset;

    // Use this for initialization
    void Start()
    {
        Map[0] = new string[7] { "X", "X", "X", "X", "X", "X", "X" };
        Map[1] = new string[7] { "X", "X", "X", "X", "X", "X", "X" };
        Map[2] = new string[7] { "X", "0", "0", "0", "0", "0", "X" };
        Map[3] = new string[7] { "X", "0", "0", "0", "0", "0", "X" };
        Map[4] = new string[7] { "X", "0", "0", "0", "0", "0", "0" };
        Map[5] = new string[7] { "X", "0", "0", "0", "0", "0", "X" };
        Map[6] = new string[7] { "X", "X", "X", "X", "X", "X", "X" };
        //Map[0] = new string[7] { "7", "7", "7", "7", "7", "7", "7" };
        //Map[1] = new string[7] { "6", "5", "5", "5", "5", "5", "6&" };
        //Map[2] = new string[7] { "5[", "1", "1", "1", "1", "1", "5&[" };
        //Map[3] = new string[7] { "5[", "1", "1D", "1", "1", "1", "5&[" };
        //Map[4] = new string[7] { "5[", "1", "1", "1", "1", "1", "1Z" };
        //Map[5] = new string[7] { "5[", "1", "1", "1", "1", "1", "0" };
        //Map[6] = new string[7] { "6|", "0", "0", "0", "0", "0", "6&|" };

        TileDictionary = TileDefinitions.ToDictionary(k => k.CharCode, k => k);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsBlocking(int x, int y)
    {
        if (Map.Length <= y) return true;
        if (Map[y].Length <= x) return true;
        return Map[y][x].Any(c =>
        {
            return TileDictionary[c].Blocking;
        });
    }

    public int[] Vector2ToGrid(Vector2 pos)
    {
        int x = Mathf.RoundToInt((pos.x - TileOffset.x) / TileSize);
        int y = -Mathf.RoundToInt(pos.y + TileOffset.y / TileSize);
        return new[] { x, y };
    }

    public Vector2 CenterInGrid(Vector2 pos)
    {
        int[] xy = Vector2ToGrid(pos);
        int x = xy[0];
        int y = xy[1];
        return (new Vector2(x, y) * TileSize) + TileOffset;
    }
}
