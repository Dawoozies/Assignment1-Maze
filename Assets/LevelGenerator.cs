using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject wallPrefab;
    public int width;
    public int height;
    GridPoint[,] grid;
    public float distanceBetweenGridPoints;
    public class GridPoint
    {
        public enum Type
        {
            Wall, Tile
        }
        public Type type;
        public int x, y;
        public GameObject gridObject;
        public Vector3 pos;
        public GridPoint(int x, int y, float distanceBetweenGridPoints)
        {
            this.x = x;
            this.y = y;
            pos = new Vector3(x * distanceBetweenGridPoints, 0, y * distanceBetweenGridPoints);
            type = Type.Wall;
        }
        public GridPoint(int x, int y, float distanceBetweenGridPoints, Type type)
        {
            this.x = x;
            this.y = y;
            pos = new Vector3(x * distanceBetweenGridPoints, 0, y * distanceBetweenGridPoints);
            this.type = type;
        }
    };
    void Start()
    {
        transform.position = new Vector3(-width*distanceBetweenGridPoints*0.5f, 0f, -height*distanceBetweenGridPoints*0.5f);
        grid = new GridPoint[width, height];
        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                grid[i,j] = new GridPoint(i,j, distanceBetweenGridPoints);
            }
        }

        List<GridPoint> frontier = new();
        int randomX = Random.Range(0, width);
        int randomY = Random.Range(0, height);
        grid[randomX, randomY].type = GridPoint.Type.Tile; //in maze if it is type tile
        frontier.AddRange(AdjacentGridPoints(randomX, randomY));


        int frontierCount = frontier.Count;
        int whileExit = 15000;
        while(frontierCount > 0)
        {
            GridPoint frontierMain = frontier[Random.Range(0, frontierCount)];
            List<GridPoint> adjToMain = AdjacentGridPoints(frontierMain.x, frontierMain.y);
            if (adjToMain.Count <= 0)
                continue;
            GridPoint adjToMainPoint = adjToMain[Random.Range(0, adjToMain.Count)];
            GridPoint inBetweenMainAndAdj = GridPointBetween(frontierMain, adjToMainPoint);
            adjToMainPoint.type = GridPoint.Type.Tile;
            inBetweenMainAndAdj.type = GridPoint.Type.Tile;
            frontier.Remove(frontierMain);
            frontierCount--;
            frontier.Remove(adjToMainPoint);
            if(frontier.Contains(inBetweenMainAndAdj))
            {
                frontier.Remove(inBetweenMainAndAdj);
            }
            List<GridPoint> toAdd = new();
            List<GridPoint> First = new();
            List<GridPoint> Second = new();
            List<GridPoint> Third = new();
            First = AdjacentGridPoints(frontierMain.x, frontierMain.y);
            Second = AdjacentGridPoints(adjToMainPoint.x, adjToMainPoint.y);
            Third = AdjacentGridPoints(inBetweenMainAndAdj.x, inBetweenMainAndAdj.y);
            foreach (var item in First)
            {
                if(toAdd.Contains(item))
                {
                    continue;
                }
                toAdd.Add(item);
            }
            foreach (var item in Second)
            {
                if(toAdd.Contains(item))
                {
                    continue;
                }
                toAdd.Add(item);    
            }
            foreach (var item in Third)
            {
                if (toAdd.Contains(item))
                {
                    continue;
                }
                toAdd.Add(item);
            }
            frontier.AddRange(toAdd);
            whileExit--;
            if (whileExit <= 0)
            {
                break;
            }
        }

        foreach (GridPoint gridPoint in grid)
        {
            if (gridPoint.type == GridPoint.Type.Wall)
            {
                gridPoint.gridObject = Instantiate(wallPrefab, transform);
            }
            if (gridPoint.type == GridPoint.Type.Tile)
            {
                gridPoint.gridObject = Instantiate(tilePrefab, transform);
            }
            gridPoint.gridObject.transform.localPosition = gridPoint.pos;
        }
    }
    List<GridPoint> AdjacentGridPoints(int x, int y)
    {
        int x0 = x - 2;
        int x1 = x + 2;
        int y0 = y - 2;
        int y1 = y + 2;
        List<GridPoint> adjacentGridPoints = new();
        if(x0 > 0)
        {
            GridPoint gridPoint = grid[x0, y];
            if(gridPoint.type == GridPoint.Type.Wall)
                adjacentGridPoints.Add(gridPoint);
        }
        if(x1 < width)
        {
            GridPoint gridPoint = grid[x1, y];
            if(gridPoint.type == GridPoint.Type.Wall)
                adjacentGridPoints.Add(gridPoint);
        }
        if (y0 > 0)
        {
            GridPoint gridPoint = grid[x, y0];
            if(gridPoint.type == GridPoint.Type.Wall)
                adjacentGridPoints.Add(gridPoint);
        }
        if (y1 < height)
        {
            GridPoint gridPoint = grid[x, y1];
            if(gridPoint.type == GridPoint.Type.Wall)
                adjacentGridPoints.Add(gridPoint);
        }
        return adjacentGridPoints;
    }
    GridPoint GridPointBetween(GridPoint gridPointA, GridPoint gridPointB)
    {
        int x = gridPointB.x;
        int y = gridPointB.y;
        if(gridPointA.x < gridPointB.x)
        {
            x = gridPointB.x - 1;
        }
        if(gridPointA.x > gridPointB.x)
        {
            x = gridPointB.x + 1;
        }
        if(gridPointA.y < gridPointB.y)
        {
            y = gridPointB.y - 1;
        }
        if(gridPointA.y > gridPointB.y)
        {
            y = gridPointB.y + 1;
        }
        return grid[x, y];
    }
    void Update()
    {
        
    }
}
