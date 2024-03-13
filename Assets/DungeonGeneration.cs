using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
//Using guide on https://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation
public class DungeonGeneration : MonoBehaviour
{
    public enum DebugType
    {
        Normal, Depth, AnimateDepth
    }
    public int borderWidth, borderHeight;
    public int roomWidth, roomHeight;
    public int iterations;
    public Vector2 percentageBounds;
    public Color depthColor;
    public DebugType debugType;
    public int depthToShow;
    float depthAnimateTime = 0.5f;
    float depthAnimateTimer;
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
        public int depth;
        public int noSisterMidGenCheck;
        public bool hasSister;
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
        public bool ValidSize(float roomWidth, float roomHeight)
        {
            if(width < roomWidth || height < roomHeight)
            {
                return false;
            }

            return true;
        }
    }
    public List<Dungeon> subDungeons = new();
    Dictionary<Dungeon, List<Dungeon>> dungeonChildrenMap = new();
    Dictionary<int, List<Dungeon>> dungeonDepthMap = new();
    Dictionary<Dungeon, Dungeon> sisterMap = new();
    void Start()
    {
    }
    void Update()
    {
        if(debugType == DebugType.AnimateDepth)
        {
            if(depthAnimateTimer < depthAnimateTime)
            {
                depthAnimateTimer += Time.deltaTime;
            }
            else
            {
                depthAnimateTimer = 0;
                depthToShow++;
                depthToShow %= dungeonDepthMap.Keys.Count;
            }
        }
    }
    public List<Dungeon> Generate()
    {
        subDungeons = new();
        dungeonChildrenMap = new();
        dungeonDepthMap = new();
        sisterMap = new();
        Dungeon startDungeon = new Dungeon(borderWidth, borderHeight, Vector3.zero);
        startDungeon.depth = 0;
        subDungeons.Add(startDungeon);
        UpdateDepthMap(startDungeon);
        for (int i = 0; i < iterations; i++)
        {
            List<Dungeon> newSubDungeonList = new();
            foreach (Dungeon subDungeon in subDungeons)
            {
                float splitDirection = Random.Range(0f,1f);
                //How far along the edge to cut
                float percentageAlong = Random.Range(percentageBounds.x, percentageBounds.y);
                if (splitDirection > 0.5f)
                {
                    //Horizontal split --> Pick y value and cut
                    float horizontalCoordinateSplit = Mathf.Lerp(subDungeon.widthBorders.x, subDungeon.widthBorders.y, percentageAlong);
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
                    if(leftDungeon.ValidSize(roomWidth, roomHeight) && rightDungeon.ValidSize(roomWidth, roomHeight))
                    {
                        leftDungeon.depth = subDungeon.depth + 1;
                        rightDungeon.depth = subDungeon.depth + 1;
                        List<Dungeon> children = new List<Dungeon>() { leftDungeon, rightDungeon};
                        newSubDungeonList.AddRange(children);
                        SetChildren(subDungeon, children);
                        UpdateDepthMap(leftDungeon);
                        UpdateDepthMap(rightDungeon);
                    }
                    else
                    {
                        newSubDungeonList.Add(subDungeon);
                    }
                }
                else
                {
                    //Vertical split --> Pick x value and cut
                    float verticalCoordinateSplit = Mathf.Lerp(subDungeon.heightBorders.x, subDungeon.heightBorders.y, percentageAlong);
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
                    if(topDungeon.ValidSize(roomWidth, roomHeight) && bottomDungeon.ValidSize(roomWidth, roomHeight))
                    {
                        topDungeon.depth = subDungeon.depth + 1;
                        bottomDungeon.depth = subDungeon.depth + 1;
                        List<Dungeon> children = new List<Dungeon>() { topDungeon, bottomDungeon};
                        newSubDungeonList.AddRange(children);
                        SetChildren(subDungeon, children);
                        UpdateDepthMap(topDungeon);
                        UpdateDepthMap(bottomDungeon);
                    }
                    else
                    {
                        newSubDungeonList.Add(subDungeon);
                    }
                }
            }
            subDungeons = newSubDungeonList;
            foreach (Dungeon dungeon in subDungeons)
            {
                dungeon.assignedDebugColor = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f),0.75f);
            }
        }
        foreach (var item in dungeonChildrenMap)
        {
            //Debug.LogError($"Parent Depth = {item.Key.depth} Children ({item.Value[0].depth} valid = {item.Value[0].ValidSize(roomWidth, roomHeight)}, {item.Value[1].depth} valid = {item.Value[1].ValidSize(roomWidth, roomHeight)})");
            List<Dungeon> children = item.Value;
            SetSisters(children[0], children[1]);
        }
        return subDungeons;
    }
    void SetChildren(Dungeon dungeon, List<Dungeon> children)
    {
        if(dungeonChildrenMap.ContainsKey(dungeon))
        {
            dungeonChildrenMap[dungeon].AddRange(children);
        }
        else
        {
            dungeonChildrenMap.Add(dungeon, children);
        }
    }
    void SetSisters(Dungeon sisterA, Dungeon sisterB)
    {
        sisterMap.TryAdd(sisterA, sisterB);
    }
    void UpdateDepthMap(Dungeon dungeon)
    {
        if(dungeonDepthMap.ContainsKey(dungeon.depth))
        {
            dungeonDepthMap[dungeon.depth].Add(dungeon);
        }
        else
        {
            dungeonDepthMap.Add(dungeon.depth, new List<Dungeon>() { dungeon });
        }
    }
    void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }
        if(debugType == DebugType.Depth || debugType == DebugType.AnimateDepth)
        {
            if(dungeonDepthMap.ContainsKey(depthToShow))
            {
                List<Dungeon> dungeonsAtDepth = dungeonDepthMap[depthToShow];
                if(dungeonsAtDepth != null && dungeonsAtDepth.Count > 0)
                {
                    for (int i = 0; i < dungeonsAtDepth.Count; i++)
                    {
                        Color gizmoColor = depthColor / (i + 1);
                        gizmoColor.a = 0.75f;
                        Gizmos.color = gizmoColor;
                        Dungeon dungeon = dungeonsAtDepth[i];
                        Gizmos.DrawCube(new Vector3(dungeon.center.x, 0f, dungeon.center.y), new Vector3(dungeon.size.x, 0.1f, dungeon.size.y));
                    }
                }
            }
        }
        if(debugType == DebugType.Normal)
        {
            foreach (Dungeon dungeon in subDungeons)
            {
                Gizmos.color = dungeon.assignedDebugColor;
                Gizmos.DrawCube(new Vector3(dungeon.center.x, 0f, dungeon.center.y), new Vector3(dungeon.size.x, 0.1f, dungeon.size.y));
            }
        }
    }
    public Dungeon GetSister(Dungeon dungeon)
    {
        if(!sisterMap.ContainsKey(dungeon))
        {
            return null;
        }
        return sisterMap[dungeon];
    }
    public void CheckIfNoSister(List<Dungeon> children)
    {
        bool childZeroValidCheck = children[0].ValidSize(roomWidth, roomHeight);
        bool childOneValidCheck = children[1].ValidSize(roomWidth, roomHeight);
        if(childZeroValidCheck && !childOneValidCheck)
        {
            //then child zero will have no sister
            children[0].noSisterMidGenCheck = 1;
        }
        if(!childZeroValidCheck && childOneValidCheck)
        {
            //then child one will have no sister
            children[1].noSisterMidGenCheck = 1;
        }
    }
}
