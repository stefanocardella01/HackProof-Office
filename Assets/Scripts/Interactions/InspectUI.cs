using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectUI : MonoBehaviour
{

    [Header("Riferimenti UI")]
    public GameObject rootPanel; //pannello full-screen
    public Image objectImage; //immagine principale dell'oggetto
    public TextMeshProUGUI objectNameText; // "ESC - Esci"
    public TextMeshProUGUI leftHintText; // "ESC - Esci"
    public TextMeshProUGUI rightHintText; // "E - Aggiungi all'inventario"

    public bool IsOpen { get; private set; }

    private InspectableObject currentObject;
    private InventoryManager inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
        CloseImmediate();
    }

    public void Open(InspectableObject obj)
    {
        currentObject = obj;

        if(rootPanel != null)
        {
            rootPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        IsOpen = true;

        if (objectImage != null)
            objectImage.sprite = obj.inspectImage;

        if (objectNameText != null)
            objectNameText.text = obj.objectName;

        if (leftHintText != null)
            leftHintText.text = "Esc - Esci";

        if (rightHintText != null)
            rightHintText.text = "E - Aggiungi all'inventario";

        //TO DO: bloccare movimento e cursore per spostarsi
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseImmediate();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryAddToInventory();
        }
    }

    private void TryAddToInventory()
    {
        if(currentObject == null || inventory == null)
        {
            return;
        }

        InventoryItem item = currentObject.ToInventoryItem();
        bool added = inventory.AddItem(item);

        if (added)
        {
            Destroy(currentObject.gameObject);
            CloseImmediate();
        }
        else
        {
            Debug.Log("Inventario pieno, impossibile aggiungere l'oggetto.");
        }
    }

    public void CloseImmediate()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
        else
            gameObject.SetActive(false);

        IsOpen = false;
        currentObject = null;
    }
}
