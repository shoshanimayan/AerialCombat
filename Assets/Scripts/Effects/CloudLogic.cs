using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _cloud;
    //  PRIVATE VARIABLES         //

    private float _speed;
    private bool _backLayer = false;
    private float _saveY = 9999;
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }
    //  PRIVATE METHODS           //

    private void Start()
    {
        _speed = Random.Range(-3f, 3f);
    }

    
    private void LateUpdate()
    {
        if (!_game) { return; }


        var new_pos = new Vector3( _game.calculateLoopingX( _cloud.transform.position.x + _speed * Time.deltaTime, true ), _cloud.transform.position.y, _cloud.transform.position.z);

        if ( _backLayer )
        {
            if (_saveY == 9999)
                _saveY = new_pos.y * 1;

            float cam_y = Mathf.Max(_game.GetCameraY(), _game.GetWaterLevel() + 50);
            float cam_delta = Mathf.Clamp( cam_y / _game.LevelMaxs().y, -1, 1);
            new_pos.y = _saveY + cam_delta * 30;
        }

        _cloud.transform.position = new_pos;  
    }

    //  PUBLIC API               //


    public void SetBackLayer( bool back )
    {
        _backLayer = back;
    }

}
