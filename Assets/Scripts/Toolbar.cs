using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;
    public PlayerController player;
    public int slotIndex = 0;

    private void Start()
    {
        byte index = 1;
        foreach(UIItemSlot s in slots)
        {
            ItemStack stack = new ItemStack(index, Random.Range(2,65));
            ItemSlot slot = new ItemSlot(s, stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                slotIndex++;
            }
            else
            {
                slotIndex--;
            }
            if (slotIndex > (byte)(slots.Length - 1))
            {
                slotIndex = 0;
            }
            else if (slotIndex < 0)
            {
                slotIndex = (byte)(slots.Length - 1);
            }
            highlight.position = slots[slotIndex].slotIcon.transform.position;
            //player.selectedBlockIndex = slots[slotIndex].itemSlot.stack.id;
            //slotIndex.text = world.blockTypes[slotIndex].blockName + " block selected";
        }
    }
}
//    World world;
//    public PlayerController player;
//    public RectTransform highlight;
//    public ItemSlot[] items;
//    int slotIndex=0;

//    private void Start()
//    {
//        world = GameObject.Find("World").GetComponent<World>();
//        foreach (ItemSlot slot in items)
//        {
//            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
//            slot.icon.enabled = true;
//        }
//        player.selectedBlockIndex = items[0].itemID;
//    }

//    
//}
