using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneJetLogic : EnemyPlaneLogic
{
    //  PRIVATE VARIABLES         //

    private bool _destroyed = false;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _planeBody = GetComponent<Rigidbody2D>();
        _planeCollider = GetComponent<PolygonCollider2D>();
        _projectilePrefab = _game.GetProjectileType(_projectileType);
        SetTeam(2);
        SetMaxHealth(_maxHealth);
        SetHealth(GetMaxHealth());
        _maxSpeed = Random.Range(90f, 100f);
        _rotSpeed = Random.Range(220, 230);
        _fireDelay = 0.2f;
        _accelerationRange = Random.Range(60, 65);
    }


    protected override void Update()
    {
        if (_game == null) { return; }

        var player = _game.PlayerEntity;

        if (player.activeSelf && !_destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            
            playerPos.y = Mathf.Max(playerPos.y, _game.GetWaterLevel() + _game.GetWaterLevelHeight());

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

            if ((distSqr < _accelerationRange * _accelerationRange) || (distSqr > _accelerationRange * _accelerationRange && diff_abs < 30) || (_planeBody.transform.position.y < (_game.GetWaterLevel() + 3) && real_rot_abs < 80) || (_planeBody.transform.position.y > (_game.GetSkyLevel() - 3) && real_rot_abs > 140))
            {
                _thrustInput = 1;
            }
            else
            {
                _thrustInput = 0;
            }

            var ahead_vec = _planeBody.transform.position + _planeBody.transform.up * _game.GetWaterLevelHeight();

            if (ahead_vec.y < _game.GetWaterLevel() && _planeBody.transform.position.y > _game.GetWaterLevel() || ahead_vec.y > _game.GetSkyLevel() && _planeBody.transform.position.y > _game.GetSkyLevel())
                _thrustInput = 0;

            if (diff_abs < 40 && distSqr < 50 * 50)
            {
                FireProjectile();
            }
        }

        if (_destroyed)
        {
            _thrustInput = 0;
            _rotInput = 1;
            _rotSpeed = 1300;
            if (_planeBody.transform.position.y < _game.GetWaterLevel())
                RealDoDestroy();
        }
        else
            HandleEnvDamage( true );

        
        HandleEffects();
    }

    protected override void HandleEffects()
    {
        DoSmokeEffect();
        DoJetEffect();
        DoWaterEffect();
    }

    protected override float GetRotPower()
    {
        if (IsAccelerating())
            return 0.15f;
        else
            return 1;
    }

    protected override void OnTeleported(float old_x, float new_x)
    {
        var trail = GetComponentInChildren<TrailRenderer>();

        Vector3 new_offset = new Vector3(_game.LevelWidth, 0, 0);

        if (new_x < old_x)
            new_offset *= -1;


        for ( int i = 0; i < trail.positionCount; i++ )
        {
            trail.SetPosition(i, trail.GetPosition(i) + new_offset);
        }

    }

    protected override void DoDestroy()
    {
        if (_destroyed)
            return;

        _destroyed = true;

        _planeCollider.enabled = false;

        _game.DoScreenshake(_planeBody.transform.position, 4, 0.5f);
        Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);

        Invoke("realDoDestroy", 3);

        if (_nextEnvDamage < Time.time)
            _game.AddScore(score, _planeBody.transform.position);

        _game.DecrementEnemyCount();

    }
    private void RealDoDestroy()
    {
        if ( _game.IsOnScreen(_planeBody.transform.position, 1.2f ) )
            _game.DoScreenshake(_planeBody.transform.position, 8, 1);

        Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {

    }

    protected override void OnTriggerStay2D(Collider2D other)
    {

    }
}
