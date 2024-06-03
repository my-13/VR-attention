using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Item", order = 1)]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int itemID;
    public string itemCode;
    public GameObject itemObj;
    public Image image;
}
