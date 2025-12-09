using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
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

    public GameObject inspectObjectSelected;  //testo a sinistra dell'inventario con oggetto selezionato e Q per ispezionare

    private TextMeshProUGUI objectSelectedName;

    private InventoryManager inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>(); //FIndObjectOfType andrebbe usato ma sembra obsoleto

        //Recupero il textmeshpro della UI a sinistra dell'inventario
        objectSelectedName = inspectObjectSelected.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        //La UI a sinistra dell'inventario deve essere inizialmente disattivata
        inspectObjectSelected.SetActive(false);

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

                //Attivo a prescindere UI a sinistra dell'inventario
                inspectObjectSelected.SetActive(true);
                objectSelectedName.text = item.displayName;

            } 
            else
            {
                slots[i].iconImage.enabled = false;
                slots[i].iconImage.sprite = null;

                //Disattivo UI a sinistra dell'inventario
                inspectObjectSelected.SetActive(false);
            }
        }
    }

    private void RefreshSelection(int selectedIndex)
    {

        Debug.Log($"[InventoryUI] RefreshSelection({selectedIndex})");  // <--- AGGIUNTO

        var item = inventory.GetItem(selectedIndex);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].background != null)
            {
                var colore = normalColor;

                if(i == selectedIndex)
                {
                    colore = selectedColor;
                }

                slots[i].background.color = colore;

                Debug.Log($"[InventoryUI] Slot {i} -> {slots[i].background.name} color={colore}");
            }
        }
        //Atttivo UI a sinistra dell'inventario se lo slot selezionato contiene un item
        if (item != null)
        {
            inspectObjectSelected.SetActive(true);
            objectSelectedName.text = item.displayName;

        }
        // Disattivo UI a sinistra dell'inventario se l'oggetto precedentemente selezionato è stato rimosso
        else
        {
            inspectObjectSelected.SetActive(false);
        }
    }
}
