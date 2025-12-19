using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class InspectUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public RawImage objectModelView;             // immagine che mostra la RenderTexture
    public TextMeshProUGUI objectNameText;       // es: "Post-it"

    [Header("Canvas con inventario, interact text e crosshair")]
    public GameObject hudCanvas;

    [Header("Bloccare movimento mentre ispeziono")]
    public FirstPersonController playerController;
    public StarterAssetsInputs starterInputs;

    [Header("3D Model Inspection")]
    public Transform modelAnchor;        // punto dove spawnare il modello (pivot)
    public Camera inspectCamera;         // camera che guarda il modello
    public string inspectLayerName = "InspectModel";
    public float rotationSpeed = 120f;
    public float zoomSpeed = 0.5f;
    public float minZoomDistance = 0.5f;
    public float maxZoomDistance = 3f;

    public bool IsOpen;

    private InspectableObject currentObject;
    private InventoryManager inventory;

    // true se aperta da inventario (non dal mondo)
    private bool openedFromInventory = false;

    private GameObject currentModelInstance;
    private int inspectLayer;

    private float currentZoomDistance = 1f;
    private float targetZoomDistance = 1f;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>();

        // Se non li hai assegnati a mano nell'Inspector, prova a recuperarli
        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (starterInputs == null)
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();

        inspectLayer = LayerMask.NameToLayer(inspectLayerName);

        CloseImmediate();
    }

    public void Open(InspectableObject obj)
    {
        openedFromInventory = false;
        currentObject = obj;

        //NASCONDI L'OGGETTO NEL MONDO MENTRE LO ISPEZIONI
        if (currentObject != null)
            currentObject.gameObject.SetActive(false);

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Blocca il movimento del personaggio
        if (playerController != null)
            playerController.enabled = false;

        // Blocca l'input di look e movimento dagli Starter Assets
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        SpawnModel(obj.inspectPrefab);

        gameObject.SetActive(true);
        gameObject.transform.Find("AddText").gameObject.SetActive(true);

        IsOpen = true;

        if (objectNameText != null)
            objectNameText.text = obj.objectName;

        // blocco cursore sulla finestra di gioco
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Metodo per ispezionare oggetti già nell'inventario
    public void OpenFromInventory(InventoryItem item)
    {
        openedFromInventory = true;
        currentObject = null; // non stiamo guardando un oggetto nel mondo

        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        SpawnModel(item.inspectPrefab);

        gameObject.SetActive(true);
        gameObject.transform.Find("AddText").gameObject.SetActive(false);

        IsOpen = true;

        if (objectNameText != null)
            objectNameText.text = item.displayName;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SpawnModel(GameObject prefab)
    {
        // pulisco eventuale modello vecchio
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }

        if (prefab == null || modelAnchor == null || inspectCamera == null)
        {
            Debug.LogWarning($"[InspectUI] prefab null? {prefab == null}, modelAnchor null? {modelAnchor == null}, camera null? {inspectCamera == null}");
            return;
        }

        // reset pivot
        modelAnchor.rotation = Quaternion.identity;

        // Istanzio come figlio dell'anchor
        currentModelInstance = Instantiate(prefab, modelAnchor);
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = Quaternion.identity;
        currentModelInstance.transform.localScale = Vector3.one;

        SetLayerRecursively(currentModelInstance, inspectLayer);

        // --- Calcolo bounds per CENTRARE il modello e normalizzare la scala ---
        var renderers = currentModelInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            // 1) centro dei bounds in world-space
            Vector3 worldCenter = bounds.center;
            // 2) lo trasformo nello spazio locale dell'anchor
            Vector3 localCenter = modelAnchor.InverseTransformPoint(worldCenter);
            // 3) sposto il modello in modo che il centro sia sull'anchor
            currentModelInstance.transform.localPosition = -localCenter;

            // 4) normalizzo la scala se vuoi tenere dimensioni simili
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            if (radius > 0.0001f)
            {
                float targetRadius = 0.2f;              // quanto grande lo vuoi nello spazio ispezione
                float scaleFactor = targetRadius / radius;
                scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 10f);

                currentModelInstance.transform.localScale *= scaleFactor;
            }
        }

        // Posiziono la camera a una distanza base
        currentZoomDistance = Mathf.Clamp(1f, minZoomDistance, maxZoomDistance);
        targetZoomDistance = currentZoomDistance;

        inspectCamera.transform.position = modelAnchor.position - inspectCamera.transform.forward * currentZoomDistance;
        inspectCamera.transform.LookAt(modelAnchor.position);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void Update()
    {
        if (!IsOpen) return;

        HandleModelControls();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CloseImmediate();
        }
        else if (!openedFromInventory && Input.GetKeyDown(KeyCode.E))
        {
            TryAddToInventory();
        }
    }

    //  QUI: rotazione con drag del mouse + zoom con rotella
    private void HandleModelControls()
    {
        if (currentModelInstance == null || inspectCamera == null || modelAnchor == null)
            return;

        // ROTAZIONE con drag del mouse (tasto sinistro premuto)
        if (Input.GetMouseButton(0))
        {
            float mouseX = - Input.GetAxis("Mouse X");
            float mouseY = - Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) > 0.001f || Mathf.Abs(mouseY) > 0.001f)
            {
                Vector3 camRight = inspectCamera.transform.right;
                Vector3 camUp = inspectCamera.transform.up;

                // sinistra/destra: intorno all'asse "up" della camera
                modelAnchor.Rotate(camUp, mouseX * rotationSpeed * Time.deltaTime, Space.World);

                // su/giù: intorno all'asse "right" della camera
                modelAnchor.Rotate(camRight, -mouseY * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        // ZOOM con rotella (fluido)
        float scroll = Input.GetAxis("Mouse ScrollWheel");   // valori piccoli, tipo 0.1 / -0.1

        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetZoomDistance -= scroll * zoomSpeed;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }

        currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, Time.deltaTime * 10f);

        inspectCamera.transform.position = modelAnchor.position - inspectCamera.transform.forward * currentZoomDistance;
        inspectCamera.transform.LookAt(modelAnchor.position);
    }

    private void TryAddToInventory()
    {
        if (currentObject == null || inventory == null)
            return;

        InventoryItem item = currentObject.ToInventoryItem();
        bool added = inventory.AddItem(item);

        if (added)
        {
            //Debug.Log("Aggiunto.");
            Destroy(currentObject.gameObject);
            CloseImmediate();
        }
        else
        {
            //Debug.Log("Inventario pieno, impossibile aggiungere l'oggetto.");
        }
    }

    public void CloseImmediate()
    {
        gameObject.SetActive(false);

        //Se l'ispezione viene da un oggetto del mondo e NON dall'inventario
        //  e l'oggetto non è stato distrutto, lo riattivo.
        if (!openedFromInventory && currentObject != null)
        {
            // Se in TryAddToInventory hai fatto Destroy(currentObject.gameObject),
            // allora qui currentObject risulta null (per l'overload di == in Unity)
            // e non entrerà in questo if.
            currentObject.gameObject.SetActive(true);
        }

        // Riattiva l'HUD
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        // Riabilita il movimento del personaggio
        if (playerController != null)
            playerController.enabled = true;

        // Riabilita input di look/movimento
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        IsOpen = false;
        openedFromInventory = false;
        currentObject = null;

        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }

        // Assicurati che il cursore rimanga bloccato e nascosto
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
