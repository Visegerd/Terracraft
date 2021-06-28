using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;
    [SerializeField] private GraphicRaycaster raycaster = null;
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem = null;

    World world;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if(!world.isInUI)
        {
            return;
        }
        cursorSlot.transform.position = Input.mousePosition;
        if(Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null)
            return;
        if (clickedSlot.itemSlot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertAll(clickedSlot.itemSlot.stack);
            return;
        }
        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;
        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertAll(clickedSlot.itemSlot.TakeAll());
            cursorSlot.UpdateSlot();
            return;
        }
        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertAll(cursorItemSlot.TakeAll());
            clickedSlot.UpdateSlot();
            return;
        }
        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();
                clickedSlot.itemSlot.InsertAll(oldCursorSlot);
                cursorSlot.itemSlot.InsertAll(oldSlot);
                cursorSlot.UpdateSlot();
                clickedSlot.UpdateSlot();
            }
            else
            {
                clickedSlot.itemSlot.stack.amount += cursorItemSlot.stack.amount;
                cursorItemSlot.TakeAll();
                cursorSlot.UpdateSlot();
                clickedSlot.UpdateSlot();
            }
            return;
        }
    }   

    private UIItemSlot CheckForSlot()
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);
        foreach (RaycastResult r in results)
        {
            if(r.gameObject.tag=="ItemSlot")
            {
                return r.gameObject.GetComponent<UIItemSlot>();
            }
        }
        return null;
    }
}
