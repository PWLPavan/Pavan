using UnityEngine;
using System.Collections;

public class SteerableBehavior : MonoBehaviour, ISteerable {

    #region Members
    private int _state;

    public const int STATE_IDLE = 0;
    public const int STATE_MOVING = 1;

    private bool _isGlobal = false;
    private float _time;
    private Vector3 _start;
    private Vector3 _target;

    public float speed = 1.0f;

    public delegate void MovementDelegate(SteerableBehavior obj = null);

    [HideInInspector]
    public MovementDelegate onMoveEnd;
    #endregion

    #region Ctrl
    public virtual void Awake () {
        _state = STATE_IDLE;
    }

    public virtual void Start () {
    
    }

    public virtual void Update () {
    
        switch (_state) {
            case STATE_MOVING:
                Move ();
                break;
        }

    }
    #endregion

    #region ISteerable Methods
    public bool IsMoving
    {
        get { return _state == STATE_MOVING; }
    }

    public void Move () {
        _time += (Time.deltaTime * speed);
        if (_time > 1f)
            _time = 1f;

        if (!_isGlobal) {
            this.transform.localPosition = Vector3.Lerp(_start, _target, _time);
            if (_time == 1f) {//if (this.transform.localPosition == _target) {
                this.EndMove();
            }
        } else {
            this.transform.position = Vector3.Lerp(_start, _target, _time);
            if (_time == 1f) {//if (this.transform.position == _target) {
                this.EndMove();
            }
        }

    }
    
    public virtual void StartMove (Vector3 targetPosition, bool isGlobal = false, float spd = 3.0f, bool factorDist = false) {
        _isGlobal = isGlobal;
        _start = (!_isGlobal) ? this.transform.localPosition : this.transform.position;
        _target = targetPosition;
        _time = 0;	// set movement time to 0 (lerping to 1)

        // derive speed from distance
        if (factorDist)
            speed = spd / Vector3.Distance(_start, _target);
        else
            speed = spd;


        //speed = spd;

        _state = STATE_MOVING;
    }

    public virtual void EndMove () {
        _state = STATE_IDLE;
        if (onMoveEnd != null)
            onMoveEnd(this);
    }
    #endregion

}
