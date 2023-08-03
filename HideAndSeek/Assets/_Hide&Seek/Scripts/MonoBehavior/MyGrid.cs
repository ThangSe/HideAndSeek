using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
    //public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;
    public List<Node> path;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
        SetAllNodeNeighbours();
    }

    private void Start()
    {
        
        MainManager.Instance.OnMapChanged += MainManager_OnMapChanged;
    }

    private void MainManager_OnMapChanged(object sender, System.EventArgs e)
    {
        UpdateGrid();
    }

    public Node[,] NodeGrid() {
        return grid;
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = Vector3.zero - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics2D.CircleCast(worldPoint, nodeRadius, Vector2.zero, 0, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    void SetAllNodeNeighbours()
    {
        if (grid != null)
        {
            foreach (Node node in grid)
            {
                node.SetNeighbours(GetNeighbours(node));
            }
        }
    }

    public void UpdateGrid()
    {
        if(grid != null)
        {
            
            foreach (Node node in grid)
            {
                bool walkable = !(Physics2D.CircleCast(node.GetWorldPos(), nodeRadius, Vector2.zero, 0, unwalkableMask));
                node.SetWalkable(walkable);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for( int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <=1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                if( checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY )
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    
    /*void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y));
        if(grid != null && displayGridGizmos)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = (node.walkable) ? Color.white : Color.red;          
                if(path!=null)
                {
                    if(path.Contains(node))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeRadius * 2 - .05f));
            }
        }
    }*/
}
