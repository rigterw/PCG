using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileGrid : MonoBehaviour
{
    [SerializeField] GameObject[] tilePrefabs;
    int tileSize = 5;

    [SerializeField] Vector2Int levelSize;
    [SerializeField] Vector2Int minRoomSize;
    [SerializeField] Vector2Int maxRoomSize;


    // Start is called before the first frame update
    void Start()
    {
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
        Vector4[,] rooms = GenerateRooms();
        for(int i = 0; i < rooms.GetLength(0); i++){
            for(int j = 0; j < rooms.GetLength(1); j++){
                SetRoom(ref level.tileTypes, rooms[i, j]);
            }
        }

        return level;
    }

    /// <summary>
    /// Make a room in the tile map
    /// </summary>
    /// <param name="tileMap">The tilemap of the level</param>
    /// <param name="room">The data of the room</param>
    private void SetRoom(ref byte[,] tileMap, Vector4 room){
        Debug.Log(room);
        for (int x = (int)room.x; x <= (int)room.z; x++)
        {
            for (int y = (int)room.y; y <= (int)room.w; y++)
            {
                tileMap[x, y] = 1;
            }
        }
    }

    /// <summary>
    /// Function that generates where rooms will be
    /// </summary>
    /// <returns>An array of vector4s, In this vector4 x and y are the top left cord, z and w the bottom right</returns>
    private Vector4[,] GenerateRooms()
    {
        //Split the level into sections
        List<int> xLines = Split(minRoomSize.x, maxRoomSize.x, levelSize.x);
        List<int> yLines = Split(minRoomSize.y, maxRoomSize.y, levelSize.y);

        Vector4[,] rooms = new Vector4[xLines.Count, yLines.Count];


        //give each section a room with a random size and position within the section
        int minX = 1, minY = 1, maxX, maxY;
        for(int i = 0; i < xLines.Count; i++){
            maxX = xLines[i];
            minY = 1;
            for(int j = 0; j < yLines.Count; j++){
                maxY = yLines[j];
                rooms[i, j] = GenerateRoom(minX, minY, maxX, maxY);
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
}
