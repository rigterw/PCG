using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public struct LevelData{
    public byte[,]tileTypes;
    public List<Tuple<DungeonObject, UnityEngine.Vector2>> objects;
}

public enum DungeonObject {
    player,
    finish,
    key,
    weapon,
    enemy
}