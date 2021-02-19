using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UniRx;
using UniRx.Triggers;
public enum PathMovementStyle
{
    Continuous,
    Slerp,
    Lerp,
}
public class PathController : MonoBehaviour
{
    public float MovementSpeed;
    public Transform PathContainer;
    public Transform JumpContainer;
    [SerializeField] List<int> JumpStartPoint = new List<int> {1,3,5,7 };
    [SerializeField] List<int> JumpEndPoint = new List<int> { 2, 4, 6, 8 };
    private List<int> CurrentJumpPoints = new List<int>();
    
    public bool IsMoving;
    public PathMovementStyle MovementStyle;
    public bool LoopThroughPoints;
    public bool StartAtFirstPointOnAwake;
    [SerializeField] Animator fishAnim;
    private Transform[] _points;
    private Transform[] _jumpPoints;

    Quaternion rotTarget = new Quaternion();
    private int _currentTargetIdx;
    private float startSpeed;
    public ReactiveProperty<int> _currentTargetIdxReactive = new ReactiveProperty<int>();
    public ReactiveProperty<bool> fishJump = new ReactiveProperty<bool>();
    public bool IsJumping;
    public int randJump;
    int randwhereStart;
    public bool catched;
    private void Awake()
    {
        PathContainer = GameObject.Find("waypointsParent").transform;
        JumpContainer= GameObject.Find("jumpPointsParent").transform;
        _jumpPoints= JumpContainer.GetComponentsInChildren<Transform>();
        _points = PathContainer.GetComponentsInChildren<Transform>();
        if (StartAtFirstPointOnAwake)
        {
            transform.position = _points[0].position;
        }
    }
    void Start()
    {
        //fishAnim = GetComponent<Animator>();
        startSpeed = MovementSpeed;
        IsMoving = true;
        _currentTargetIdxReactive
            .Where(_=>IsMoving)
            .Do(_ => SetAngle(_points[_currentTargetIdx].position))
            .Do(_ => StartCoroutine(SlerpRot(transform.rotation, rotTarget, 2)))
            .Subscribe()
            .AddTo(this);
        ObserveFishJump();
    }
    void SetAngle(Vector3 destination)
    {
        Vector3 relativePos = destination - transform.position;

        rotTarget = Quaternion.LookRotation(relativePos, Vector3.up);
    }
    private void Update()
    {
        if (IsMoving) 
        {
            if (_points == null || _points.Length == 0) return;
            var distance = Vector3.Distance(transform.position, _points[_currentTargetIdx].position);

            if (Mathf.Abs(distance) < 0.1f) 
            {
                if (!fishJump.Value) {
                    randJump = UnityEngine.Random.Range(0, 11);

                    _currentTargetIdx = UnityEngine.Random.Range(0, _points.Length - 1);

                    _currentTargetIdxReactive.Value = _currentTargetIdx;
                    if (_currentTargetIdx >= _points.Length)
                    {
                        _currentTargetIdx = LoopThroughPoints ? 0 : _points.Length - 1;
                    }
                    if (randJump >= 4)
                    {
                        fishJump.Value = true;

                    }
                    else
                    {
                        fishJump.Value = false;
                    }
                }
                
            }
            switch (MovementStyle)
            {
                default:
                case PathMovementStyle.Continuous:
                    transform.position = Vector3.MoveTowards(transform.position, _points[_currentTargetIdx].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Lerp:
                    transform.position = Vector3.Lerp(transform.position, _points[_currentTargetIdx].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Slerp:
                    transform.position = Vector3.Slerp(transform.position, _points[_currentTargetIdx].position, MovementSpeed * Time.deltaTime);
                    break;
            }


        }





    }
     IEnumerator SlerpRot(Quaternion startRot, Quaternion endRot, float slerpTime)
    {
        float elapsed = 0;
        while (elapsed < slerpTime)
        {
            elapsed += Time.deltaTime;

            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / slerpTime);

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_points == null || _points.Length == 0) return;
        var idx = 0;
        foreach (var point in _points)
        {
            Gizmos.color = Color.yellow;
            if (idx < _currentTargetIdx)
            {
                Gizmos.color = Color.red;
            }

            if (idx > _currentTargetIdx)
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawWireSphere(point.position, 1f);
            idx++;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _points[_currentTargetIdx].position);
    }
    void ObserveFishJump()
    {
        fishJump
            .Where(_ => fishJump.Value == true)
            .Do(_ => fishjumpCond())
            .Do(_ => setJumpPoints())
            .Do(_=>IsMoving=false)
            .Do(_=> setJumpRot(randwhereStart))
            .Delay(TimeSpan.FromMilliseconds(1300))
            .Do(_ => fishAnim.enabled = false)
            .Do(_=>IsJumping=false)
            .Do(_ => IsMoving = true)
            .Do(_=> MovementSpeed = startSpeed)
            .Subscribe()
            .AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => IsJumping == true)
            .Where(_ => IsMoving == false)
            .Where(_=>catched==false)
            .Subscribe(_ => jumpPathMove(randwhereStart))
            .AddTo(this);
            
    }
    void fishjumpCond()
    {
        IsJumping = true;
        MovementSpeed = 0.4f;
        fishAnim.enabled = true;
        fishAnim.SetTrigger("Jump");
        fishJump.Value = false;
    }
    void setJumpPoints()
    {
        int randJumpPoint = UnityEngine.Random.Range(0, 4);
        CurrentJumpPoints.Clear();
        CurrentJumpPoints.Add(JumpStartPoint[randJumpPoint]);
        CurrentJumpPoints.Add(JumpEndPoint[randJumpPoint]);
        randwhereStart = UnityEngine.Random.Range(0, 1);
        transform.position = _jumpPoints[randwhereStart].position;
    }
    void jumpPathMove(int startIndex)
    {
        if (startIndex == 0)
        {
            
            switch (MovementStyle)
            {
                default:
                case PathMovementStyle.Continuous:
                    transform.position = Vector3.MoveTowards(transform.position, _jumpPoints[CurrentJumpPoints[1]].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Lerp:
                    transform.position = Vector3.Lerp(transform.position, _jumpPoints[CurrentJumpPoints[1]].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Slerp:
                    transform.position = Vector3.Slerp(transform.position, _jumpPoints[CurrentJumpPoints[1]].position, MovementSpeed * Time.deltaTime);
                    break;
            }
        }
        else
        {
           

            switch (MovementStyle)
            {
                default:
                case PathMovementStyle.Continuous:
                    transform.position = Vector3.MoveTowards(transform.position, _jumpPoints[CurrentJumpPoints[0]].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Lerp:
                    transform.position = Vector3.Lerp(transform.position, _jumpPoints[CurrentJumpPoints[0]].position, MovementSpeed * Time.deltaTime);
                    break;
                case PathMovementStyle.Slerp:
                    transform.position = Vector3.Slerp(transform.position, _jumpPoints[CurrentJumpPoints[0]].position, MovementSpeed * Time.deltaTime);
                    break;
            }
        }
    }
    void setJumpRot(int startIndex)
    {
        StopCoroutine("SlerpRot");

        if (startIndex == 0)
        {
            transform.LookAt(_jumpPoints[CurrentJumpPoints[1]]);
        }
        else
        {
            transform.LookAt(_jumpPoints[CurrentJumpPoints[0]]);

        }


    }


}