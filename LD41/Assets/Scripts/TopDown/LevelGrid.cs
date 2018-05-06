using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class LevelGrid : MonoBehaviour
{
    public class GridActionEventArgs : EventArgs
    {
        public string Action;
        public GridObject Source;
        public int[] TargetTile;
        public Dictionary<string, object> Values;
    }

    public event EventHandler<GridActionEventArgs> OnGridAction;

    [Serializable]
    public class ObjectTile
    {
        public char CharCode;
        public PoolInstance PoolInstance;
        public bool Blocking;
        public Vector2 Offset;
        public int LayerOffset;
    }

    public static class GridEvents
    {
        public const string EnemyAttack = "EnemyAttack";
        public const string HeroAttack = "HeroAttack";
    }

    public static class GridValues
    {
        public const string Damage = "Damage";
        public const string Push = "Push";
    }

    public ObjectTile[] TileDefinitions;
    public Dictionary<char, ObjectTile> TileDictionary;
    private const int _rows = 7;
    private const int _columns = 7;
    public string[][] Map = new string[_rows][];
    public float TileSize;
    public Vector2 TileOffset;
    public PoolInstance GridFlash;
    private List<GridObject> _gridObjects = new List<GridObject>();
    public PoolInstance TemporaryBlock;

    // Use this for initialization
    void Start()
    {
        Map[0] = new string[_columns] { "X1|", "X1|", "X1|", "X1|34", "X1|", "X1|", "X1|" };
        Map[1] = new string[_columns] { "X27/(", "X16", "X16", "X16", "X16", "X16", @"X2&7\" };
        Map[2] = new string[_columns] { "X1&[6[>", "0", "0", "0", "0", "0", "X1|[6[|<" };
        Map[3] = new string[_columns] { "X1&[6[>", "0", "0", "0", "0", "0", "X1|[6[|<" };
        Map[4] = new string[_columns] { "X1&[6[>", "0", "0", "0", "0", "0", "X1|[6[|<" };
        Map[5] = new string[_columns] { "X1&[6[>", "0", "0", "0", "0", "0", "X1|[6[|<" };
        Map[6] = new string[_columns] { @"X2|7\()&>", "X1|6|", "X1|6|", "X1|36|", "X1|6|", "X1|6|", @"X2|[7/)" };
        //Map[0] = new string[7] { "7", "7", "7", "7", "7", "7", "7" };
        //Map[1] = new string[7] { "6", "5", "5", "5", "5", "5", "6&" };
        //Map[2] = new string[7] { "5[", "1", "1", "1", "1", "1", "5&[" };
        //Map[3] = new string[7] { "5[", "1", "1D", "1", "1", "1", "5&[" };
        //Map[4] = new string[7] { "5[", "1", "1", "1", "1", "1", "1Z" };
        //Map[5] = new string[7] { "5[", "1", "1", "1", "1", "1", "0" };
        //Map[6] = new string[7] { "6|", "0", "0", "0", "0", "0", "6&|" };

        TileDictionary = TileDefinitions.ToDictionary(k => k.CharCode, k => k);
        StartCoroutine(CreateMap());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FlashTile(int[] xy, float durationInSeconds, Color color, Action onFlash = null)
    {
        StartCoroutine(Flash(xy, durationInSeconds, color, onFlash));
    }

    private IEnumerator Flash(int[] xy, float durationInSeconds, Color color, Action onFlash)
    {
        var obj = Toolbox.Instance.Pool.Retrieve(GridFlash);
        SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
        obj.transform.position = GridToVector2(xy);

        float time = durationInSeconds;
        spr.material.SetColor("_Colorize", color);
        spr.material.SetFloat("_Saturation", 0.6f);
        while (time > 0)
        {
            spr.material.SetFloat("_Opacity", Mathf.Min(1 / time * 0.45f, 0.45f) + Mathf.Sin(time * 5) * 0.1f);
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (time < durationInSeconds / 2)
        {
            time = time == 0 ? 0.001f : time;
            spr.material.SetFloat("_Opacity", Mathf.Max((durationInSeconds - time) * 0.45f, 0));
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Toolbox.Instance.Pool.Release(GridFlash, obj);
        if (onFlash != null)
        {
            onFlash();
        }
    }

    public IEnumerator CreateMap()
    {
        for (int y = 0; y < Map.Length; y++)
        {
            for (int x = 0; x < Map[y].Length; x++)
            {
                GameObject prev = null;
                ObjectTile prevTile = null;
                foreach (char c in Map[y][x])
                {
                    if (!TryProcessingAsSymbol(c, prev, prevTile))
                    {
                        var tile = TileDictionary[c];
                        if (tile.PoolInstance == null) continue;
                        var obj = Toolbox.Instance.Pool.Retrieve(tile.PoolInstance);
                        obj.SetActive(true);
                        var sprRender = obj.GetComponent<SpriteRenderer>();
                        if (sprRender != null)
                        {
                            sprRender.flipX = sprRender.flipY = false;
                            sprRender.sortingOrder += tile.LayerOffset;
                        }
                        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                        obj.transform.position = GridToVector2(new[] { x, y }) + tile.Offset;
                        prev = obj;
                        prevTile = tile;
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    private bool TryProcessingAsSymbol(char c, GameObject prev, ObjectTile prevTile)
    {
        if (prev == null || prevTile == null) return false;
        SpriteRenderer spr = null;
        switch (c)
        {
            case '>':
                prev.transform.position = prev.transform.position + new Vector3(0.1f, 0);
                return true;
            case '<':
                prev.transform.position = prev.transform.position + new Vector3(-0.1f, 0);
                return true;
            case '^':
                prev.transform.position = prev.transform.position + new Vector3(0, 0.1f);
                return true;
            case '_':
                prev.transform.position = prev.transform.position + new Vector3(0, -0.1f);
                return true;
            case '(':
                prev.transform.position = prev.transform.position - new Vector3(prevTile.Offset.x * 2, 0);
                return true;
            case ')':
                prev.transform.position = prev.transform.position - new Vector3(0, prevTile.Offset.y * 2);
                return true;
            case '\\':
                prev.transform.localRotation = Quaternion.Euler(0, 0, -45);
                return true;
            case '/':
                prev.transform.localRotation = Quaternion.Euler(0, 0, 45);
                return true;
            case '[':
                prev.transform.localRotation = Quaternion.Euler(0, 0, 90);
                return true;
            case '&':
                spr = prev.GetComponent<SpriteRenderer>();
                if (spr != null)
                {
                    spr.flipX = true;
                }
                return true;
            case '|':
                spr = prev.GetComponent<SpriteRenderer>();
                if (spr != null)
                {
                    spr.flipY = true;
                }
                return true;
        }
        return false;
    }

    public bool IsBlocking(int x, int y)
    {
        if (Map.Length <= y) return true;
        if (Map[y].Length <= x) return true;
        return Map[y][x].Any(c =>
        {
            return TileDictionary[c].Blocking;
        }) ||
        _gridObjects.Any(g => g.isActiveAndEnabled && g.Blocking && Vector2ToGrid(g.transform.position).SequenceEqual(new[] { x, y }));
    }

    public int[] Vector2ToGrid(Vector2 pos)
    {
        int x = Mathf.RoundToInt((pos.x - TileOffset.x) / TileSize);
        int y = -Mathf.RoundToInt(pos.y + TileOffset.y / TileSize);
        return new[] { x, y };
    }

    public Vector2 GridToVector2(int[] xy)
    {
        int x = xy[0];
        int y = xy[1];
        return (new Vector2(x, -y) * TileSize) + new Vector2(TileOffset.x, -TileOffset.y);
    }

    public Vector2 CenterInGrid(Vector2 pos)
    {
        return GridToVector2(Vector2ToGrid(pos));
    }

    public void RegisterGridObject(GridObject gridObject)
    {
        if (!_gridObjects.Contains(gridObject))
        {
            _gridObjects.Add(gridObject);
        }
    }

    public GridObject[] IsCollidingWith(GridObject source)
    {
        var gridPos = Vector2ToGrid(source.transform.position);
        return _gridObjects.Where(g => Vector2ToGrid(g.transform.position).SequenceEqual(gridPos)).ToArray();
    }

    public void TriggerGridEvent(string action, GridObject source, int[] targetTile, Dictionary<string,object> values)
    {
        if (OnGridAction == null) return;
        var sourceGridPos = Vector2ToGrid(source.transform.position);
        foreach(var obj in _gridObjects)
        {
            if (!obj.isActiveAndEnabled) continue;
            var objGridPos = Vector2ToGrid(obj.transform.position);
            if (targetTile.SequenceEqual(objGridPos))
            {
                OnGridAction.Invoke(this, new GridActionEventArgs()
                {
                    Action = action,
                    Source = source,
                    TargetTile = targetTile,
                    Values = values
                });
            }
        }
    }

    public void CreateTemporaryBlock(int[] xy, float seconds)
    {
        Makinery tempBlock = new Makinery(mk.Priority.MapBlock);
        tempBlock.AddRoutine(() => MkCreateTempBlock(xy, seconds));
        Toolbox.Instance.MainMakina.AddMakinery(tempBlock);
    }

    IEnumerator<MakineryGear> MkCreateTempBlock(int[] xy, float seconds)
    {
        var obj = Toolbox.Instance.Pool.Retrieve(TemporaryBlock);
        obj.transform.position = GridToVector2(xy);

        yield return new WaitForSecondsGear(seconds);

        Toolbox.Instance.Pool.Release(TemporaryBlock, obj);
    }
}
