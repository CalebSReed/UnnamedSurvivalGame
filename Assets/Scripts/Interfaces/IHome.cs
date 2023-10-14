using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHome
{
    int currentGuests { get; set; }
    int progressToNextGuest { get; set; }
    int goalToCreateNewGuest { get; set; }
    void SaveData();
    void LoadData();
}
