using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using StarterAssets;
public class InspectUI : MonoBehaviour
{

    [Header("Riferimenti UI")]
    public Image objectImage; //immagine principale dell'oggetto
    public TextMeshProUGUI objectNameText; // es: "Post-it"

    [Header("Canvas con inventario, interact text e crosshair")]
    public GameObject hudCanvas;

    [Header("Bloccare movimento mentre ispeziono")]
    public FirstPersonController playerController;
    public StarterAssetsInputs starterInputs;


    public bool IsOpen;

    private InspectableObject currentObject;
    private InventoryManager inventory;
    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>();

        // Se non li hai assegnati a mano nell'Inspector, prova a recuperarli
        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (starterInputs == null)
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();

        CloseImmediate();
    }

    public void Open(InspectableObject obj)
    {
        currentObject = obj;

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(false);
        }

        // Blocca il movimento del personaggio
        if (playerController != null)
            playerController.enabled = false;

        // Blocca l'input di look e movimento dagli Starter Assets
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false; // il mouse non controlla più la camera
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        gameObject.SetActive(true);
    
        IsOpen = true;

        if (objectImage != null)
            objectImage.sprite = obj.inspectImage;

        if (objectNameText != null)
            objectNameText.text = obj.objectName;

        //TO DO: bloccare movimento e cursore per spostarsi
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Q))
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
        if (currentObject == null || inventory == null)
        {
            return;
        }

        InventoryItem item = currentObject.ToInventoryItem();
        bool added = inventory.AddItem(item);

        if (added)
        {
            Debug.Log("Aggiunto.");
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

        gameObject.SetActive(false);


        // Riattiva l'HUD
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        //  Riabilita il movimento del personaggio
        if (playerController != null)
            playerController.enabled = true;

        //  Riabilita input di look/movimento
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        IsOpen = false;
        currentObject = null;

        // Assicurati che il cursore rimanga bloccato e nascosto
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
