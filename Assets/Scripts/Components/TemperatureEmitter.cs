using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureEmitter : MonoBehaviour
{
    public int temp;
    public float tempRadius;

    public IEnumerator EmitTemperature()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.TransformPoint(GetComponent<RealWorldObject>().spriteRenderer.sprite.bounds.center), tempRadius);

        foreach (Collider2D _target in _targetList)
        {
            if(_target.isTrigger && _target.GetComponent<TemperatureReceiver>())
            {
                StartCoroutine(_target.GetComponent<TemperatureReceiver>().ReceiveTemperature(temp));//emit less temp the farther away target is. 
            }
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(EmitTemperature());
    }

    public void OnDrawGizmos()
    {
        
        Gizmos.DrawWireSphere(transform.TransformPoint(GetComponent<RealWorldObject>().spriteRenderer.sprite.bounds.center), tempRadius);
    }
}
