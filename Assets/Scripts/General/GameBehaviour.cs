using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum BulletType { Player, EnemyNormal, EnemyAdvanced }

public class GameBehaviour : Singleton<GameBehaviour>
{


    //  INSPECTOR VARIABLES      //

    public GameObject PlayerEntity;
    public AudioMixer MainMixer;
    public AudioMixerGroup MainMixerGroup;

    [SerializeField]
    private GameObject _cloudPrefab;

    [SerializeField]
    private GameObject _darkCloudPrefab;

    [SerializeField]
    private Camera _cameraObj;

    [SerializeField]
    private GameObject _scoreTextPrefab;

    [SerializeField]
    private GameObject _playerProjectileType;

    [SerializeField]
    private GameObject _enemyProjectileType;
    [SerializeField]
    private GameObject _enemyAdvancedProjectileType;

    [SerializeField]
    private List<GameObject> _enemyTypes;

    [SerializeField]
    private GameObject _pauseMenu;
    [SerializeField]
    private GameObject _optionsMenu;
    [SerializeField]
    private GameObject _resultsPanel;

    //  PRIVATE VARIABLES         //

    private int _maxEnemies = 35;
    private int _curMaxEnemies = 0;
    private float _maxEnemiesSpawnTime = 90f;
    private float _curMaxEnemiesSpawnTime = 0;
    private int _enemyCount = 0;
    private float _nextEnemySpawn = 0;

    public float LevelWidth = 1000;
    public float LevelHeight = 500;


    private Vector2 _levelMins;
    private Vector2 _levelMaxs;

    private float _skyLevel = 0.15f;
    private float _waterLevel = 0.15f;

    private bool _playerDead = false;
    private bool _didRestart = false;

    private float _teleportFraction = 0.25f;

    private float _cloudZ = 5;
    private bool _gameStarted = false;

    private int _score = 0;
    private int _tempScore = 0;
    private int _curCombo = 0;
    private float _comboTime = 0;
    private float _comboDur = 7;

    private bool _gamePaused = false;
    private Dictionary<BulletType, GameObject> _projectileDict;

    //  PRIVATE METHODS           //


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        _projectileDict = new Dictionary<BulletType, GameObject> { { BulletType.Player, _playerProjectileType }, { BulletType.EnemyNormal, _enemyProjectileType }, { BulletType.EnemyAdvanced, _enemyAdvancedProjectileType } };
    }

    private void Start()
    {
       
        Cursor.visible = false;
        SetupLevelBounds();
        CreateClouds();

        PlayerEntity.transform.position = new Vector3(0, GetWaterLevel() + 4, 0);
        PlayerEntity.SetActive(false);

        float vol = PlayerPrefs.GetFloat("MasterVolume", 0);
        MainMixer.SetFloat("masterVolume", vol);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(!IsGamePaused());
        }

        if (!_gameStarted)
        {
            if (Input.GetAxisRaw("Vertical") > 0)
                StartGame();
            return;
        }
            

        float spawn_delta = Mathf.Clamp(1 - (_curMaxEnemiesSpawnTime - Time.time) / _maxEnemiesSpawnTime, 0, 1);
        int calculated_max = Mathf.CeilToInt((_maxEnemies - 3) * spawn_delta);

        _curMaxEnemies = 3 + calculated_max;

        SpawnEnemiesOverTime( 1.2f );

        HandleScore();

        if ( _playerDead && !_didRestart )
        {
            _didRestart = true;
            ForceFinishTempScore();
            Invoke("ShowResultsScreen", 0.5f);
          
        }

    }

   

    private void SetupLevelBounds()
    {
        _levelMins.x = -1 * LevelWidth / 2;
        _levelMins.y = -1 * LevelHeight / 2;

        _levelMaxs.x = LevelWidth / 2;
        _levelMaxs.y = LevelHeight / 2;
    }

    private void CreateClouds()
    {
        int amount = Random.Range(30, 40);

        for ( int i=1; i<=amount; i++)
        {
            float scale = Random.Range(3f, 13f);
            var pos = new Vector3(Random.Range(LevelMins().x * 0.9f, LevelMaxs().x * 0.9f), Random.Range(GetWaterLevel() * 0.8f, LevelMaxs().y), _cloudZ);
            GameObject cloud = Instantiate(_cloudPrefab, pos, Quaternion.identity);
            cloud.transform.GetChild(0).transform.localScale = new Vector3(scale * Random.Range(1.2f, 1.5f), scale, 1);
        }

        amount = Random.Range(60, 90);

        for (int i = 1; i <= amount; i++)
        {
            float scale = Random.Range(1f, 3f);
            var pos = new Vector3(Random.Range(LevelMins().x * 0.9f, LevelMaxs().x * 0.9f), Random.Range(GetWaterLevel() * 0.8f, LevelMaxs().y), _cloudZ + 1);
            GameObject cloud = Instantiate(_darkCloudPrefab, pos, Quaternion.identity);
            cloud.transform.GetChild(0).transform.localScale = new Vector3(scale * Random.Range(1.2f, 2f), scale, 1);
            var cloud_logic = cloud.GetComponent<CloudLogic>();
            cloud_logic.SetBackLayer(true);
        }

    }
    //  PUBLIC API               //

    public void StartGame()
    {
        _gameStarted = true;
        PlayerEntity.SetActive(true);
        PlayerEntity.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 630), ForceMode2D.Impulse);
        _curMaxEnemiesSpawnTime = Time.time + _maxEnemiesSpawnTime;
    }

    public Vector2 LevelMins()
    {
        return _levelMins;
    }

    public Vector2 LevelMaxs()
    {
        return _levelMaxs;
    }

    public float GetSkyLevel()
    {
        return _levelMaxs.y - LevelHeight * _skyLevel;
    }

    public float GetSkyLevelHeight()
    {
        return LevelHeight * _skyLevel;
    }

    public float GetWaterLevel()
    {
        return _levelMins.y + GetWaterLevelHeight();
    }

    public float GetWaterLevelHeight()
    {
        return 45;
    }

    public float GetCameraX()
    {
        return _cameraObj.transform.position.x;
    }

    public float GetCameraY()
    {
        return _cameraObj.transform.position.y;
    }

    private bool IsOnLeftSide( float x, bool isCamera = false )
    {
        if ( isCamera )
            return x < (_levelMins.x + LevelWidth * _teleportFraction);

        return IsOnRightSide( GetCameraX(), true ) && x < (_levelMins.x + LevelWidth * _teleportFraction); 
    }

    private bool IsOnRightSide(float x, bool isCamera = false)
    {
        if (isCamera)
            return x > (_levelMaxs.x - LevelWidth * _teleportFraction);

        return IsOnLeftSide( GetCameraX(), true ) && x > (_levelMaxs.x - LevelWidth * _teleportFraction);
    }

    public float calculateLoopingX( float x, bool resetOutOfBounds = false )
    {
        if (IsOnLeftSide(x))
            return x + LevelWidth;

        if (IsOnRightSide(x))
            return x - LevelWidth;

        if (resetOutOfBounds)
        {
            if (x < _levelMins.x && !IsOnLeftSide(GetCameraX(), true))
                return x + LevelWidth;

            if (x > _levelMaxs.x && !IsOnRightSide(GetCameraX(), true))
                return x - LevelWidth;
        }

        return x;
    }

    public void DoScreenshake(Vector3 pos, float am, float dur)
    {
        _cameraObj.GetComponent<CameraLogic>().DoScreenshake(pos, am, dur);
    }

    public void CreateProjectile( GameObject proj_prefab, Vector3 pos, Vector3 dir, float sp, float dmg, int tm, Vector3 extra_vel )
    {
        GameObject proj = Instantiate( proj_prefab, pos, Quaternion.identity);
        var proj_data = proj.GetComponent<ProjectileLogic>();
        proj_data.SetupProjectile(dir, sp, dmg, tm, extra_vel);
    }

    public void CreateEnemy( GameObject enemy_prefab, Vector3 pos )
    {
        GameObject enemy = Instantiate(enemy_prefab, pos, Quaternion.identity);
    }

    private void SpawnEnemiesOverTime( float delay )
    {
        if ( GetEnemyCount() < _curMaxEnemies && _nextEnemySpawn <= Time.time )
        {
            int type = 0;
            // small ship
            int rand = Random.Range(1, 8);
            if ( rand == 1 )
                type = 1;

            // jet
            rand = Random.Range(1, 6);
            if (rand == 1 && type == 0)
                type = 2;

            // big ship
            rand = Random.Range(1, 25);
            if (rand == 1 && type == 0)
                type = 3;

            // ace
            rand = Random.Range(1, 30);
            if (rand == 1 && type == 0)
                type = 4;

            int dir = Random.Range(1, 3) == 1 ? 1 : -1;

            float cam_size = _cameraObj.orthographicSize * _cameraObj.aspect;
            float spawn_x = GetCameraX() + (cam_size * 1.3f + Random.Range(10, LevelMaxs().x/2 - 10)) * dir;

            var spawn_pos = new Vector3(spawn_x, Random.Range(0, LevelMaxs().y + 40), 0);
            //var spawn_pos = new Vector3( Random.Range(levelMins().x + 10, levelMaxs().x - 10), Random.Range(0, levelMaxs().y + 40), 0 );

            if ( Mathf.Abs( spawn_pos.x - GetCameraX() ) < cam_size)
            {
                if (spawn_pos.x > GetCameraX())
                    spawn_pos.x += cam_size * 1.5f;
                else
                    spawn_pos.x -= cam_size * 1.5f;
            }

            CreateEnemy(GetEnemyType( type ), spawn_pos);
            IncrementEnemyCount();
            _nextEnemySpawn = Time.time + delay;
        }
    }

    public GameObject GetProjectileType( BulletType bullet )
    {
        return _projectileDict[bullet];
    }

    public GameObject GetEnemyType( int index )
    {
        return _enemyTypes[index];
    }

    private void IncrementEnemyCount()
    {
        _enemyCount += 1;
    }

    public void DecrementEnemyCount()
    {
        _enemyCount -= 1;
    }

    public int GetEnemyCount()
    {
        return _enemyCount;
    }

    public void SetPlayerDead( bool dead )
    {
        _playerDead = dead;
    }

    public bool IsPlayerDead()
    {
        return _playerDead;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameActive()
    {
        return _gameStarted;
    }

    public void AddScore( int sc, Vector3 pos )
    {
        if (_playerDead)
            return;
        
        if (_curCombo < 20)
            _curCombo += 1;

        pos.z = 4.5f;

        GameObject scoreText = Instantiate(_scoreTextPrefab, pos, Quaternion.identity);
        scoreText.GetComponentInChildren<TextMeshPro>().SetText((sc * _curCombo).ToString());

        _tempScore += sc * _curCombo;
        _comboTime = Time.time + _comboDur;
    }

    public void AddStaticScore( int sc )
    {
        _score += sc;
    }

    public int GetScore()
    {
        return _score;
    }

    public int GetTempScore()
    {
        return _tempScore;
    }

    private void ForceFinishTempScore()
    {
        _comboTime = 0;
    }

    private void HandleScore()
    {
        if ( _comboTime <= Time.time && _tempScore > 0 )
        {
            _score += _tempScore;
            _tempScore = 0;
            _curCombo = 0;
        }
    }

    private int GetHighScore()
    {
        return PlayerPrefs.GetInt("UPG_HighScore", 0);
    }

    private bool CheckHighScore()
    {
        if ( GetScore() > GetHighScore())
        {
            PlayerPrefs.SetInt("UPG_HighScore", GetScore());
            return true;
        }
        return false;
    }

    public float GetComboDelta()
    {
        return Mathf.Clamp01( 1 - ( _comboTime - Time.time ) / _comboDur );
    }  
    
    public int GetCombo()
    {
        return _curCombo;
    }

    public bool IsOnScreen( Vector3 pos, float scale = 1 )
    {
        float cam_x = GetCameraX();
        float cam_y = GetCameraY();

        Vector2 cam_mins = new Vector2(cam_x - _cameraObj.orthographicSize * _cameraObj.aspect * scale, cam_y - _cameraObj.orthographicSize * scale);
        Vector2 cam_maxs = new Vector2(cam_x + _cameraObj.orthographicSize * _cameraObj.aspect * scale, cam_y + _cameraObj.orthographicSize * scale);

        return pos.x > cam_mins.x && pos.x < cam_maxs.x && pos.y > cam_mins.y && pos.y < cam_maxs.y;
    }

    public Vector3 ToScreenView( Vector3 pos, float scale = 0.9f )
    {

        float cam_x = GetCameraX();
        float cam_y = GetCameraY();

        Vector2 cam_mins = new Vector2(cam_x - _cameraObj.orthographicSize * _cameraObj.aspect * scale, cam_y - _cameraObj.orthographicSize * scale);
        Vector2 cam_maxs = new Vector2(cam_x + _cameraObj.orthographicSize * _cameraObj.aspect * scale, cam_y + _cameraObj.orthographicSize * scale);

        pos.x = Mathf.Clamp(pos.x, cam_mins.x, cam_maxs.x);
        pos.y = Mathf.Clamp(pos.y, cam_mins.y, cam_maxs.y);

        return pos;
    }

    public void PauseGame( bool bl )
    {
        // not the best place for this but whatever
        if (_optionsMenu.activeSelf)
        {
            _optionsMenu.GetComponent<OptionsMenuLogic>().ReturnToMenu();
            return;
        }

        if (_playerDead)
            _resultsPanel.SetActive(!bl);

        _gamePaused = bl;
        Time.timeScale = bl ? 0f : 1f;
        _pauseMenu.SetActive(bl);
        
        Cursor.visible = bl;
    }

    public bool IsGamePaused()
    {
        return _gamePaused;
    } 
    
    private void ShowResultsScreen()
    {
        _resultsPanel.SetActive(true);
        bool newHighscore = CheckHighScore();
        var resPanelLogic = _resultsPanel.GetComponent<ResultsPanelLogic>();
        resPanelLogic.UpdateScoreResults(GetHighScore(), GetScore(), newHighscore);
    }

}

