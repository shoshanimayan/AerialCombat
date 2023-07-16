using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLogic : MonoBehaviour
{

    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _waterObj;

    //  PRIVATE VARIABLES         //
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PRIVATE METHODS           //
    void LateUpdate()
    {
        if (!_game) { return; }

        var update_pos = new Vector3(_game.GetCameraX(), _game.GetWaterLevel() - _waterObj.transform.localScale.y / 2, -3);
        _waterObj.transform.position = update_pos;
    }
}
