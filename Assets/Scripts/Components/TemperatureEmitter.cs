using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureEmitter : MonoBehaviour
{
    public int temp;
    public float tempRadius;

    public IEnumerator EmitTemperature()
    {
        Collider[] _targetList = Physics.OverlapSphere(transform.position, tempRadius);

        foreach (Collider _target in _targetList)
        {
            if(!_target.isTrigger && _target.GetComponent<TemperatureReceiver>())
            {
                StartCoroutine(_target.GetComponent<TemperatureReceiver>().ReceiveTemperature(temp));//emit less temp the farther away target is. 
                //Debug.Log("Found something!");
            }
        }
        //Debug.Log("Looking for temperature targets");
        yield return new WaitForSeconds(1);
        StartCoroutine(EmitTemperature());
    }

    public void OnDrawGizmos()
    {
        
        Gizmos.DrawWireSphere(transform.TransformPoint(GetComponent<RealWorldObject>().spriteRenderer.sprite.bounds.center), tempRadius);
    }
}
