using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    [SerializeField]GameObject[] tilePrefabs;
    int tileSize = 5;
    // Start is called before the first frame update
    void Start()
    {
        SpawnLevel();
    }

    private void SpawnLevel(){
          byte[][] tiles = new byte[][]
        {
            new byte[] {0, 0, 0, 0, 0},
            new byte[] {0, 1, 1, 1, 0},
            new byte[] {0, 0, 0, 0, 0}
        };

        for(int y = 0; y < tiles.Length; y++){
            for(int x = 0; x < tiles[y].Length; x++){
                if (tiles[y][x] >= tilePrefabs.Length){
                    Debug.LogError("Tile value is higher than ntileTypes");
                    return;
                }
                Vector2 position = new Vector2(x, y) * tileSize;
                Instantiate(tilePrefabs[tiles[y][x]], position, quaternion.identity, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
