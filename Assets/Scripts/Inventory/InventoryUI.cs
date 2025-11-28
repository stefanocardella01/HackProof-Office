using UnityEngine;
using UnityEngine.UI;
public class InventoryUI : MonoBehaviour
{

    [System.Serializable]
    public class SlotUI 
    {

        public Image background; //Sfondo dello slot
        public Image iconImage;  //Immagine/icona dell'oggetto

    }

    [Header("Slot UI (in ordine da 0 a 4)")]
    public SlotUI[] slots;

    [Header("Colori")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 0.7f, 0.7f); //colore di sfondo per oggetto selezionato

    private InventoryManager inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>(); //FIndObjectOfType andrebbe usato ma sembra obsoleto
    }

    private void OnEnable()
    {
        if(inventory != null)
        {

            inventory.OnInventoryChanged += Refresh;
            inventory.OnSelectionChanged += RefreshSelection;

        }

        Refresh();
        RefreshSelection(inventory.GetSelectedIndex());
    }

    private void OnDisable()
    {
        if (inventory != null)
        {

            inventory.OnInventoryChanged -= Refresh;
            inventory.OnSelectionChanged -= RefreshSelection;

        }

    }

    private void Start()
    {
        Refresh();

        RefreshSelection(inventory != null ? inventory.GetSelectedIndex() : -1);
    }

    private void Refresh()
    {

        if(inventory == null)
        {
            return;
        }

        for(int i = 0; i < slots.Length; i++)
        {

            var item = inventory.GetItem(i);
            
            if(item != null && item.icon != null)
            {

                slots[i].iconImage.enabled = true;
                slots[i].iconImage.sprite = item.icon;

            } 
            else
            {
                slots[i].iconImage.enabled = false;
                slots[i].iconImage.sprite = null;
            }
        }
    }

    private void RefreshSelection(int selectedIndex)
    {

        Debug.Log($"[InventoryUI] RefreshSelection({selectedIndex})");  // <--- AGGIUNTO


        for (int i = 0; i < slots.Length; i++)
        {

            if (slots[i].background != null)
            {
                var colore = (i == selectedIndex) ? selectedColor : normalColor;
                slots[i].background.color = colore;
                Debug.Log($"[InventoryUI] Slot {i} -> {slots[i].background.name} color={colore}");


            }

        }

    }
}
