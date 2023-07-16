using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class EnemyShipLogic : EnemyPlaneLogic
{

    //  INSPECTOR VARIABLES      //

    [SerializeField]
    protected List<GameObject> _cannons;

    [SerializeField]
    protected List<MeshRenderer> _renderers;

    [SerializeField]
    protected List<MeshRenderer> _renderersDebris;

    //  PRIVATE VARIABLES         //

    protected int _moveDir = 0;
    protected float _barrageDur = 1f;
    protected int _barrageNumShots = 3;
    protected float _barrageDelay;
    protected float _nextBarrage = 0;
    protected float _nextCannonTurn = 0;
    protected float _fireBarrage = 0;
    protected bool _destroyed = false;
    protected float _sinkTime = 0;
    protected float _sinkDuration = 6;
    protected float _sinkHeight = -2;
    protected Rigidbody2D _shipBody;
    protected PolygonCollider2D _shipCollider;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _shipBody = GetComponent<Rigidbody2D>();
        _shipCollider = GetComponent<PolygonCollider2D>();
        _projectilePrefab = _game.GetProjectileType(_projectileType);
        SetTeam(2);
        SetHealth(200);
        SetMaxHealth(200);
        _barrageDelay = 2;
        _fireDelay = _barrageDur/_barrageNumShots;
        _smokeThreshold = 0.5f;
    }


    protected override void Update()
    {
        if (_game == null) { return; }


        var player = _game.PlayerEntity;

        if (player.activeSelf && !_destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            float dist_x_abs = Mathf.Abs(playerPos.x - _shipBody.transform.position.x);

            if (dist_x_abs > 60)
            {
                if (playerPos.x > _shipBody.transform.position.x)
                    _moveDir = 1;
                else
                    _moveDir = -1;
            }
            else
                _moveDir = 0;

            if (_moveDir == 0)
                FireProjectile();

            if ( _nextCannonTurn <= Time.time )
            { 
                var cannon_pos = _cannons[0].transform.position;
                Vector3 dir = (playerPos - cannon_pos).normalized;
                dir.x = Mathf.Clamp(dir.x, -0.8f, 0.8f);
                dir.y = Mathf.Clamp(dir.y, 0f, 1);

                _cannons[0].transform.up = Vector3.RotateTowards(_cannons[0].transform.up, dir, 5 * Time.deltaTime, 0.0f);
                _cannons[0].transform.eulerAngles = new Vector3( 0, 0, _cannons[0].transform.eulerAngles.z );
            }
        }

        DoSmokeEffect();

    }

    private void FixedUpdate()
    {
        if (_game == null) { return; }

        if ( _moveDir != 0 && !_destroyed )
        {
            var move_pos = new Vector2(_moveDir * _maxSpeed * Time.fixedDeltaTime, 0);
            _shipBody.MovePosition(_shipBody.position + move_pos);
        }

        if ( _destroyed && _sinkTime >= Time.time )
        {
            float sink = _sinkHeight * Mathf.Clamp(1 - (_sinkTime - Time.time) / _sinkDuration, 0, 1);
            var move_pos = new Vector2( 0, sink * Time.fixedDeltaTime);
            _shipBody.MovePosition(_shipBody.position + move_pos);
        }

    }

    protected override void LateUpdate()
    {
        if (_game == null) { return; }

        var update_pos = _shipBody.transform.position;
        update_pos.x = _game.calculateLoopingX(update_pos.x);
        if (!_destroyed)
            update_pos.y = _game.GetWaterLevel();

        _shipBody.transform.position = update_pos;
    }

    protected override void FireProjectile()
    {
        if (_nextBarrage <= Time.time)
        {
            _nextCannonTurn = _fireDelay * _barrageNumShots + Time.time;
            _fireBarrage = _nextCannonTurn * 1;
            _nextBarrage = _barrageDelay + Time.time + UnityEngine.Random.Range( 0.4f, 0.9f) ;
        }

        if (_nextFire <= Time.time && _fireBarrage > Time.time)
        {
            var spawn_pos = _cannons[0].transform.position + _cannons[0].transform.up * 2.2f;

            var dir = _cannons[0].transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range(-1, 1)) * dir;

            _game.CreateProjectile(_projectilePrefab, spawn_pos, dir, 80, 25, GetTeam(), Vector3.zero);

            PlaySound(SearchAudioName("bullet"),0.5f);

            _nextFire = _fireDelay + Time.time;   
        }
       
    }

    protected override void DoDestroy()
    {
        if (_destroyed)
            return;

        _destroyed = true;
        _moveDir = 0;

        var exp_pos = _shipBody.transform.position;
        exp_pos.z -= 2;

        _shipCollider.enabled = false;

        _game.DoScreenshake(_shipBody.transform.position, 8, 1);

        GameObject explosionObj = Instantiate(_explosionFx, exp_pos, Quaternion.identity);

        foreach ( MeshRenderer renderer in _renderers )
        {
            renderer.enabled = false;
        }

        foreach (MeshRenderer renderer in _renderersDebris)
        {
            renderer.enabled = true;
        }

        _sinkTime = Time.time + _sinkDuration;

        Invoke("realDoDestroy", _sinkDuration);

        PlaySound(SearchAudioName("explosion"));

        _game.AddScore(score, _shipBody.transform.position);
        _game.DecrementEnemyCount();
       
    }

    private void RealDoDestroy()
    {
        Destroy(gameObject);
    }
}
