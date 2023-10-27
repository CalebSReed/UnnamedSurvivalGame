using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Journal Entry", menuName = "Entry")]
public class JournalEntry : ScriptableObject
{
    public string entryName;//search by entry name for each entry in entry list
    [TextArea(10,10)]
    public string contents;
    public int size;//size in lines of dialogue
    public UI_JournalBehavior.PageType page;
    public bool needsItems;
    public List<string> requiredItems = new List<string>();
    public bool needsMobs;
    public List<string> requiredMobs = new List<string>();
    public bool needsObjects;
    public List<string> requiredObjs = new List<string>();
    public bool isSpecial;
}
