using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneLogic : PlayerLogic
{
    //  PRIVATE VARIABLES         //

    protected float _closeupRange;
    protected float _accelerationRange;
    protected float _desiredGoalAng = 30;
    protected float _attackRange = 30;
    //  INSPECTOR VARIABLES      //

    public int score;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _planeBody = GetComponent<Rigidbody2D>();
        _planeCollider = GetComponent<PolygonCollider2D>();
        _projectilePrefab = _game.GetProjectileType(_projectileType);
        SetTeam(2);
        SetMaxHealth(_maxHealth);
        SetHealth(GetMaxHealth());
        _maxSpeed = Random.Range(30f, 50f);
        _rotSpeed = Random.Range(100, 200);
        _fireDelay = Random.Range( 0.4f, 1.2f );
        _closeupRange = Random.Range(3, 10);
        _accelerationRange = Random.Range(20, 35);
    }

    protected virtual void Update()
    {
        if (!_game) { return; }

        var player = _game.PlayerEntity;

        if ( player.activeSelf )
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            playerPos.y = Mathf.Max(playerPos.y, _game.GetWaterLevel());

            Vector3 dir = (playerPos - _planeBody.transform.position).normalized;

            float goal_ang = Mathf.Atan2(dir.y, dir.x);
            goal_ang = (goal_ang / Mathf.PI) * 180;

            float real_rot = NormalizeAngle(_planeBody.rotation);
            float real_rot_abs = Mathf.Abs(real_rot);

            float diff = NormalizeAngle(goal_ang - _planeBody.rotation + 90);
            float diff_abs = Mathf.Abs(diff - 180);

            if (diff_abs > _desiredGoalAng)
            {
                if (diff > 0)
                    _rotInput = 1;
                else
                    _rotInput = -1;
            }
            else
                _rotInput = 0;


            float distSqr = (playerPos - _planeBody.transform.position).sqrMagnitude;

            if ( (distSqr > _accelerationRange * _accelerationRange && diff_abs < 40 ) || ( _planeBody.transform.position.y < (_game.GetWaterLevel() + 3) && real_rot_abs < 70 ))
            {
                _thrustInput = 1;
            }
            else
            {
                if ( distSqr < _closeupRange * _closeupRange)
                    _thrustInput = 1;
                else
                    _thrustInput = 0;
            }


            if (diff_abs < 40 && distSqr < _attackRange * _attackRange)
            {
                FireProjectile();
            }                

        }

        HandleEffects();

    }

    protected override void HandleEffects()
    {
        DoWaterEffect();
    }

    private void FixedUpdate()
    {
                if (!_game) { return; }

        HandleMotion();

        
    }

    protected virtual void LateUpdate()
    {
        if (_game == null) { return; }

        var update_pos = _planeBody.transform.position;
        float old_x = _planeBody.transform.position.x;
        update_pos.x = _game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            OnTeleported(old_x, update_pos.x);
        }


        if (_planeBody.transform.position.y < _game.LevelMins().y)
            update_pos.y = _game.LevelMins().y;
        if (_planeBody.transform.position.y > _game.LevelMaxs().y)
            update_pos.y = _game.LevelMaxs().y;

        _planeBody.transform.position = update_pos;
    }


    protected override void DoDestroy()
    {
        _game.DoScreenshake(_planeBody.transform.position, 2, 0.5f);
        Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);
        _game.AddScore(score, _planeBody.transform.position);
        _game.DecrementEnemyCount();
        gameObject.SetActive(false);
        Destroy(gameObject,1f);
    }

    protected virtual void OnTeleported( float old_x, float new_x)
    {

    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
      
    }

    protected override void OnTriggerStay2D(Collider2D other)
    {

    }

    protected override void DoMuzzleflash()
    {
        
    }

}
