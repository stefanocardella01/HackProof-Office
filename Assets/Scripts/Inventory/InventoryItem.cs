using UnityEngine;

[System.Serializable]
public class InventoryItem 
{

    public string id; //es: "item_usb"
    public string displayName; //es: "Chiavetta USB"
    public Sprite icon; //icona da mostrare nello slot dell'inventario
    public GameObject inspectPrefab; //Prefab da mostrare nel pannello inspect

}