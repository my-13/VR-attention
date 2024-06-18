using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private GameObject mainItemSlot;
    private GameObject distractionItemSlotParent;
    private GameObject[] distractionItemSlots;
    private GameObject usedItemSlotParent;
    private GameObject[] usedItemSlots;
    
    public void Start()
    {
        // Get the main item slot
        mainItemSlot = GameObject.Find("ItemSlot");

        // Get the distraction item slots
        distractionItemSlotParent = GameObject.Find("DistractorSlots");
        usedItemSlotParent = GameObject.Find("UsedSlots");

        foreach (Transform child in distractionItemSlotParent.transform)
        {
            distractionItemSlots.Append(child.gameObject);
        }

        foreach (Transform child in usedItemSlotParent.transform)
        {
            usedItemSlots.Append(child.gameObject);
        }
    }

    public GameObject[] GetObjects()
    {
        GameObject[] items = new GameObject[0];
        // Return list of Objects from the UI manager in GameObject form

        // Get the main item
        if (mainItemSlot.transform.childCount > 0)
        {
            GameObject item = mainItemSlot.transform.GetChild(0).gameObject;
            GameObject itemObj = item.GetComponent<Item>().itemData.itemObj;
            items.Append(itemObj);
        }


        // Get the distraction items
        foreach (GameObject slot in distractionItemSlots)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                GameObject itemObj = item.GetComponent<Item>().itemData.itemObj;

                // TODO: Modify distractor items to fit distractor color

                items.Append(itemObj);
            }
        }

        // Get the used items
        foreach (GameObject slot in usedItemSlots)
        {
            if (slot.transform.childCount > 0)
            {
                
                GameObject item = slot.transform.GetChild(0).gameObject;
                GameObject itemObj = item.GetComponent<Item>().itemData.itemObj;

                //TODO: Modify used Items to fit the color

                items.Append(itemObj);
            }
        }

        return items;
    }

}
