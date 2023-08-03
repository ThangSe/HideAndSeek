using System;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemy : IState
{

    public static event EventHandler OnCatchPLayer;

    public static  bool _isCatchPlayer;

    private GameObject _object;
    private Transform _target;
    private float _viewRadius = 3;
    private float _viewAngle = 60;
    private float _meshResolution = .1f;
    private int _edgeResolveIterations = 4;
    private float _edgeDstThreshold = .5f;

    private LayerMask _obstacleMask;
    private LayerMask _playerMask;
    private Mesh _mesh;



    public NewEnemy (GameObject newObject, Transform target, SOEnemyRefs enemyRefsSO, Vector3 pos)
    {
        _viewRadius = enemyRefsSO.viewRadius;
        _viewAngle = enemyRefsSO.viewAngle;
        _object = newObject;
        _object.SetActive(true);
        _target = target;
        _obstacleMask = 64;
        _playerMask = 128;
        _mesh = new Mesh();
        _object.transform.GetComponentInChildren<MeshFilter>().mesh = _mesh;
        _object.transform.position = pos;
    }
    public void Act()
    {
        FindVisibleTargets();
        DrawFieldOfView();
    }

    public void Activate(Vector3 pos, Vector3 dir)
    {
        _object.transform.position = pos;
        _object.SetActive(true);
    }

    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetState()
    {
        return _object.activeSelf;
    }
    public void SettingSprite(Sprite newSprite, int index)
    {
        throw new System.NotImplementedException();
    }

    void FindVisibleTargets()
    {
        Collider2D targetsInViewRadius = Physics2D.OverlapCircle(_object.transform.position, _viewRadius, _playerMask);
        if (targetsInViewRadius != null )
        {
            Vector3 dirToTarget = (_target.position - _object.transform.position).normalized;
            if(Vector3.Angle(_object.transform.right, dirToTarget) < _viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(_object.transform.position, _target.position);
                if(!Physics2D.Raycast(_object.transform.position, dirToTarget, dstToTarget, _obstacleMask) && !_isCatchPlayer)
                {
                    OnCatchPLayer?.Invoke(this, EventArgs.Empty);
                    _isCatchPlayer =!_isCatchPlayer;
                }
            }
        }
    }
    public Vector3 DirFromAngle(float angle, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angle += _object.transform.eulerAngles.z;
        }
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(_viewAngle * _meshResolution);
        float stepAngleSize = _viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -_object.transform.eulerAngles.z + 90 - _viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            if(i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for(int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = _object.transform.InverseTransformPoint(viewPoints[i]);
            if(i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for(int i = 0; i < _edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast (float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(_object.transform.position, dir, _viewRadius, _obstacleMask);
        if(raycastHit2D)
        {
            return new ViewCastInfo(true, raycastHit2D.point, raycastHit2D.distance, globalAngle);
        } else
        {
            return new ViewCastInfo(false, _object.transform.position + dir * _viewRadius, _viewRadius, globalAngle);
        }

    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;
        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
