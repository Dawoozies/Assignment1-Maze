using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static DungeonGeneration;
using static RoomGeneration;
public class GameManager : MonoBehaviour
{
    public bool generate;
    DungeonGeneration dungeonGen;
    RoomGeneration roomGen;
    Dictionary<Dungeon, Room> roomDungeonMap = new();
    Dictionary<Room, Room> sisterRoomMap = new();
    void Start()
    {
        dungeonGen = GetComponent<DungeonGeneration>();
        roomGen = GetComponent<RoomGeneration>();
        Generate();
    }
    void Update()
    {
        if(generate)
        {
            Generate();
            generate = false;
        }
    }
    void Generate()
    {
        roomDungeonMap.Clear();
        sisterRoomMap.Clear();
        List<Dungeon> subDungeons = dungeonGen.Generate();
        roomGen.CreateRooms(subDungeons, ref roomDungeonMap);
        foreach (var item in roomDungeonMap)
        {
            Dungeon sister = dungeonGen.GetSister(item.Key);
            if(sister != null)
            {
                Debug.Log("sister in dungeon = " + roomDungeonMap.ContainsKey(sister));
                if(roomDungeonMap.ContainsKey(sister))
                {
                    Room sisterRoom = roomDungeonMap[sister];
                    sisterRoomMap.TryAdd(item.Value, sisterRoom);
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }
        if(sisterRoomMap == null || sisterRoomMap.Count == 0)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        foreach (var item in sisterRoomMap)
        {
            Gizmos.DrawLine(item.Key.center, item.Value.center);
        }
    }
}
