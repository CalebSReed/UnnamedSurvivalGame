using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureEmitter : MonoBehaviour
{
    public void EmitTemperature(int _temp)
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, 10);

        foreach (Collider2D _target in _targetList)
        {
            if(_target.isTrigger && _target.GetComponent<TemperatureReceiver>())
            {
                _target.GetComponent<TemperatureReceiver>().ReceiveTemperature(_temp);//emit less temp the farther away target is. 
            }
        }
    }
}
