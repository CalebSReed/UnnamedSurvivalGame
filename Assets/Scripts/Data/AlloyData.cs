using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlloyData
{
    public List<ItemSO> ingredientList;
    public ItemSO alloyReward;
    public float temperatureRequired;

    public static ItemSO CanMixElements(Item[] ingList, float currentTemp)
    {
        if (ingList[0] != null && ingList[1] != null && ingList[0].itemSO == ingList[1].itemSO)
        {
            return null;
        }

        int correctMaterialCount = 0;

        int nonNullItemsCount = 0;
        for (int j = 0; j < ingList.Length; j++)
        {
            if (ingList[j] != null)
            {
                nonNullItemsCount++;
            }
        }

        foreach(var possibleAlloy in possibleAlloys)
        {
            if (nonNullItemsCount == possibleAlloy.ingredientList.Count)
            {
                foreach(var item in ingList)
                {
                    for (int i = 0; i < possibleAlloy.ingredientList.Count; i++)
                    {
                        if (item.itemSO.itemType == possibleAlloy.ingredientList[i].itemType)
                        {
                            correctMaterialCount++;
                        }
                    }
                }
                if (correctMaterialCount == possibleAlloy.ingredientList.Count && currentTemp >= possibleAlloy.temperatureRequired)//correct material for each item in ingList
                {
                    return possibleAlloy.alloyReward;
                }
            }
            correctMaterialCount = 0;
        }
        return null;
    }

    public static List<AlloyData> possibleAlloys = new List<AlloyData>
    {
        new AlloyData { ingredientList = new List<ItemSO>  { ItemObjectArray.Instance.SearchItemList("RawCopper"), ItemObjectArray.Instance.SearchItemList("RawTin") }, temperatureRequired = 1085f, alloyReward = ItemObjectArray.Instance.SearchItemList("BronzeIngot")  },
        new AlloyData { ingredientList = new List<ItemSO>  { ItemObjectArray.Instance.SearchItemList("CopperIngot"), ItemObjectArray.Instance.SearchItemList("TinIngot") }, temperatureRequired = 1085f, alloyReward = ItemObjectArray.Instance.SearchItemList("BronzeIngot")  },
        new AlloyData { ingredientList = new List<ItemSO>  { ItemObjectArray.Instance.SearchItemList("RawCopper"), ItemObjectArray.Instance.SearchItemList("TinIngot") }, temperatureRequired = 1085f, alloyReward = ItemObjectArray.Instance.SearchItemList("BronzeIngot")  },
        new AlloyData { ingredientList = new List<ItemSO>  { ItemObjectArray.Instance.SearchItemList("CopperIngot"), ItemObjectArray.Instance.SearchItemList("RawTin") }, temperatureRequired = 1085f, alloyReward = ItemObjectArray.Instance.SearchItemList("BronzeIngot")  },
        new AlloyData { ingredientList = new List<ItemSO>  { ItemObjectArray.Instance.SearchItemList("WroughtIron"), ItemObjectArray.Instance.SearchItemList("Charcoal") }, temperatureRequired = 1538f, alloyReward = ItemObjectArray.Instance.SearchItemList("steelingot") }
    };
}
