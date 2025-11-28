using UnityEngine;

public class InventoryInput : MonoBehaviour
{

    private InventoryManager inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
    }

    private void Update()
    {
        
        if(inventory == null)
        {
            return;
        }

        //Handle selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SelectSlot(4);

    }

}
