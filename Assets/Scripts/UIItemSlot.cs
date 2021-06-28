using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get
        {
            if (itemSlot == null)
            {
                return false;
            }
            else return itemSlot.HasItem;
        }
    }

    public void Link (ItemSlot iSlot)
    {
        itemSlot = iSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void Unlink()
    {
        itemSlot.UnlinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
            slotAmount.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else Clear();
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked)
        {
            itemSlot.UnlinkUISlot();
        }
    }
}

//[System.Serializable]
public class ItemSlot
{
    public ItemStack stack=null;
    private UIItemSlot uiItemSlot = null;
    public bool isCreative = false;
    public ItemSlot(UIItemSlot itemSlot)
    {
        stack = null;
        uiItemSlot = itemSlot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot itemSlot,ItemStack itemStack)
    {
        stack = itemStack;
        uiItemSlot = itemSlot;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot (UIItemSlot itemSlot)
    {
        uiItemSlot = itemSlot;
    }
    public void UnlinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if (uiItemSlot != null)
            uiItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get
        {
            if (stack != null)
                return true;
            else
                return false;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertAll(ItemStack itemStack)
    {
        stack = itemStack;
        uiItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if(amt>stack.amount)
        {
            int _amt = stack.amount;
            EmptySlot();
            return _amt;
        }
        else if(amt<stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }
    //public byte itemID;
    //public Image icon;
}
