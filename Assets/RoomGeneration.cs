using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DungeonGeneration;

public class RoomGeneration : MonoBehaviour
{
    public GameObject floorPrefab;
    [Tooltip("Bounds = (1stCutMin, 1stCutMax)")]
    public Vector2 xFirstCutBounds;
    [Tooltip("Bounds = (firstXCut + 2ndCutMin, 1 - 2ndCutMax)")]
    public Vector2 xSecondCutBounds;
    [Tooltip("Bounds = (1stCutMin, 1stCutMax)")]
    public Vector2 yFirstCutBounds;
    [Tooltip("Bounds = (firstXCut + 2ndCutMin, 1 - 2ndCutMax)")]
    public Vector2 ySecondCutBounds;
    public Vector2 objectScaleMultiplier;
    public List<Room> rooms = new();
    public class Room
    {
        public Transform floor;
        public Vector3 center;
        public float width, height;
        public Vector3 size => new Vector3(width, 1f, height);
        public Vector2 widthBorders => new Vector2(center.x - width / 2f, center.x + width / 2f);
        public Vector2 heightBorders => new Vector2(center.y - height / 2f, center.y + height / 2f);
        public Vector2 bottomLeft => new Vector2(widthBorders.x, heightBorders.x);
        public Vector2 bottomRight => new Vector2(widthBorders.y, heightBorders.x);
        public Vector2 topLeft => new Vector2(widthBorders.x, heightBorders.y);
        public Vector2 topRight => new Vector2(widthBorders.y, heightBorders.y);
        public Room(float width, float height, Vector3 center)
        {
            this.width = width;
            this.height = height;
            this.center = center;
        }
        public Room(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, Vector2 scale)
        {
            width = Vector3.Distance(bottomLeft, bottomRight)*scale.x;
            height = Vector3.Distance(bottomLeft, topLeft)*scale.y;
            center = new Vector3(
                    (bottomLeft.x + bottomRight.x) / 2f,
                    0f,
                    (bottomLeft.y + topLeft.y) / 2f
                );
        }
        public void CreateFloorObject(GameObject floorPrefab)
        {
            GameObject floorObject = Instantiate(floorPrefab);
            floor = floorObject.transform;
            floor.position = center;
            floor.localScale = new Vector3(size.x, 1f, size.z);
        }
        public void DeleteRoomObjects()
        {
            if(floor != null)
            {
                Destroy(floor.gameObject);
            }
        }
    }
    public void CreateRooms(List<Dungeon> subDungeons, ref Dictionary<Dungeon, Room> roomDungeonMap)
    {
        if(rooms != null && rooms.Count > 0)
        {
            foreach (Room room in rooms)
            {
                room.DeleteRoomObjects();
            }
        }
        rooms = new List<Room>();
        foreach (var subDungeon in subDungeons)
        {
            float firstXCut = Random.Range(xFirstCutBounds.x, xFirstCutBounds.y);
            float secondXCut = Random.Range(firstXCut + xSecondCutBounds.x, 1 - xSecondCutBounds.y);

            float firstYCut = Random.Range(yFirstCutBounds.x, yFirstCutBounds.y);
            float secondYCut = Random.Range(firstYCut + ySecondCutBounds.x, 1 - ySecondCutBounds.y);

            float firstX = Mathf.Lerp(subDungeon.bottomLeft.x, subDungeon.bottomRight.x, firstXCut);
            float secondX = Mathf.Lerp(subDungeon.bottomLeft.x, subDungeon.bottomRight.x, secondXCut);
            float firstY = Mathf.Lerp(subDungeon.bottomLeft.y, subDungeon.topLeft.y, firstYCut);
            float secondY = Mathf.Lerp(subDungeon.bottomLeft.y, subDungeon.topLeft.y, secondYCut);

            Vector2 bottomLeft = new(firstX, firstY);
            Vector2 bottomRight = new(secondX, firstY);
            Vector2 topLeft = new(firstX, secondY);
            Vector2 topRight = new(secondX, secondY);

            Room newRoom = new Room(bottomLeft, bottomRight, topLeft, topRight, objectScaleMultiplier);
            newRoom.CreateFloorObject(floorPrefab);
            rooms.Add(newRoom);
            roomDungeonMap.TryAdd(subDungeon, newRoom);
        }
    }
}
