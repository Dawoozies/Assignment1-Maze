using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGeneration : MonoBehaviour
{
    public GameObject floorPrefab;
    public Vector2 yPercentageBounds;
    public Vector2 xPercentageBounds;
    public void CreateRooms(List<DungeonGeneration.Dungeon> subDungeons)
    {
        foreach (var subDungeon in subDungeons)
        {
            float yPercentageAlong = Random.Range(yPercentageBounds.x, yPercentageBounds.y);
            float xPercentageAlong = Random.Range(xPercentageBounds.x, xPercentageBounds.y);
            //Randomly choose center
        }
    }
}
