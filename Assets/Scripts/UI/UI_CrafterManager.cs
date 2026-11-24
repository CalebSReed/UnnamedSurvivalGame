using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CrafterManager : MonoBehaviour
{
    [SerializeField] private List<RectTransform> craftingTabs = new List<RectTransform>();
    public void SelectCraftingTab(RectTransform tab)
    {
        foreach(var craftingTab in craftingTabs)
        {
            craftingTab.localScale = new Vector3(0, 0, 0);
        }
        tab.localScale = Vector3.one;
    }
}
