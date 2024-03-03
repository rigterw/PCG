using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileGrid : MonoBehaviour
{
    [SerializeField] GameObject[] tilePrefabs;
    [SerializeField] GameObject[] levelObjectsPrefabs;
    int tileSize = 5;

    [SerializeField] Vector2Int levelSize;
    [SerializeField] Vector2Int minRoomSize;
    [SerializeField] Vector2Int maxRoomSize;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        if (!CheckSettings())
            return;
        SpawnLevel();
    }

    
    private bool CheckSettings(){
        if(minRoomSize.x > levelSize.x - 2){
            Debug.LogError("min room width larger than level size");
            return false;
        }
        if(minRoomSize.y > levelSize.y - 2){
            Debug.LogError("min room height larger than level size");
            return false;
        }

        if(NullCheckVector(minRoomSize)){
            Debug.LogError("no Max room size");
            return false;
        }

        if(NullCheckVector(levelSize)){
            Debug.LogError("no level size");
            return false;
        }

        if(NullCheckVector(maxRoomSize)){
            Debug.LogError("no Max room size");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Function that checks if a vector has any value set below 1
    /// </summary>
    /// <param name="vector">The vector that needs to be checked</param>
    /// <returns>true if any value is below 1</returns>
    private bool NullCheckVector(Vector2 vector){
        return vector.x <= 0 || vector.y <= 0;
    }

    /// <summary>
    /// Function that spawns a new level
    /// </summary>
    private void SpawnLevel()
    {
        LevelData level = GenerateLevel();

        //Create the level
        byte[,] tiles = level.tileTypes;
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                if (tiles[x,y] >= tilePrefabs.Length)
                {
                    Debug.LogError("Tile value is higher than ntileTypes");
                    return;
                }

                Vector2 position = new Vector2(x, y) * tileSize;
                GameObject tile = Instantiate(tilePrefabs[tiles[x,y]], position, Unity.Mathematics.quaternion.identity, transform);
                tile.name = $"{x},{y}";
            }
        }

        SpawnObjects(level.objects);
    }


    /// <summary>
    /// Function that generates a new randomised level
    /// </summary>
    /// <returns>An Leveldata object containing all data needed to generate a level</returns>
    private LevelData GenerateLevel()
    {
        LevelData level = new LevelData();
        //generate a grid of walls
        level.tileTypes = new byte[levelSize.x, levelSize.y];

        //dig rooms in the wall grid
        Debug.Log("start rooms");
        Vector4[,] rooms = GenerateRooms(level);

        Debug.Log("start hallways");
        int[] roomOrder = GenerateHallways(rooms, level).ToArray();

        level.objects = GenerateLevelObjects(rooms, roomOrder);
        return level;
    }

    /// <summary>
    /// Make a room in the tile map
    /// </summary>
    /// <param name="tileMap">The tilemap of the level</param>
    /// <param name="room">The data of the room</param>
    private void SetRoom(ref byte[,] tileMap, Vector4 room){
        for (int x = (int)room.x; x <= (int)room.z; x++)
        {
            for (int y = (int)room.y; y <= (int)room.w; y++)
            {
                tileMap[x, y] = 1;
            }
        }
    }


    /// <summary>
    /// Function that creates a hallway in the tilemap
    /// </summary>
    /// <param name="tileMap">The map of all the tiles</param>
    /// <param name="hallway">Vector4 containing 2 connectionpoints of the hallway</param>
    private void SetHallway(ref byte[,] tileMap, Vector4 hallway)
    {
        Vector2Int pos = new Vector2Int((int)hallway.x, (int)hallway.y);
        Vector2Int destination = new Vector2Int((int)hallway.z, (int)hallway.w);

        int vx = pos.x > destination.x ? -1 : 1;
        int vy = pos.y > destination.y ? -1 : 1;

        while(pos != destination){


            bool HorizontalMove = Random.Range(0, 2) == 1;

            //make sure that the hallway never goes past the point
            if ((HorizontalMove && (pos.x == destination.x || pos.y + vy == destination.y)) || (!HorizontalMove && (pos.y == destination.y || pos.x + vx == destination.x)))
                continue;

            if(HorizontalMove){
                pos.x += vx;
            }else{
                pos.y += vy;
            }

            tileMap[pos.x, pos.y] = 1;
        }
    }

    /// <summary>
    /// Function that generates where rooms will be
    /// </summary>
    /// <returns>An array of vector4s, In this vector4 x and y are the top left cord, z and w the bottom right</returns>
    private Vector4[,] GenerateRooms(LevelData level)
    {
        //Split the level into sections
        List<int> xLines = Split(minRoomSize.x, maxRoomSize.x, levelSize.x);
        List<int> yLines = Split(minRoomSize.y, maxRoomSize.y, levelSize.y);

        Vector4[,] rooms = new Vector4[xLines.Count, yLines.Count];


        //give each section a room with a random size and position within the section
        int minX = 1, minY, maxX, maxY;
        for(int i = 0; i < xLines.Count; i++){
            maxX = xLines[i];
            minY = 1;
            for(int j = 0; j < yLines.Count; j++){
                maxY = yLines[j];
                Vector4 room = GenerateRoom(minX, minY, maxX, maxY);
                rooms[i, j] = room;
                SetRoom(ref level.tileTypes, room);

                minY = maxY + 1;
            }
            minX = maxX +1;
        }

        return rooms;
    }

    /// <summary>
    /// Function that creates cords for a room
    /// </summary>
    /// <param name="minX">The min value for x (including)</param>
    /// <param name="minY">The min value for y (including)</param>
    /// <param name="maxX">The max value for x (including)</param>
    /// <param name="maxY">the max value for y (including)</param>
    /// <returns>A vector4 which define the top left (x, y) and bottom right (z,w) cord of the room</returns>
    private Vector4 GenerateRoom(int minX, int minY, int maxX, int maxY){

        int randomX = Random.Range(minX, maxX - minRoomSize.x); 
        int randomY = Random.Range(minY, maxY - minRoomSize.y); 

        int roomWidth = Random.Range(minRoomSize.x, Mathf.Min(maxRoomSize.x, maxX - randomX));
        int roomHeight = Random.Range(minRoomSize.y, Mathf.Min(maxRoomSize.y, maxY - randomY));

        int bottomRightX = randomX + roomWidth;
        int bottomRightY = randomY + roomHeight;

        return new Vector4(randomX, randomY, bottomRightX, bottomRightY);
    }

    /// <summary>
    /// Function that splits an axis of the grid into small pieces
    /// </summary>
    /// <param name="minValue">Min value of the space between</param>
    /// <param name="maxValue">Max value of the space between</param>
    /// <param name="length">The length of the axis</param>
    /// <returns>The values which will definetly be walls</returns>
    private List<int> Split(int minValue, int maxValue, int length){
        int pointer = 0;

        List<int> values = new List<int>();

        do
        {
            int sectionWidth = Random.Range(pointer + minValue, pointer + maxValue+1);


            pointer += 1 + sectionWidth;

            if(pointer >= length-1){

                values.Add(length - 1);
                break;
            }

            values.Add(pointer);
        } while (true);

        return values;
    }

    /// <summary>
    /// Function that selects which points the hallways will lead to
    /// </summary>
    /// <param name="rooms">The rooms that need to be connected</param>
    /// <returns>A list of all rooms in the order that they received an hallway</returns>
    private List<int> GenerateHallways(Vector4[,] rooms, LevelData level){
        int columns = rooms.GetLength(0);
        int rows = rooms.GetLength(1);
        int nRooms = rows * columns;

        //An array of bitmaps that store for each room which sides have some sort of connection
        byte[] usedSides = InitUsedSides(rows, columns);

        List<int> usedRooms = new()
        {
            Random.Range(0, nRooms)
        };
        
        //create hallways between rooms till all rooms are connected
        while(usedRooms.Count < nRooms){

            int nextRoomId = usedRooms[Random.Range(0, usedRooms.Count)];

            //if room has no free sides left, continue
            if (usedSides[nextRoomId] == 15)
                continue;

            int otherRoomId;
            byte nextSide;
            int counter = 0;//counter prevents the room to get in an infinite loop if it can't find a neighbour
            do
            {
                counter++;
                //pick a random side of the room that still is available.
                do
                {
                    nextSide = (byte)(1 << Random.Range(0, 4));
                } while ((usedSides[nextRoomId] & nextSide) != 0);

                //Get the other room id
                otherRoomId = nextSide switch
                {
                    1 => nextRoomId + columns,
                    2 => nextRoomId + 1,
                    4 => nextRoomId - columns,
                    _ => nextRoomId - 1,
                };

                //If a neighbour can't be found, start with a new room
                if(counter > 100){
                    break;
                }
            } while (usedRooms.Contains(otherRoomId));

            if (counter > 100)
                continue;

            int firstId = Math.Min(nextRoomId, otherRoomId);
            int secondId = Math.Max(nextRoomId, otherRoomId);

            Vector4 hallway = GenerateHallway(firstId, secondId, nextSide == 2 || nextSide == 8, rooms, ref usedSides, columns);

            SetHallway(ref level.tileTypes, hallway);
            usedRooms.Add(otherRoomId);
        }
        return usedRooms;
    }


    /// <summary>
    /// Generate a hallway between 2 rooms
    /// </summary>
    /// <param name="room1">The room closest to 0,0</param>
    /// <param name="room2">The room furthest away from 0,0</param>
    /// <param name="horizontal">If the rooms are horizontal or vertical located of eachother</param>
    /// <returns>A vector4 containing the points of the hallway</returns>
    private Vector4 GenerateHallway(int room1Id, int room2Id, bool horizontal, Vector4[,] rooms, ref byte[] sides, int columns){

        Vector4 room1 = GetRoom(room1Id, rooms);
        Vector4 room2 = GetRoom(room2Id, rooms);

        //get for each room the min and max of where a hallway can be connected
        Vector2 room1Bounds = horizontal ? new Vector2(room1.y, room1.w) : new Vector2(room1.x, room1.z);
        Vector2 room2Bounds = horizontal ? new Vector2(room2.y, room2.w) : new Vector2(room2.x, room2.z);


        int p1 = (int)Random.Range(room1Bounds.x, room1Bounds.y);
        int p2 = (int)Random.Range(room2Bounds.x, room2Bounds.y);

        if (horizontal){
            Vector4 hallway = new Vector4(room1.z, p1, room2.x, p2);

            sides[room1Id] |= 4;
            sides[room2Id] |= 1;

            return hallway;
        }
        else{
            Vector4 hallway = new Vector4(p1, room1.w, p2, room2.y);

            sides[room1Id] |= 2;
            sides[room2Id] |= 8;
            return hallway;
        }
    }

    private List<Tuple<DungeonObject, Vector2>> GenerateLevelObjects(Vector4[,] rooms, int[] roomIds){
        List<Tuple<DungeonObject, Vector2>> objects = new()
        {
            Tuple.Create(DungeonObject.player, roomCenter(roomIds[0], rooms)),
            Tuple.Create(DungeonObject.finish, roomCenter(roomIds[roomIds.Length-1], rooms))

        };
        return objects;
    } 

    private void SpawnObjects(List<Tuple<DungeonObject, Vector2>> objects){
        foreach(Tuple<DungeonObject, Vector2> obj in objects){
            Instantiate(levelObjectsPrefabs[(int)obj.Item1], obj.Item2, Unity.Mathematics.quaternion.identity);
        }
    }

    private byte[] InitUsedSides(int rows, int columns){
        int nRooms = rows * columns;
        byte[] usedSides = new byte[nRooms];

        for(int i = 0; i < columns; i++){
            usedSides[i] |= 4;
        }

        //right row
        for (int i = 1; i <= rows; i++)
        {
            usedSides[i * columns -1] |= 2;
        }

        for(int i = 0; i < columns; i++){
            usedSides[(rows - 1) * columns + i] |=1;
        }
        //left row
        for(int i = 0; i < rows; i++){
            usedSides[i * columns] |=8;
        }

        return usedSides;
    }

    private Vector4 GetRoom(int id, Vector4[,] rooms){
        int columns = rooms.GetLength(0);
        return rooms[id % columns, id / columns];
    }
    private Vector2 roomCenter(int id, Vector4[,] rooms){

        Vector4 room = GetRoom(id, rooms);
        return new Vector2((room.x + room.z) / 2, (room.y + room.w) / 2) * tileSize;
    }
}
