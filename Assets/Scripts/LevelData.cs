using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public struct LevelData{
    public byte[,]tileTypes;
    public List<Tuple<DungeonObject, Vector2Int>> objects;
}

public enum DungeonObject {
    key,
    weapon,
    enemy
}