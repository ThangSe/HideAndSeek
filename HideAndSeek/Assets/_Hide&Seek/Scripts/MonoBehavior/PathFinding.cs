using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PathFinding : MonoBehaviour
{
    [SerializeField] private MyGrid _grid;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _spriteEnemyTF;
    private Heap<Node> openSet;
    private HashSet<Node> closedSet = new HashSet<Node>();
    private float _angleRotation;
    private float _speed;
    private float _speedMultipler;
    private float _speedMultiplerDefault;
    private Vector3 _moveDir;
    private Vector3 _facingRight = new Vector3(.5f, .5f, 1f);
    private Vector3 _facingLeft = new Vector3(-.5f, .5f, 1f);
    private bool _pathSuccess;
    private int _targetIndex;
    private Vector3[] _wayPoints;
    private float _delayTime;
    private float _delayTimeMax = .04f;
    [SerializeField] private Image[] _enemySprites;
    private SpriteRenderer _enemycurrentSprite;

    private void Awake()
    {
        _enemycurrentSprite = _spriteEnemyTF.GetComponent<SpriteRenderer>();
        _speed = MainManager.Instance.GetGameSettings().enemySpeed;
        _speedMultiplerDefault = MainManager.Instance.GetGameSettings().enemySpeedMultiplerDefault;
    }
    private void Start()
    {
        openSet = new Heap<Node>(_grid.MaxSize);
        MainManager.Instance.OnSpawnEnemy += MainManager_OnSpawnEnemy;
        _delayTime = _delayTimeMax;
    }

    private void MainManager_OnSpawnEnemy(object sender, EventArgs e)
    {
        _pathSuccess = false;
        _delayTime = _delayTimeMax;
        _speedMultipler = _speedMultiplerDefault;
        FindPath(transform.position, _player.position);
    }

    private void Update()
    {    
        if(MainManager.Instance.IsGamePlayingState())
        {
            _delayTime -=Time.deltaTime;
            if(_delayTime < 0)
            {
                FindPath(transform.position, _player.position);
                _delayTime += _delayTimeMax;
            }

            OnPathFound();
            _speedMultipler = _speedMultiplerDefault + (_speedMultiplerDefault - MainManager.Instance.PlayingTimerNomalized()) / 4;
        }    
    }

    public void FindPath(Vector3 currentPos, Vector3 targetPos)
    {
        _wayPoints = new Vector3[0];
        _pathSuccess = false;

        Node startNode = _grid.NodeFromWorldPoint(currentPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);
        if (!targetNode.walkable)
        {
            foreach (Node newTargetNode in targetNode.neighbours)
            {
                if (newTargetNode.walkable)
                {
                    targetNode = newTargetNode;
                    break;
                }
            }
        }
        if (targetNode.walkable)
        {
            closedSet.Clear();
            openSet.Clear();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    _pathSuccess = true;
                }

                foreach (Node neighbour in currentNode.neighbours)
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                };
            }
        }
        if (_pathSuccess)
        {
            _wayPoints = RetracePath(startNode, targetNode);
            _pathSuccess = _wayPoints.Length > 0;
        }
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path, startNode);
        Array.Reverse(waypoints);
        //_grid.path = path;
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path, Node startNode)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }
            directionOld = directionNew;
            if (i == path.Count - 1 && directionOld != new Vector2(path[i].gridX, path[i].gridY) - new Vector2(startNode.gridX, startNode.gridY))
                waypoints.Add(path[path.Count - 1].worldPosition);
        }
        return waypoints.ToArray();
    }

    private void OnPathFound()
    {
        if (_pathSuccess)
        {
            _targetIndex = 0;
            Vector3 currentWayPoint = _wayPoints[0];
            if (transform.position == currentWayPoint)
            {
                _targetIndex++;
                if (_targetIndex >= _wayPoints.Length)
                {
                    return;
                }
                currentWayPoint = _wayPoints[_targetIndex];
            }
            _angleRotation = Mathf.Atan2(_wayPoints[_targetIndex].y - transform.position.y, _wayPoints[_targetIndex].x - transform.position.x) * Mathf.Rad2Deg;
            transform.GetChild(0).transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, _angleRotation);
            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, Time.deltaTime * _speed * _speedMultipler);
            _moveDir = (currentWayPoint - transform.position).normalized;
            SpriteChangedFacingDir();
        }
    }

    private int _spriteRunFont = 1;
    private int _spriteRunSide = 3;
    private int _spriteRunBack = 5;
    private void SpriteChangedFacingDir()
    {
        if (_moveDir == Vector3.zero)
        {
            _enemycurrentSprite.sprite = _enemySprites[0].sprite;
        }
        if (_moveDir.x > 0 || _moveDir.x < 0 && _moveDir.y < 0)
        {
            _enemycurrentSprite.sprite = _enemySprites[_spriteRunSide].sprite;
            _spriteRunSide++;
            if (_moveDir.x > 0) _spriteEnemyTF.localScale = _facingRight;
            if (_moveDir.x < 0) _spriteEnemyTF.localScale = _facingLeft;
            
            if (_spriteRunSide > 4) _spriteRunSide = 3;
        }
        if (_moveDir.y < 0f && _moveDir.x == 0f)
        {
            _enemycurrentSprite.sprite = _enemySprites[_spriteRunFont].sprite;
            _spriteRunFont++;
            if (_spriteRunFont > 2) _spriteRunFont = 1;
        }
        if (_moveDir.y > 0f)
        {
            _enemycurrentSprite.sprite = _enemySprites[_spriteRunBack].sprite;
            _spriteRunBack++;
            if (_spriteRunBack > 6) _spriteRunBack = 5;
        }
    }

    int GetDistance (Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY- nodeB.gridY);

        if(dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
