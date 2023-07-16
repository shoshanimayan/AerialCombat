using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionFXLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    protected GameObject _waterExplosionFx;

    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PRIVATE METHODS           //

    private void Start()
    {
        
        if ( transform.position.y < (_game.GetWaterLevel() + 10) )
        {
            var exp_pos = transform.position;
            exp_pos.z = 0;
            exp_pos.y = _game.GetWaterLevel();

            Instantiate(_waterExplosionFx, exp_pos, Quaternion.identity);
        }   
    }
}
