using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverable : MonoBehaviour
{
    public string Name {get; set;}
    public string Prefix { get; set; }//such as Eat, Equip, Light, Store... etc!
    public bool SpecialCase { get; set; }
}
