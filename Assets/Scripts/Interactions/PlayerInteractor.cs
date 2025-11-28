using Unity.VisualScripting;
using UnityEngine;


public class PlayerInteractor : MonoBehaviour
{

    [Header("Raycast")]
    public Camera playerCamera;
    public float maxDistance = 3f;
    public LayerMask interactableMask;

    [Header("UI")]
    public GameObject interactionUI; //E dentro al cerchio
    public TMPro.TextMeshProUGUI interactionText;
    public UnityEngine.UI.Image crosshair; //Pallino al centro dello schermo

    [Header("Finestre bloccanti (opzionali)")]
    public InspectUI inspectUI;                // la UI full-screen degli oggetti
    //public DialogueUI dialogueUI;              // la UI di dialogo NPC

    private IInteractable currentInteractable;

    private void Update()
    {

        // Se è aperta una finestra di ispezione o dialogo, non interagire con altro
        if (inspectUI != null && inspectUI.IsOpen)
        {
            return;
        }

        HandleRaycast();
        HandleInteraction();
    }


    private void HandleRaycast()
    {
        currentInteractable = null;
        interactionUI.SetActive(false);
        crosshair.color = Color.white;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if(Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if(interactable != null)
            {
                currentInteractable = interactable;

                Debug.Log("Oggetto visto");

                //Aggiorno la UI
                if (interactionUI != null)
                    interactionUI.SetActive(true);

                if (interactionText != null)
                    interactionText.text = interactable.GetInteractionText();

                if (crosshair != null)
                    crosshair.color = Color.red;
            }
        }
    }

    private void HandleInteraction()
    {

        if(currentInteractable == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }

    }
}
