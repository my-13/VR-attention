using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("UI")]
    public Image image;
    

    public ItemData itemData;

    public void InitializeItem(ItemData newItemData)
    {
        this.itemData = newItemData;
        GetComponent<Image>().sprite = newItemData.image.sprite;
        image = newItemData.image;
    }

    [HideInInspector] public Transform parentAfterDrag;
    private Vector3 mOffset;

    public void Start()
    {
        InitializeItem(itemData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.pointerCurrentRaycast.worldPosition + mOffset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        mOffset = gameObject.transform.position - eventData.pointerCurrentRaycast.worldPosition;
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }

}
