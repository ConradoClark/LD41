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

    public class GridObjectReference
    {
        public GameObject Object;
        public ObjectTile Tile;
    }

    public static class GridEvents
    {
        public const string CrushAttack = "CrushAttack";
        public const string EnemyAttack = "EnemyAttack";
        public const string HeroAttack = "HeroAttack";
    }

    public static class GridValues
    {
        public const string Damage = "Damage";
        public const string Push = "Push";
    }

    // Public Properties
    public bool Initialized { get; private set; }
    public string CurrentGoal { get; private set; }
    public string CurrentGoalProgress { get; private set; }
    public int CompletedGoals { get; private set; }
    // Public Fields
    public Dictionary<GoalEnum, IGoal> PossibleGoals;
    public Dictionary<char, ObjectTile> TileDictionary;
    public string[][] Map = new string[_rows][];
    // Public Parameters
    public float TileSize;
    public Vector2 TileOffset;
    public PoolInstance GridFlash;
    public PoolInstance TemporaryBlock;
    // Private Fields
    private List<GridObject> _gridObjects = new List<GridObject>();
    private Goal[] _currentGoals;
    private GameObject _sceneryPool;

    // Constants 
    private const int _rows = 7;
    private const int _columns = 7;

    private void Awake()
    {
        PossibleGoals = new Dictionary<GoalEnum, IGoal>()
        {
            { GoalEnum.DefeatAllEnemies, new DefeatAllEnemies(this) }
        };
    }

    // Use this for initialization
    void Start()
    {
        LoadLevel("Levels/Tilesets/Forest1", "Levels/World1/1");
    }

    public void LoadLevel(string tilesetAsset, string levelAsset)
    {
        Makinery loadLevel = new Makinery(1);
        loadLevel.AddRoutine(
            () => MkLoadLevel("Levels/Tilesets/Forest1", "Levels/World1/1"),
            () => MkCreateMap(),
            () => MkLoadGoals());
        Toolbox.Instance.MainMakina.AddMakinery(loadLevel);
    }

    private IEnumerator<MakineryGear> MkLoadLevel(string tilesetAsset, string levelAsset)
    {
        Initialized = false;
        ResourceRequest tilesetResource = Resources.LoadAsync<TextAsset>(tilesetAsset);
        ResourceRequest levelResource = Resources.LoadAsync<TextAsset>(levelAsset);

        // Load resources async
        while (!tilesetResource.isDone || !levelResource.isDone)
        {
            yield return new WaitForFrameCountGear();
        }

        // Serialization
        TextAsset tilesetText = (TextAsset)tilesetResource.asset;
        TextAsset levelText = (TextAsset)levelResource.asset;

        TilesetJson tileset = JsonUtility.FromJson<TilesetJson>(tilesetText.text);
        LevelJson level = JsonUtility.FromJson<LevelJson>(levelText.text);

        // Setting goals
        _currentGoals = level.Goals.Select(g =>
        {
            GoalEnum ge = (GoalEnum)Enum.Parse(typeof(GoalEnum), g.Goal);
            return new Goal()
            {
                Objective = ge,
                GoalLogic = PossibleGoals[ge],
                Spawn = g.Spawn.Select(a => a.Array).ToArray()
            };
        }).ToArray();

        // Cleanup
        if (_sceneryPool == null)
        {
            _sceneryPool = new GameObject("sceneryPool");

        }
        Pool pool = _sceneryPool.transform.GetOrAddComponent<Pool>();
        if (pool.pools != null)
        {
            foreach (var p in pool.pools)
            {
                Destroy(p.gameObject);
            }
        }

        // Object pool creation
        Dictionary<string, ResourceRequest> resources = new Dictionary<string, ResourceRequest>();
        Dictionary<string, PoolInstance> poolInstances = new Dictionary<string, PoolInstance>();

        foreach (var path in tileset.Tiles.Select(a => a.AssetPath).Distinct())
        {
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }
            resources.Add(path, Resources.LoadAsync<GameObject>(path));
        }

        while (resources.Any(r => !r.Value.isDone))
        {
            yield return new WaitForFrameCountGear();
        }

        int i = 1;
        foreach (var resource in resources)
        {
            GameObject poolInstance = new GameObject("poolInstance#" + i);
            poolInstance.transform.SetParent(pool.transform);
            PoolInstance instance = poolInstance.AddComponent<PoolInstance>();
            instance.objectType = (GameObject)resource.Value.asset;
            instance.count = 20;
            poolInstances[resource.Key] = instance;
            i++;
        }

        pool.pools = poolInstances.Values.ToArray();
        pool.Initialize(); // late initialize

        Map = level.Scenery.Select(s => s.Array.ToArray()).ToArray();
        TileDictionary = tileset.Tiles.ToDictionary(t => t.CharCode.FirstOrDefault(), t => new ObjectTile()
        {
            CharCode = t.CharCode.FirstOrDefault(),
            PoolInstance = !string.IsNullOrEmpty(t.AssetPath) ? poolInstances[t.AssetPath] : null,
            Blocking = t.Blocking,
            Offset = t.Offset,
            LayerOffset = t.LayerOffset,
        });
        Initialized = true;
    }

    private IEnumerator<MakineryGear> MkCreateMap()
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
                        var reference = CreateGridObject(c, new[] { x, y });
                        prev = reference == null ? null : reference.Object;
                        prevTile = reference == null ? null : reference.Tile;
                    }
                }
            }
        }
        yield break;
    }

    private GridObjectReference CreateGridObject(char c, int[] xy)
    {
        if (!TileDictionary.ContainsKey(c)) return null;
        var tile = TileDictionary[c];
        if (tile.PoolInstance == null) return null;
        var obj = Toolbox.Instance.Pool.Retrieve(tile.PoolInstance);
        obj.SetActive(true);
        var sprRender = obj.GetComponent<SpriteRenderer>();
        if (sprRender != null)
        {
            sprRender.flipX = sprRender.flipY = false;
            sprRender.sortingOrder += tile.LayerOffset;
        }
        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
        obj.transform.position = GridToVector2(xy) + tile.Offset;
        return new GridObjectReference()
        {
            Object = obj,
            Tile = tile
        };
    }

    private IEnumerator<MakineryGear> MkLoadGoals()
    {
        if (_currentGoals == null) yield break;

        foreach (var goal in _currentGoals)
        {
            for (int y = 0; y < goal.Spawn.Length; y++)
            {
                for (int x = 0; x < goal.Spawn[y].Length; x++)
                {
                    foreach (char c in goal.Spawn[y][x])
                    {
                        var reference = CreateGridObject(c, new[] { x, y });
                        if (reference != null)
                        {
                            reference.Object.GetComponent<SpawnObject>().Spawn(GridToVector2(new[] { x, y }), this);
                        }
                    }
                }
            }

            var progress = goal.GoalLogic.GetGoalProgress();
            while (!progress.Completed)
            {
                CurrentGoal = progress.GoalName;
                CurrentGoalProgress = progress.Progress;
                yield return new WaitForFrameCountGear();
            }

            CompletedGoals++;
        }
    }

    public void FlashTile(int[] xy, float strength, float durationInSeconds, Color color, Action onFlash = null)
    {
        StartCoroutine(Flash(xy, strength, durationInSeconds, color, onFlash));
    }

    private IEnumerator Flash(int[] xy, float strength, float durationInSeconds, Color color, Action onFlash)
    {
        var obj = Toolbox.Instance.Pool.Retrieve(GridFlash);
        SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
        obj.transform.position = GridToVector2(xy);

        float time = durationInSeconds;
        spr.material.SetColor("_Colorize", color);
        spr.material.SetFloat("_Saturation", 0.6f);
        while (time > 0)
        {
            spr.material.SetFloat("_Opacity", Mathf.Min(1 / time * 0.45f, 0.45f) + Mathf.Sin(time * 5) * 0.1f * strength);
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        float opacity = spr.material.GetFloat("_Opacity");
        time = 0f;
        while (time < 1f)
        {
            spr.material.SetFloat("_Opacity", Mathf.Lerp(opacity, 0, time));
            time += Time.deltaTime * 4f;
            yield return new WaitForEndOfFrame();
        }

        Toolbox.Instance.Pool.Release(GridFlash, obj);
        if (onFlash != null)
        {
            onFlash();
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
        if (!Initialized) return false;
        if (Map.Length <= y) return true;
        if (Map[y].Length <= x) return true;
        return Map[y][x].Any(c =>
        {
            return TileDictionary.ContainsKey(c) && TileDictionary[c].Blocking;
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

    public void TriggerGridEvent(string action, GridObject source, int[] targetTile, Dictionary<string, object> values)
    {
        if (OnGridAction == null) return;
        var sourceGridPos = Vector2ToGrid(source.transform.position);
        foreach (var obj in _gridObjects)
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

    public GridObject[] GetActiveGridObjects(GridObject.GridObjectType type)
    {
        return _gridObjects.Where(g => g.Type == type && g.isActiveAndEnabled).ToArray();
    }
}
