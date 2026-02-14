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

    [Header("Canvas smartphone")]
    public GameObject hudSmartphone;

    [Header("Canvas Mission CheckList")]
    public GameObject hudMissionCheckList;

    [Header("Bloccare movimento mentre ispeziono")]
    public FirstPersonController playerController;
    public StarterAssetsInputs starterInputs;

    [Header("Inspect Orientation")]
    public Vector3 modelFaceAxis = Vector3.up;   // +Y del modello = faccia principale (normale)
    public Vector3 modelUpAxis = Vector3.forward; // +Z del modello = “su” del modello


    [Header("3D Model Inspection")]
    public Transform modelAnchor;        // punto dove spawnare il modello (pivot)
    public Camera inspectCamera;         // camera che guarda il modello
    public string inspectLayerName = "InspectModel";
    public float rotationSpeed = 120f;
    public float zoomSpeed = 0.5f;
    public float minZoomDistance = 0.5f;
    public float maxZoomDistance = 3f;

    [Header("Fit & Zoom (dinamico per oggetto)")]
    public float targetBoxDiagonal = 1.2f;     // diagonale desiderata dopo normalizzazione (prova 1.0–1.6)
    public float fitPadding = 1.15f;           // margine anti-taglio (1.10–1.30)
    public float zoomOutMultiplier = 3f;       // quanto puoi allontanarti rispetto al fit
    public bool autoComputeZoomLimits = true;

    private float runtimeMinZoom;
    private float runtimeMaxZoom;



    // Audio
    [SerializeField] private ManagerAudio mixer;

    public bool IsOpen;

    private InspectableObject currentObject;
    private InventoryManager inventory;

    // true se aperta da inventario (non dal mondo)
    private bool openedFromInventory = false;

    private GameObject currentModelInstance;
    private int inspectLayer;

    private float currentZoomDistance = 1f;
    private float targetZoomDistance = 1f;

    private DialogueUI dialogueUI;
    private ReportUI reportUI;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>();

        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (starterInputs == null)
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();

        dialogueUI = FindFirstObjectByType<DialogueUI>();
        reportUI = FindFirstObjectByType<ReportUI>();

        inspectLayer = LayerMask.NameToLayer(inspectLayerName);

        CloseImmediate();
    }

    public void Open(InspectableObject obj)
    {
        if ((dialogueUI != null && dialogueUI.IsDialogueActive) ||
            (reportUI != null && reportUI.IsOpen))
            return;

        openedFromInventory = false;
        currentObject = obj;

        //NASCONDI L'OGGETTO NEL MONDO MENTRE LO ISPEZIONI
        if (currentObject != null)
            currentObject.gameObject.SetActive(false);

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(false);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(false);

        // Blocca il movimento del personaggio
        if (playerController != null)
            playerController.enabled = false;
            playerController.ForceStopWalking();

        // Blocca l'input di look e movimento dagli Starter Assets
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        mixer.SetDialog();

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

        if ((dialogueUI != null && dialogueUI.IsDialogueActive) ||
            (reportUI != null && reportUI.IsOpen))
            return;
        openedFromInventory = true;
        currentObject = null; // non stiamo guardando un oggetto nel mondo

        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(false);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;
            playerController.ForceStopWalking();

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        mixer.SetDialog();

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

        // reset anchor
        modelAnchor.rotation = Quaternion.identity;

        // istanzio come figlio dell'anchor
        currentModelInstance = Instantiate(prefab, modelAnchor);
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = Quaternion.identity;
        currentModelInstance.transform.localScale = Vector3.one;

        SetLayerRecursively(currentModelInstance, inspectLayer);


        BoxCollider box = currentModelInstance.GetComponentInChildren<BoxCollider>(true);
        if (box == null)
        {
            Debug.LogWarning("[InspectUI] Nessun BoxCollider trovato sul prefab di ispezione.");
            return;
        }

        // centro reale in world (rispetta box.center != 0)
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 localCenter = modelAnchor.InverseTransformPoint(worldCenter);
        currentModelInstance.transform.localPosition = -localCenter;

        // funzione per ottenere size world del BoxCollider (dopo centratura/scala)
        Vector3 GetWorldSizeFromBox(BoxCollider b)
        {
            Vector3 ls = b.transform.lossyScale;
            return new Vector3(
                Mathf.Abs(b.size.x * ls.x),
                Mathf.Abs(b.size.y * ls.y),
                Mathf.Abs(b.size.z * ls.z)
            );
        }

    
        Vector3 worldSize = GetWorldSizeFromBox(box);
        float diag = worldSize.magnitude;

        if (diag > 0.0001f)
        {
            float scaleFactor = targetBoxDiagonal / diag;

            // clamp: evita oggetti invisibili o giganteschi
            scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 10f);

            currentModelInstance.transform.localScale *= scaleFactor;
        }

        // ricalcolo size dopo la scala
        worldSize = GetWorldSizeFromBox(box);


        Vector3 worldFaceDir = -inspectCamera.transform.forward;
        Vector3 worldUpDir = inspectCamera.transform.up;

        Quaternion worldRot = Quaternion.LookRotation(worldFaceDir, worldUpDir);
        Quaternion fix = Quaternion.Inverse(Quaternion.LookRotation(modelFaceAxis, modelUpAxis));
        modelAnchor.rotation = worldRot * fix;


        // bounding sphere radius dal box (robusto per tutte le rotazioni)
        float radius = worldSize.magnitude * 0.5f;

        // FOV verticale
        float vFov = inspectCamera.fieldOfView * Mathf.Deg2Rad;

        // FOV orizzontale calcolato dall’aspect (RT 900x900 => aspect ~ 1)
        float aspect = inspectCamera.aspect;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * aspect);

        // uso l’FOV più “stretto” per garantire che stia dentro sia in verticale che in orizzontale
        float minFov = Mathf.Min(vFov, hFov);

        // distanza minima per far entrare la sfera nel frustum
        float fitDistance = (radius / Mathf.Tan(minFov * 0.5f)) * fitPadding;

        // evita che la near-clip tagli quando sei troppo vicino
        float nearSafe = radius + inspectCamera.nearClipPlane * 1.5f;
        fitDistance = Mathf.Max(fitDistance, nearSafe);

        if (autoComputeZoomLimits)
        {
            runtimeMinZoom = fitDistance;

            // max zoom = quanto vuoi allontanarti
            runtimeMaxZoom = fitDistance * Mathf.Max(1f, zoomOutMultiplier);
        }
        else
        {
            runtimeMinZoom = minZoomDistance;
            runtimeMaxZoom = maxZoomDistance;
            fitDistance = Mathf.Clamp(fitDistance, runtimeMinZoom, runtimeMaxZoom);
        }

        // clamp solo “di sicurezza”, NON legato a maxZoomDistance
        runtimeMinZoom = Mathf.Max(runtimeMinZoom, minZoomDistance);          // evita troppo vicino
        runtimeMaxZoom = Mathf.Max(runtimeMaxZoom, runtimeMinZoom + 0.001f);  // coerente


        currentZoomDistance = Mathf.Clamp(fitDistance, runtimeMinZoom, runtimeMaxZoom);
        targetZoomDistance = currentZoomDistance;

        // posiziono camera
        inspectCamera.transform.LookAt(modelAnchor.position);
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
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, runtimeMinZoom, runtimeMaxZoom);

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

            currentObject.gameObject.SetActive(true);
        }

        // Riattiva l'HUD
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        // Riattiva l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(true);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(true);

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

        mixer.SetNormal();

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
