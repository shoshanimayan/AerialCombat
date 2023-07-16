using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneAceLogic : EnemyPlaneLogic
{
    //  PRIVATE VARIABLES         //

    protected float _barrageDur = 0.5f;
    protected int _barrageNumShots = 5;
    protected float _barrageDelay;
    protected float _nextBarrage = 0;
    protected float _fireBarrage = 0;
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
        _maxSpeed = Random.Range(70f, 80f);
        _rotSpeed = Random.Range(300, 310);
        _closeupRange = 3;
        _accelerationRange = 10;
        _barrageDelay = 1;
        _fireDelay = _barrageDur / _barrageNumShots;
        _smokeThreshold = 0.5f;
        _desiredGoalAng = 20;
        _attackRange = 50;
    }

    protected override void Update()
    {
        if (_game == null) { return; }

        if ( !_destroyed )
            base.Update();
        else
        {
            _thrustInput = 0;
            _rotInput = -1;
            _rotSpeed = 700;
            HandleEffects();

            if (_planeBody.transform.position.y < _game.GetWaterLevel())
                RealDoDestroy();
        }   
    }

    protected override void HandleEffects()
    {
        DoSmokeEffect();
        DoJetEffect();
        DoWaterEffect();
    }

    protected override void FireProjectile()
    {
        if (_nextBarrage <= Time.time)
        {
            _fireBarrage =  _fireDelay * _barrageNumShots + Time.time;
            _nextBarrage = _barrageDelay + Time.time + Random.Range(0.4f, 0.9f);
        }

        if (_nextFire <= Time.time && _fireBarrage > Time.time)
        {
            var spawn_pos = _planeBody.transform.position + _planeBody.transform.up * 5.5f;

            var dir = _planeBody.transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range(-5, 5)) * dir;

            _game.CreateProjectile(_projectilePrefab, spawn_pos, dir, _maxSpeed * 2, 25, GetTeam(), _planeBody.velocity);

            PlaySound(SearchAudioName("bullet"));

            _nextFire = _fireDelay + Time.time;
        }

    }

    protected override float GetRotPower()
    {
        if (IsAccelerating())
            return 0.7f;
        else
            return 1;
    }

    protected override void DoDestroy()
    {
        if (_destroyed)
            return;

        _destroyed = true;

        _planeCollider.enabled = false;

        _game.DoScreenshake(_planeBody.transform.position, 5, 0.5f);
        GameObject explosionObj = Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);

        Invoke("realDoDestroy", 3);

        _game.AddScore(score, _planeBody.transform.position);
        _game.DecrementEnemyCount();

    }
    private void RealDoDestroy()
    {
        if (_game.IsOnScreen(_planeBody.transform.position, 1.4f))
            _game.DoScreenshake(_planeBody.transform.position, 8, 1);

        Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }
}
