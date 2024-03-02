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

    private bool NullCheckVector(Vector2 vector){
        return vector.x == 0 || vector.y == 0;
    }

    private void SpawnLevel()
    {
        LevelData level = GenerateLevel();
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


    private LevelData GenerateLevel()
    {
        LevelData level = new LevelData();
        level.tileTypes = new byte[levelSize.x, levelSize.y];
        Vector4[,] rooms = GenerateRooms();
        
        for(int i = 0; i < rooms.GetLength(0); i++){
            for(int j = 0; j < rooms.GetLength(1); j++){
                SetRoom(ref level.tileTypes, rooms[i, j]);
            }
        }

        return level;
    }

    private void SetRoom(ref byte[,] tiles, Vector4 room){
        Debug.Log(room);
        for (int x = (int)room.x; x <= (int)room.z; x++)
        {
            for (int y = (int)room.y; y <= (int)room.w; y++)
            {
                tiles[x, y] = 1;
            }
        }
    }

    private Vector4[,] GenerateRooms()
    {
        List<int> xLines = Split(minRoomSize.x, maxRoomSize.x, levelSize.x);
        List<int> yLines = Split(minRoomSize.y, maxRoomSize.y, levelSize.y);

        Vector4[,] rooms = new Vector4[xLines.Count, yLines.Count];

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

    private Vector4 GenerateRoom(int minX, int minY, int maxX, int maxY){

        int randomX = Random.Range(minX, maxX - minRoomSize.x); 
        int randomY = Random.Range(minY, maxY - minRoomSize.y); 

        int roomWidth = Random.Range(minRoomSize.x, Mathf.Min(maxRoomSize.x, maxX - randomX));
        int roomHeight = Random.Range(minRoomSize.y, Mathf.Min(maxRoomSize.y, maxY - randomY));

        int bottomRightX = randomX + roomWidth;
        int bottomRightY = randomY + roomHeight;

        if(bottomRightY < randomY){
            Debug.Log($"wrong roomHeight: {roomHeight}, maxY: {maxY}, minY: {minY}, randomY: {randomY}");
        }

        return new Vector4(randomX, randomY, bottomRightX, bottomRightY);
    }

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
