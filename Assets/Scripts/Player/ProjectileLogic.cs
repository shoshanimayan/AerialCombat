using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _hitFx;
    [SerializeField]
    private GameObject _hitWaterFx;

    //  PRIVATE VARIABLES         //

    private Rigidbody2D _projBody;
    private CircleCollider2D _projCollider;
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    private float _speed;
    private int _team;
    private float _damage;
    private float _lifeTime;
    private float _maxLifeTime = 3.5f;
    private Vector3 _direction;
    private Vector3 _extraVelocity = new Vector3( 0, 0, 0 );
    private int _teleports = 0;
    private bool _hitWater = false;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _projBody = GetComponent<Rigidbody2D>();
        _projCollider = GetComponent<CircleCollider2D>();

        SetLifeTime(_maxLifeTime);

        if (GetTeam() != 1)
            SetLifeTime(_maxLifeTime * 2);

        _projBody.transform.right = GetDirection();

        _projBody.AddForce(GetDirection() * GetSpeed() + _extraVelocity, ForceMode2D.Impulse);
    }

    
    private void Update()
    {
        if (_game == null) { return; }

        if ( GetLifeTime() <= Time.time || _projBody.transform.position.y > _game.LevelMaxs().y || _projBody.transform.position.y < _game.LevelMins().y)
        {
            var exp_pos = _projBody.transform.position;
            exp_pos.z = -2;

            GameObject explosionObj = Instantiate(_hitFx, exp_pos, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        if ( !_hitWater && _projBody.transform.position.y < ( _game.GetWaterLevel() + 1 ) )
        {
            _hitWater = true;

            var exp_pos = _projBody.transform.position;
            exp_pos.y = _game.GetWaterLevel();

            GameObject explosionObj = Instantiate(_hitWaterFx, exp_pos, Quaternion.identity);
        }

    }

    private void FixedUpdate()
    {
        if (_game == null) { return; }

    }

    private void LateUpdate()
    {
        if (_game == null) { return; }

        var update_pos = _projBody.transform.position;
        float old_x = _projBody.transform.position.x;
        update_pos.x = _game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            OnTeleported(old_x, update_pos.x);
        }

        _projBody.transform.position = update_pos;
    }

    private void OnTeleported(float old_x, float new_x)
    {
        _teleports += 1;

        if (_teleports >= 2)
            Destroy(gameObject);

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if ( other_class == null )
        {
            Destroy(gameObject);
            return;
        }


        other_class.ProceedDamage(GetDamage());

        var exp_pos = _projBody.transform.position;
        exp_pos.z = -2;

        GameObject explosionObj = Instantiate(_hitFx, exp_pos, Quaternion.identity);

        Destroy(gameObject);
    }

    //  PUBLIC API               //

    public void SetupProjectile( Vector3 dir, float sp, float dmg, int tm, Vector3 extra_vel )
    {
        SetDirection(dir);
        SetSpeed(sp);
        SetDamage(dmg);
        SetTeam(tm);
        SetExtraVelocity(extra_vel);
    }

    public void SetLifeTime( float time )
    {
        _lifeTime = Time.time + time;
    }

    public float GetLifeTime()
    {
        return _lifeTime;
    }

    public void SetDamage( float dmg )
    {
        _damage = dmg;
    }

    public float GetDamage()
    {
        return _damage;
    }

    public void SetDirection( Vector3 dir )
    {
        _direction = dir;
    }

    public Vector3 GetDirection()
    {
        return _direction;
    }

    public void SetSpeed( float sp )
    {
        _speed = sp;
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public void SetTeam( int tm )
    {
        _team = tm;
    }
    public int GetTeam()
    {
        return _team;
    }

    public void SetExtraVelocity( Vector3 extra )
    {
        _extraVelocity = extra;
    }

}
