using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public Sprite buttonAIcon;
    public Sprite buttonBIcon;
    void Start()
    {
        var inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory == null) return;
        InventoryItem usb = new InventoryItem
        {
            id = "item_usb",
            displayName = "Chiavetta USB",
            icon = buttonAIcon
        };

        InventoryItem note = new InventoryItem
        {
            id = "item_note",
            displayName = "Post-it",
            icon = buttonBIcon
        };

        //Debug.Log("USB added: " + inventory.AddItem(usb));
        //Debug.Log("NOTE added: " + inventory.AddItem(note));

    }

}
