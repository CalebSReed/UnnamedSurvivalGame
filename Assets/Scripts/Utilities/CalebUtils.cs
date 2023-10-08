using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CalebUtils
{
    public static Vector3 RandomPositionInRadius(Vector3 oldPos, int innerRadius, int outerRadius)
    {
        Vector3 _newPos = oldPos;
        float _tX = (Random.Range(innerRadius, outerRadius + 1));//new value within inner and outer radiuses
        float _tY = (Random.Range(innerRadius, outerRadius + 1));
        int _rand_num = Random.Range(0, 2);//random if we will be going left or right
        if (_rand_num == 1)
        {
            _tX *= -1;
        }
        _rand_num = Random.Range(0, 2);//random if we will be going up or down
        if (_rand_num == 1)
        {
            _tY *= -1;
        }
        _newPos.x += _tX;//target starts as current pos, then we add these new values
        _newPos.y += _tY;
        return _newPos;
    }
}
