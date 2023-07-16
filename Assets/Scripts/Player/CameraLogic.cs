using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _player;

    //  PRIVATE VARIABLES         //

    private Rigidbody2D _playerBody;
    private PlayerLogic _playerLogic;
    private Camera _cameraObj;

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    private Vector2 _offsetPosition = new Vector2(0, 0);
    private Vector3 _screenShakePos = new Vector3(0, 0, 0);

    private float _screenShakeAmount = 0;
    private float _screenShakeDuration = 1;
    private float _screenShakeTime = 0;

    //  PRIVATE METHODS           //


    private void Awake()
    {
        if (_player== null)
        {
            _player = GameObject.Find("Player");
        }
    }
    private void Start()
    {
        _cameraObj = GetComponent<Camera>();
        _playerBody = _player.GetComponent<Rigidbody2D>();
        _playerLogic = _player.GetComponent<PlayerLogic>();
    }

    private void Update()
    {
        if (_game==null) { return; }

        var goal_direction = _playerLogic.GetVelDir();

        if (_playerLogic.IsAccelerating())
            goal_direction = _playerLogic.GetAimDir();

        ApproachPosition(_playerBody.transform.position, goal_direction, _playerLogic.GetThrustDelta() * 10, _playerLogic.GetAimDir(), 10);
        ApplyScreenshake();
    }

    private void ApproachPosition(Vector2 pos, Vector2 offset, float offsetSize, Vector2 constantOffset, float constantOffsetSize)
    {
        var goal = new Vector2(offset.x * offsetSize + constantOffset.x * constantOffsetSize, offset.y * offsetSize + constantOffset.y * constantOffsetSize);

        _offsetPosition = Vector2.Lerp(_offsetPosition, goal, 3f * Time.deltaTime);

        _cameraObj.transform.position = new Vector3(pos.x + _offsetPosition.x, Mathf.Clamp(pos.y + _offsetPosition.y, _game.LevelMins().y + _cameraObj.orthographicSize, _game.LevelMaxs().y - _cameraObj.orthographicSize), _cameraObj.transform.position.z);
    }
  

    private void ApplyScreenshake()
    {

        if (_screenShakeAmount != 0 && _screenShakeTime < Time.time)
        {
            _screenShakeAmount = 0;
        }
        else
        {
            float delta = Mathf.Clamp((_screenShakeTime - Time.time ) / _screenShakeDuration, 0, 1 );

            var cam_pos = _cameraObj.transform.position;
            cam_pos.z = 0;

            var shake_dir = (_screenShakePos - cam_pos).normalized;

            var add_shake = Vector3RandDir(_screenShakeAmount / 2 * delta, _screenShakeAmount * 1.1f * delta, shake_dir);
            add_shake += Vector3Rand(-_screenShakeAmount / 3 * delta, _screenShakeAmount / 3 * delta);
            add_shake.z = 0;

            _cameraObj.transform.position += add_shake * Time.deltaTime * 143;
        }

    }

    private Vector3 Vector3RandDir(float min, float max, Vector3 dir)
    {
        return new Vector3(Random.Range(min, max) * dir.x, Random.Range(min, max) * dir.y, Random.Range(min, max) * dir.z);
    }

    private Vector3 Vector3Rand( float min, float max )
    {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }

    //  PUBLIC API               //


    public void DoScreenshake(Vector3 pos, float am, float dur)
    {
        var cam_pos = _cameraObj.transform.position;
        cam_pos.z = 0;

        pos.z = 0;

        // make sure that bigger screenshake has priority over the small ones
        if (_screenShakeAmount <= am)
        {
            _screenShakePos = pos;
            _screenShakeAmount = am;
            _screenShakeTime = Time.time + dur;
        }
    }
}
