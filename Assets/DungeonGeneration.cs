using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
//Using guide on https://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation
public class DungeonGeneration : MonoBehaviour
{
    public int borderWidth, borderHeight;
    public int roomWidth, roomHeight;
    public int subdivisions;
    [Serializable]
    public class Dungeon
    {
        public Color assignedDebugColor;
        public Vector3 center;
        public float width, height;
        public Vector3 size => new Vector3(width, height, 0.1f);
        public Vector2 widthBorders => new Vector2(center.x - width/2f, center.x + width/2f);
        public Vector2 heightBorders => new Vector2(center.y - height / 2f, center.y + height / 2f);
        public Vector2 bottomLeft => new Vector2(widthBorders.x, heightBorders.x);
        public Vector2 bottomRight => new Vector2(widthBorders.y, heightBorders.x);
        public Vector2 topLeft => new Vector2(widthBorders.x, heightBorders.y);
        public Vector2 topRight => new Vector2(widthBorders.y, heightBorders.y);
        public Dungeon(float width, float height, Vector3 center)
        {
            this.width = width;
            this.height = height;
            this.center = center;
        }
        public Dungeon(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
        {
            width = Vector3.Distance(bottomLeft, bottomRight);
            height = Vector3.Distance(bottomLeft, topLeft);
            center = new Vector3(
                    (bottomLeft.x + bottomRight.x)/2f,
                    (bottomLeft.y + topLeft.y)/2f,
                    0f
                );
        }
    }
    public List<Dungeon> subDungeons = new();
    void Start()
    {
        Generate();
    }
    public bool generate;
    void Update()
    {
        if (generate)
        {
            Generate();
            generate = false;
        }
    }
    void Generate()
    {
        subDungeons = new();
        Dungeon startDungeon = new Dungeon(borderWidth, borderHeight, Vector3.zero);
        subDungeons.Add(startDungeon);

        for (int i = 0; i < subdivisions; i++)
        {
            List<Dungeon> newSubDungeonList = new();
            foreach (Dungeon subDungeon in subDungeons)
            {
                float splitDirection = Random.Range(0f,1f);
                if(splitDirection > 0.5f)
                {
                    //Horizontal split --> Pick y value and cut
                    float horizontalCoordinateSplit = Random.Range(
                            subDungeon.widthBorders.x, 
                            subDungeon.widthBorders.y
                        );

                    Vector3 splitCoordinateBottom = new Vector3(horizontalCoordinateSplit, subDungeon.heightBorders.x, 0f);
                    Vector3 splitCoordinateTop = new Vector3(horizontalCoordinateSplit, subDungeon.heightBorders.y, 0f);
                    //left dungeon corners
                    //maintained corners are bottomLeft and topLeft
                    //new corners are bottomRight and
                    Dungeon leftDungeon = new Dungeon(
                            subDungeon.bottomLeft,
                            splitCoordinateBottom,
                            subDungeon.topLeft,
                            splitCoordinateTop
                        );
                    //right dungeon corners
                    //maintained corners are bottomRight and topRight
                    Dungeon rightDungeon = new Dungeon(
                            splitCoordinateBottom,
                            subDungeon.bottomRight,
                            splitCoordinateTop,
                            subDungeon.topRight
                        );
                    newSubDungeonList.Add(leftDungeon);
                    newSubDungeonList.Add(rightDungeon);
                }
                else
                {
                    //Vertical split --> Pick x value and cut
                    float verticalCoordinateSplit = Random.Range(
                            subDungeon.heightBorders.x, 
                            subDungeon.heightBorders.y
                        );
                    Vector2 splitCoordinateLeft = new Vector2(subDungeon.widthBorders.x, verticalCoordinateSplit);
                    Vector2 splitCoordinateRight = new Vector2(subDungeon.widthBorders.y, verticalCoordinateSplit);
                    //top dungeon corners
                    //maintained corners are topLeft and topRight
                    //new corners are bottomLeft and bottomRight
                    Dungeon topDungeon = new Dungeon(
                            splitCoordinateLeft,
                            splitCoordinateRight,
                            subDungeon.topLeft,
                            subDungeon.topRight
                        );
                    //bottom dungeon corners
                    //maintained corners are bottomleft and bottomRight
                    //new corners are topLeft and topRight
                    Dungeon bottomDungeon = new Dungeon(
                            subDungeon.bottomLeft,
                            subDungeon.bottomRight,
                            splitCoordinateLeft,
                            splitCoordinateRight
                        );
                    newSubDungeonList.Add(topDungeon);
                    newSubDungeonList.Add(bottomDungeon);
                }
            }
            subDungeons = newSubDungeonList;
        }
        foreach (var dungeon in subDungeons)
        {
            dungeon.assignedDebugColor = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), 1f);
        }
    }
    void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }
        foreach (Dungeon dungeon in subDungeons)
        {
            Gizmos.color = dungeon.assignedDebugColor;
            Gizmos.DrawCube(new Vector3(dungeon.center.x, 0f, dungeon.center.y), new Vector3(dungeon.size.x, 0.1f, dungeon.size.y));
        }
    }
}
