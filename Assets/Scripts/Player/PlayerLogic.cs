using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour, IDamagable
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    protected BulletType _projectileType;

    [SerializeField]
    protected GameObject _explosionFx;

    [SerializeField]
    private ParticleSystem _muzzleFlashFx;
    [SerializeField]
    protected ParticleSystem _damageSmokeFx;
    [SerializeField]
    protected ParticleSystem _jetmainFx;
    [SerializeField]
    protected ParticleSystem _repairFx;
    [SerializeField]
    protected GameObject _impactFx;
    [SerializeField]
    protected ParticleSystem _waterFx;

    [SerializeField]
    private List<AudioClip> _soundList;

    [SerializeField]
    private AudioSource _audioSourceParent;
    [SerializeField]
    protected float _maxHealth = 100;

    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }


    protected Rigidbody2D _planeBody;
    protected PolygonCollider2D _planeCollider;

    protected float _curThrust = 0;
    private float _maxThrust = 1;

    protected float _health;
    

    protected float _rotInput = 0;
    protected float _thrustInput = 0;

    public float _rotSpeed = 320;

    public float _maxSpeed = 40;
    public float _acceleration = 10;

    private bool _isRepairing = false;

    protected Vector2 _vectorDown = new Vector2(0, -1);
    protected Vector2 _vectorUp = new Vector2(0, 1);
    protected Vector3 _vectorUp3 = new Vector3(0, 0, 1);

    protected int _team = 1;

    protected float _fireDelay = 0.11f;

    protected float _nextFire = 0;
    protected float _nextRegen = 0;
    protected float _nextEnvDamage = 0;

    protected float _smokeThreshold = 1;

    protected float _firePitch = 0;
    protected GameObject _projectilePrefab;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _planeBody = GetComponent<Rigidbody2D>();
        _planeCollider = GetComponent<PolygonCollider2D>();
        _projectilePrefab = _game.GetProjectileType(_projectileType);

        SetMaxHealth(_maxHealth);
        SetHealth(GetMaxHealth());
    }


    private void Update()
    {
        if (!_game) { return; }

        _rotInput = Input.GetAxisRaw("Horizontal");
        _thrustInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
        {
            FireProjectile();
        }
        HandleEnvDamage( true );
        HandleHealthRegen();
        HandleEffects();
    }

    private void FixedUpdate()
    {
        if (!_game) { return; }

        HandleMotion();
        HandleLevelBounds();
    }

    protected virtual void HandleEffects()
    {
        DoSmokeEffect();
        DoJetEffect();
        DoRepairEffect();
        DoWaterEffect();
    }

    protected void HandleMotion()
    {
        if (_rotInput != 0)
        {
            var new_rotation = _planeBody.rotation - _rotSpeed * GetRotPower() * Time.fixedDeltaTime * _rotInput;
            new_rotation = NormalizeAngle(new_rotation);
            _planeBody.MoveRotation(new_rotation);
        }

        _planeBody.transform.GetChild(0).transform.localEulerAngles = new Vector3(0, _planeBody.rotation * -1, 0);

        if (IsAccelerating())
        {
            _curThrust = Mathf.Min(_curThrust + 2.3f * Time.fixedDeltaTime, _maxThrust);
            _planeBody.gravityScale = 0;
            _planeBody.AddForce(GetAimDir() * _acceleration * _curThrust, ForceMode2D.Impulse);
        }
        else
        {
            _curThrust = 0;
            _planeBody.gravityScale = 3f;
        }


        if (_planeBody.transform.position.y > _game.GetSkyLevel())
            _planeBody.AddForce(_vectorDown * (1 - (_game.LevelMaxs().y - _planeBody.transform.position.y) / _game.GetSkyLevelHeight()) * 120, ForceMode2D.Force);

        if (_planeBody.transform.position.y < _game.GetWaterLevel())
        {
            float force_delta = 1 - (_game.LevelMins().y - _planeBody.transform.position.y) / _game.GetWaterLevelHeight();
            force_delta = Mathf.Pow(force_delta, 3f);
            _planeBody.AddForce(_vectorUp * force_delta * 30, ForceMode2D.Force);
        }

        var clamped_vel = _planeBody.velocity.magnitude;

        clamped_vel = Mathf.Clamp(clamped_vel, 0, _maxSpeed);

        _planeBody.velocity = GetVelDir() * clamped_vel;
    }

    private void HandleLevelBounds()
    {
        var update_pos = _planeBody.transform.position;

        if (_planeBody.transform.position.x <= _game.LevelMins().x)
            update_pos.x = _game.LevelMaxs().x;
        if (_planeBody.transform.position.x >= _game.LevelMaxs().x)
            update_pos.x = _game.LevelMins().x;

        if (_planeBody.transform.position.y < _game.LevelMins().y)
            update_pos.y = _game.LevelMins().y;
        if (_planeBody.transform.position.y > _game.LevelMaxs().y)
            update_pos.y = _game.LevelMaxs().y;

        _planeBody.transform.position = update_pos;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject);
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if ( other_class != null && _nextEnvDamage <= Time.time )
        {
            _game.DoScreenshake(other.attachedRigidbody.transform.position, 1.5f, 0.6f);
            other_class.ProceedDamage( 25 );
            ProceedDamage(15);
            _nextEnvDamage = Time.time + 0.1f;
        }
       
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if (other_class != null && _nextEnvDamage <= Time.time)
        {
            _game.DoScreenshake(other.attachedRigidbody.transform.position, 1.5f, 0.6f);
            other_class.ProceedDamage(13);
            ProceedDamage(7);
            _nextEnvDamage = Time.time + 0.05f;

            var exp_pos = _planeBody.transform.position;
            exp_pos.z = -2;

            Instantiate(_impactFx, exp_pos, Quaternion.identity);

        }

    }


    private void HandleHealthRegen()
    {
        if (GetHealth() < GetMaxHealth())
        {
            if (_nextRegen < Time.time)
            {
                SetHealth(Mathf.Clamp(GetHealth() + 1, 0, GetMaxHealth()));
                _nextRegen = 0.016f + Time.time;
                PlaySound("player_repair", 0.1f);
                _isRepairing = true;
            }
        }
        else
        {
            if (_isRepairing)
            {
                PlaySound("player_finish_repair", 0.2f);
                _isRepairing = false;
            }
        }    
            
    }

    protected void HandleEnvDamage( bool noSnd = false )
    {
        if (_nextEnvDamage < Time.time && _planeBody.transform.position.y < _game.GetWaterLevel())
        {
            var exp_pos = _planeBody.transform.position;
            exp_pos.z = -2;

            Instantiate(_impactFx, exp_pos, Quaternion.identity);
            _nextEnvDamage = 0.03f + Time.time;
            ProceedDamage(1, noSnd);
        }
    }

    protected virtual float GetRotPower()
    {
        if (IsAccelerating())
            return 0.3f;
        else
            return 1;
    }

    //  PUBLIC API               //


    public bool IsAccelerating()
    {
        return _thrustInput > 0;
    }

    public Vector2 GetVelDir()
    {
        return _planeBody.velocity.normalized;
    }

    public Vector2 GetAimDir()
    {
        return _planeBody.transform.up;
    }

    protected float NormalizeAngle(float ang)
    {
        ang = (ang + 180) % 360;
        if (ang < 0)
            ang += 360;
        return ang - 180;
    }


    protected virtual void FireProjectile()
    {
        if (_game.IsGamePaused())
            return;

        if (_nextFire < Time.time)
        {
            var spawn_pos = _planeBody.transform.position + _planeBody.transform.up * 2.5f;

            var dir = _planeBody.transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range(-4, 4)) * dir;

            _game.CreateProjectile(_projectilePrefab, spawn_pos, dir, _maxSpeed * 2, 25, GetTeam(), _planeBody.velocity);

            DoMuzzleflash();

            PlaySound(SearchAudioName("bullet"), 0.5f);

            _nextFire = _fireDelay + Time.time;
            _nextRegen = 0.2f + Time.time;
            _isRepairing = false;
        }
    }

    protected virtual void DoMuzzleflash()
    {
        _muzzleFlashFx.Emit(1);
    }

    protected virtual void DoSmokeEffect()
    {

        float hp_delta = Mathf.Clamp(GetHealth() / GetMaxHealth(), 0, 1);
        float interval = 0.08f - 0.07f * (1 - hp_delta);
        int new_min, new_max;

        var new_burst = _damageSmokeFx.emission.GetBurst(0);

        new_min = 0 + Mathf.RoundToInt(2 * (1 - hp_delta));
        new_max = 1 + Mathf.RoundToInt(3 * (1 - hp_delta));


        new_burst.repeatInterval = interval;
        new_burst.minCount = (short)new_min;
        new_burst.maxCount = (short)new_max;

        _damageSmokeFx.emission.SetBurst(0, new_burst);

        if (GetHealth() < GetMaxHealth() * _smokeThreshold)
        {
            if (!_damageSmokeFx.isEmitting)
                _damageSmokeFx.Play();
        }
        else
        {
            if (_damageSmokeFx.isEmitting)
                _damageSmokeFx.Stop();
        }
    }

    protected virtual void DoJetEffect()
    {
        if (IsAccelerating())
        {
            if (!_jetmainFx.isEmitting)
            {
                _jetmainFx.Play(true);
                PlaySound("jet_on", 0.1f);
            }

        }
        else
        {
            if (_jetmainFx.isEmitting)
                _jetmainFx.Stop();
        }
    }

    protected virtual void DoRepairEffect()
    {
        if (_isRepairing)
        {
            if (!_repairFx.isEmitting)
                _repairFx.Play();
        }
        else
        {
            if (_repairFx.isEmitting)
                _repairFx.Stop();
        }
    }

    protected virtual void DoWaterEffect()
    {
        float pow_dist = 15;
        float power = Mathf.Clamp01(1 - (_planeBody.transform.position.y - _game.GetWaterLevel()) / pow_dist);

        if (_planeBody.transform.position.y < (_game.GetWaterLevel() + pow_dist))
        {
            if (!_waterFx.isEmitting)
                _waterFx.Play(true);

            var waterLogic = _waterFx.GetComponent<WaterSplashEngineLogic>();
            waterLogic.SetPower(1 + power * (5 + (IsAccelerating() ? 5 : 0)));
            waterLogic.SetDirection(_planeBody.transform.up);
        }
        else
        {
            if (_waterFx.isEmitting)
                _waterFx.Stop();
        }

    }

    protected string SearchAudioName(string namePart)
    {
        foreach (AudioClip clip in _soundList)
        {
            if (clip.name.Contains(namePart))
            {
                return clip.name;
            }
        }
        return namePart;
    }

    protected virtual void DoDestroy()
    {
        _game.DoScreenshake(_planeBody.transform.position, 9, 2);
        Instantiate(_explosionFx, _planeBody.transform.position, Quaternion.identity);
        _game.SetPlayerDead(true);
        gameObject.SetActive(false);
    }

    private void CheckSound(string clipname, out AudioClip foundClip)
    {
        foundClip = null;
        foreach (var clip in _soundList)
        {
            if (clip.name == clipname) { foundClip = clip; }
        }
    }

    public float GetVelDelta()
    {
        return _planeBody.velocity.sqrMagnitude / (_maxSpeed * _maxSpeed);
    }

    public float GetThrustDelta()
    {
        return _curThrust;
    }

    public void SetTeam( int t = 1 )
    {
        _team = t;
    }

    public int GetTeam()
    {
        return _team;
    }

    public void SetHealth( float am )
    {
        _health = am;
    }

    public float GetHealth()
    {
        return _health;
    }

    public void SetMaxHealth(float am)
    {
        _maxHealth = am;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    
    public void ProceedDamage( float dmg, bool noSnd = false )
    {
        _nextRegen = 0.5f + Time.time;
        _isRepairing = false;
        SetHealth(Mathf.Clamp(GetHealth() - dmg, 0, GetMaxHealth()));

        if (GetHealth() <= 0)
            DoDestroy();
        else
        {
            if (!noSnd)
                PlaySound(SearchAudioName("hit"), 0.5f);
        }
           
    }

    

    public void PlaySound(string clipName, float volume = 1)
    {
        if (_game.IsGamePaused())
            return;

        if (_soundList.Count < 1 )
        {
            return;
        }
        ;
        CheckSound(clipName,out AudioClip clip);
        
        if (_audioSourceParent == null || clip == null)
        {
            return;
        }

        if (_firePitch == 0)
            _firePitch = _audioSourceParent.pitch * 1;

        if ( clip.name.Contains("bullet") )
        {
            _audioSourceParent.pitch = _firePitch + Random.Range(-0.15f, 0.15f);
            _audioSourceParent.volume = volume;
            _audioSourceParent.PlayOneShot(clip);
        }
        else
        {
            _audioSourceParent.pitch = _firePitch * 1;
            _audioSourceParent.volume = 1;
            _audioSourceParent.PlayOneShot(clip, volume);
        }
    }

    public void PlaySoundAtPoint(int index, Vector3 pos, float volume = 1)
    {
        if (_soundList[index] == null)
        {
            return;
        }
        AudioSource.PlayClipAtPoint(_soundList[index], pos, volume);
    }



}
