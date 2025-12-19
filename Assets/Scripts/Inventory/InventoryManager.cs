using UnityEngine;
using System;
using System.IO;

public class InventoryManager : MonoBehaviour
{

    public const int MaxSlots = 5;

    private InventoryItem[] items = new InventoryItem[MaxSlots];
    private int selectedIndex = -1; //Niente selezionato all'inizio

    //Eventi che avvisano la UI che qualcosa è cambiato (per farla aggiornare)
    public event Action OnInventoryChanged;
    public event Action<int> OnSelectionChanged;

    //Funzione per aggiungere un oggetto nell'inventario nel primo spazio libero (ma solo se c'è spazio)
    public bool AddItem(InventoryItem newItem)
    {

        for(int i = 0; i < MaxSlots; i++)
        {

            if (items[i] == null)
            {

                items[i] = newItem;
                //Debug.Log($"[Inventory] Aggiunto '{newItem.displayName}' nello slot {i}");
                OnInventoryChanged?.Invoke();
                return true;

            }
        }

        //se viene eseguito questo codice vuol dire che non c'erano slot liberi
        //Debug.Log("Inventario pieno!");
        return false;

    }
    //Metodo per rimuovere l'oggetto selezionato nell'inventario
    public bool RemoveItem()
    {
        if(selectedIndex >= 0 && selectedIndex < 5)
        {
            //Controllo che l'oggetto selezionato (che deve essere rimosso) esista 
            if (items[selectedIndex] != null)
            {
                //Se esiste, allora lo elimino
                items[selectedIndex] = null;

                //Riporto l'indice dello slot selezionato a -1 (niente è selezionato)
                selectedIndex = -1;

                //Debug.Log("Oggetto rimosso");


                OnInventoryChanged?.Invoke();
                OnSelectionChanged?.Invoke(selectedIndex);

                return true;
            }
        }

        return false;
    }

    public InventoryItem GetItem(int index)
    {
        //Prevengo l'accesso out of range
        if(index < 0 || index >= MaxSlots)
        {

            return null;

        }

        return items[index];

    }

    public void SelectSlot(int index)
    {
        //Prevengo accesso out of range
        if(index < 0 || index >= MaxSlots)
        {
            //Debug.Log($"[Inventory] SelectSlot({index}) fuori range");
            return;

        }
        //Se seleziono uno slot vuoto allora deseleziono l'eventuale oggetto selezionato in precedenza
        if (items[index] == null)
        {
            //Debug.Log($"[Inventory] SelectSlot({index})  slot vuoto, nessuna selezione");
        }
        //Seleziono l'oggetto
        else
        {

            selectedIndex = index;

            //Debug.Log($"[Inventory] SelectSlot({index})  selezionato '{items[index].displayName}'");


        }

        OnSelectionChanged?.Invoke(selectedIndex);

    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public bool HasSelectedItem()
    {
        return selectedIndex >= 0 && selectedIndex < MaxSlots && items[selectedIndex] != null;
    }
    public InventoryItem GetSelectedItem()
    {
        if (!HasSelectedItem())
            return null;

        return items[selectedIndex];
    }


}
