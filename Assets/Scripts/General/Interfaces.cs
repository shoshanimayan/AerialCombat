using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void ProceedDamage(float dmg, bool noSnd = false);
}
