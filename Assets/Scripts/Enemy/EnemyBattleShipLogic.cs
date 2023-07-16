using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleShipLogic : EnemyShipLogic
{
    //  PRIVATE METHODS           //

    private void Start()
    {
        _shipBody = GetComponent<Rigidbody2D>();
        _shipCollider = GetComponent<PolygonCollider2D>();
        _projectilePrefab = _game.GetProjectileType(_projectileType);
        SetTeam(2);
        SetHealth(1000);
        SetMaxHealth(1000);
        _barrageDelay = 2;
        _barrageNumShots = 8;
        _fireDelay = _barrageDur / _barrageNumShots;
        _smokeThreshold = 0.4f;
    }


    protected override void Update()
    {
        if (_game == null) { return; }

        var player = _game.PlayerEntity;

        if (player.activeSelf && !_destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;
            var playerPos_ahead = playerPos + new Vector3( player_body.velocity.normalized.x, 0, 0 ).normalized * 35;

            float dist_x_abs = Mathf.Abs(playerPos.x - _shipBody.transform.position.x);

            if (dist_x_abs > 100)
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

            if (_nextCannonTurn <= Time.time)
            {
                foreach ( GameObject cannon in _cannons )
                {
                    var cannon_pos = cannon.transform.position;
                    Vector3 dir = (playerPos_ahead - cannon_pos).normalized;
                    dir.x = Mathf.Clamp(dir.x, -0.8f, 0.8f);
                    dir.y = Mathf.Clamp(dir.y, 0f, 1);

                    cannon.transform.up = Vector3.RotateTowards(cannon.transform.up, dir, 3 * Time.deltaTime, 0.0f);
                    cannon.transform.eulerAngles = new Vector3(0, 0, cannon.transform.eulerAngles.z);
                }
            }
        }

        DoSmokeEffect();
    }

    protected override void FireProjectile()
    {
        if (_nextBarrage <= Time.time)
        {
            _nextCannonTurn = _fireDelay * _barrageNumShots + Time.time;
            _fireBarrage = _nextCannonTurn * 1;
            _nextBarrage = _barrageDelay + Time.time + UnityEngine.Random.Range(0.4f, 0.9f);
        }

        if (_nextFire <= Time.time && _fireBarrage > Time.time)
        {
            foreach (GameObject cannon in _cannons)
            {
                var spawn_pos = cannon.transform.position + cannon.transform.up * 5;

                var dir = cannon.transform.up;
                dir = Quaternion.Euler(0, 0, Random.Range(-1, 1)) * dir;

                _game.CreateProjectile(_projectilePrefab, spawn_pos, dir, 80, 20, GetTeam(), Vector3.zero);
                PlaySound(SearchAudioName("bullet"),0.7f);
            }

            _nextFire = _fireDelay + Time.time;
        }

    }
}
