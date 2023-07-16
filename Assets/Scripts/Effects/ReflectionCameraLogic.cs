using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionCameraLogic : MonoBehaviour
{
    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }
    private Camera _cameraObj;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _cameraObj = GetComponent<Camera>();
    }

    
    private void LateUpdate()
    {
        if (_game == null) { return; }

        if (!_game) { return; }

        var update_pos = new Vector3(_game.GetCameraX(), _game.GetWaterLevel() + _cameraObj.orthographicSize, _cameraObj.transform.position.z);
        _cameraObj.transform.position = update_pos;
    }
}
