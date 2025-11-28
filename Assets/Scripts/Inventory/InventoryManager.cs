using UnityEngine;
using System;

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
                OnInventoryChanged?.Invoke();
                return true;

            }
        }

        //se viene eseguito questo codice vuol dire che non c'erano slot liberi
        Debug.Log("Inventario pieno!");
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

            return;

        }
        //Se seleziono uno slot vuoto allora deseleziono l'eventuale oggetto selezionato in precedenza
        if (items[index] == null)
        {

            selectedIndex = -1;

        }
        //Seleziono l'oggetto
        else
        {

            selectedIndex = index;

        }

        OnSelectionChanged?.Invoke(selectedIndex);

    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }



}
