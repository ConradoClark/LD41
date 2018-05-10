using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class TileDefinitionJson
{
    public string CharCode;
    public string AssetPath;
    public bool Blocking;
    public Vector2 Offset;
    public int LayerOffset;
}
