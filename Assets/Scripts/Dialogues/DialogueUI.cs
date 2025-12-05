using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce la UI dei dialoghi a partire da un DialogueConversation.
/// Mostra le linee dell'NPC una alla volta, poi le scelte, gestendo nodi e scelte single-use.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Root del pannello di dialogo (es. un GameObject dentro il Canvas)")]
    public GameObject dialogueRoot;

    [Tooltip("Testo in cui mostrare le frasi dell'NPC")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Oggetto che conterrà i pulsanti delle scelte (es. un GameObject con VerticalLayoutGroup)")]
    public Transform choicesContainer;

    [Tooltip("Prefab di un pulsante per una scelta (Button + TextMeshProUGUI figlio)")]
    public GameObject choiceButtonPrefab;

    [Header("Impostazioni input")]
    [Tooltip("Se true, puoi avanzare le linee col tasto spazio")]
    public bool advanceWithSpace = true;

    [Tooltip("Se true, puoi avanzare le linee con click sinistro del mouse")]
    public bool advanceWithClick = false;

    // --- Stato interno del sistema di dialogo ---

    // Conversazione corrente (ScriptableObject)
    private DialogueConversation currentConversation;

    // Indice del nodo corrente all'interno di currentConversation.nodes
    private int currentNodeIndex = -1;

    // Indice della linea corrente dentro il node.lines
    private int currentLineIndex = 0;

    // True se stiamo ancora mostrando le linee dell'NPC (prima delle scelte)
    private bool isShowingLines = false;

    // True se il dialogo è attivo (canvas visibile e input abilitato)
    private bool isDialogueActive = false;

    // Nodi già visitati: se torniamo su un nodo già visitato, saltiamo direttamente alle scelte
    private HashSet<int> visitedNodes = new HashSet<int>();

    // Scelte single-use già usate, identificate da string ID (custom o generato)
    // Esempio ID auto-generato: "nodeIndex_choiceIndex"
    private HashSet<string> usedSingleUseChoices = new HashSet<string>();

    private void Awake()
    {
        // All'avvio nascondiamo il pannello di dialogo
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isDialogueActive)
            return;

        // Se stiamo mostrando le linee dell'NPC, possiamo gestire l'input per andare avanti
        if (isShowingLines)
        {
            bool advance = false;

            if (advanceWithSpace && Input.GetKeyDown(KeyCode.Space))
                advance = true;

            if (advanceWithClick && Input.GetMouseButtonDown(0))
                advance = true;

            if (advance)
            {
                AdvanceLine();
            }
        }
    }

    /// <summary>
    /// Avvia una conversazione, partendo dal nodo startNodeIndex.
    /// Puoi chiamare questo metodo da un trigger, da uno script NPC, ecc.
    /// </summary>
    public void StartConversation(DialogueConversation conversation)
    {
        if (conversation == null || conversation.nodes == null || conversation.nodes.Length == 0)
        {
            Debug.LogWarning("DialogueUI: conversazione nulla o senza nodi.");
            return;
        }

        currentConversation = conversation;

        // Reset stato
        visitedNodes.Clear();
        usedSingleUseChoices.Clear();
        currentNodeIndex = -1;
        currentLineIndex = 0;
        isDialogueActive = true;

        // Mostra il pannello
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        // Vai al nodo di partenza
        GoToNode(currentConversation.startNodeIndex);
    }

    /// <summary>
    /// Vai a un nodo specifico della conversazione.
    /// Se il nodo è nuovo, mostra le linee una alla volta.
    /// Se è già stato visitato, salta direttamente alle scelte.
    /// </summary>
    private void GoToNode(int nodeIndex)
    {
        if (currentConversation == null)
        {
            Debug.LogWarning("DialogueUI: nessuna conversazione attiva.");
            EndDialogue();
            return;
        }

        if (nodeIndex < 0 || nodeIndex >= currentConversation.nodes.Length)
        {
            Debug.LogWarning($"DialogueUI: nodeIndex {nodeIndex} fuori range, chiudo dialogo.");
            EndDialogue();
            return;
        }

        currentNodeIndex = nodeIndex;
        currentLineIndex = 0;

        DialogueNode node = currentConversation.nodes[currentNodeIndex];

        // Puliamo sempre le scelte precedenti
        ClearChoices();

        // Se è la prima volta che entriamo in questo nodo, mostriamo le linee
        bool firstTime = !visitedNodes.Contains(currentNodeIndex);
        if (firstTime)
        {
            visitedNodes.Add(currentNodeIndex);
            // Se ci sono linee, iniziamo a mostrarle una alla volta
            if (node.lines != null && node.lines.Length > 0)
            {
                isShowingLines = true;
                ShowCurrentLine();
            }
            else
            {
                // Nessuna linea, andiamo direttamente alle scelte
                isShowingLines = false;
                ShowChoicesForCurrentNode();
            }
        }
        else
        {
            // Nodo già visitato: saltiamo direttamente alle scelte
            isShowingLines = false;
            ShowChoicesForCurrentNode();
        }
    }

    /// <summary>
    /// Mostra la linea corrente del nodo attuale.
    /// </summary>
    private void ShowCurrentLine()
    {
        DialogueNode node = currentConversation.nodes[currentNodeIndex];

        if (node.lines == null || node.lines.Length == 0)
        {
            // Nodo senza linee, andiamo direttamente alle scelte
            isShowingLines = false;
            ShowChoicesForCurrentNode();
            return;
        }

        // Clamp di sicurezza
        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, node.lines.Length - 1);

        if (dialogueText != null)
        {
            dialogueText.text = node.lines[currentLineIndex];
        }
    }

    /// <summary>
    /// Avanza alla prossima linea. Se non ci sono più linee, mostra le scelte.
    /// </summary>
    private void AdvanceLine()
    {
        if (currentConversation == null || currentNodeIndex < 0)
            return;

        DialogueNode node = currentConversation.nodes[currentNodeIndex];

        if (node.lines == null || node.lines.Length == 0)
        {
            // Nessuna linea da avanzare
            isShowingLines = false;
            ShowChoicesForCurrentNode();
            return;
        }

        // Se NON siamo all'ultima linea, andiamo alla successiva
        if (currentLineIndex < node.lines.Length - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else
        {
            // Abbiamo finito le linee di questo nodo, ora mostriamo le scelte
            isShowingLines = false;
            ShowChoicesForCurrentNode();
        }
    }

    /// <summary>
    /// Mostra tutte le scelte disponibili del nodo corrente.
    /// Filtra quelle single-use già usate.
    /// </summary>
    private void ShowChoicesForCurrentNode()
    {
        if (currentConversation == null || currentNodeIndex < 0)
        {
            EndDialogue();
            return;
        }

        DialogueNode node = currentConversation.nodes[currentNodeIndex];

        // Se il nodo non ha scelte o l'array è vuoto, chiudiamo il dialogo
        if (node.choices == null || node.choices.Length == 0)
        {
            // Possibile comportamento alternativo: restare nel nodo, ma qui chiudo
            EndDialogue();
            return;
        }

        ClearChoices();

        bool anyChoiceVisible = false;

        for (int i = 0; i < node.choices.Length; i++)
        {
            DialogueChoice choice = node.choices[i];

            // Se la scelta è single-use e già usata, la saltiamo
            if (choice.singleUse && IsSingleUseChoiceAlreadyUsed(currentNodeIndex, i, choice))
            {
                continue;
            }

            // Istanziamo un nuovo pulsante come figlio di choicesContainer
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer);

            // Settiamo il testo del pulsante
            TextMeshProUGUI choiceText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null)
            {
                choiceText.text = choice.text;
            }

            // Aggiungiamo listener al Button
            Button btn = choiceObj.GetComponent<Button>();
            if (btn != null)
            {
                int capturedIndex = i; // catturiamo l'indice per la lambda
                btn.onClick.AddListener(() => OnChoiceClicked(capturedIndex));
            }

            anyChoiceVisible = true;
        }

        // Se non c'è nessuna scelta visualizzabile (es. tutte single-use già usate), chiudiamo il dialogo
        if (!anyChoiceVisible)
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Richiamato quando l'utente clicca una scelta.
    /// Applica la logica di single-use, nextNodeIndex e endsDialogue.
    /// </summary>
    private void OnChoiceClicked(int choiceIndex)
    {
        if (currentConversation == null || currentNodeIndex < 0)
            return;

        DialogueNode node = currentConversation.nodes[currentNodeIndex];

        if (node.choices == null || choiceIndex < 0 || choiceIndex >= node.choices.Length)
        {
            Debug.LogWarning("DialogueUI: choiceIndex fuori range.");
            return;
        }

        DialogueChoice choice = node.choices[choiceIndex];

        // Se è single-use, la segniamo come già usata
        if (choice.singleUse)
        {
            MarkSingleUseChoiceAsUsed(currentNodeIndex, choiceIndex, choice);
        }

        // Se questa scelta termina il dialogo
        if (choice.endsDialogue)
        {
            EndDialogue();
            return;
        }

        // Se nextNodeIndex è fuori range o -1 => chiudiamo il dialogo
        if (choice.nextNodeIndex < 0 || choice.nextNodeIndex >= currentConversation.nodes.Length)
        {
            EndDialogue();
            return;
        }

        // Altrimenti andiamo al nodo indicato
        GoToNode(choice.nextNodeIndex);
    }

    /// <summary>
    /// Genera un ID univoco per una scelta single-use.
    /// Usa customId se presente, altrimenti "nodeIndex_choiceIndex".
    /// </summary>
    private string GetChoiceId(int nodeIndex, int choiceIndex, DialogueChoice choice)
    {
        if (!string.IsNullOrEmpty(choice.customId))
            return choice.customId;

        // ID auto-generato leggibile
        return $"{nodeIndex}_{choiceIndex}";
    }

    /// <summary>
    /// Ritorna true se la scelta single-use è già stata usata.
    /// </summary>
    private bool IsSingleUseChoiceAlreadyUsed(int nodeIndex, int choiceIndex, DialogueChoice choice)
    {
        string id = GetChoiceId(nodeIndex, choiceIndex, choice);
        return usedSingleUseChoices.Contains(id);
    }

    /// <summary>
    /// Segna una scelta single-use come usata.
    /// </summary>
    private void MarkSingleUseChoiceAsUsed(int nodeIndex, int choiceIndex, DialogueChoice choice)
    {
        string id = GetChoiceId(nodeIndex, choiceIndex, choice);
        usedSingleUseChoices.Add(id);
    }

    /// <summary>
    /// Distrugge tutti i pulsanti di scelta attualmente creati.
    /// </summary>
    private void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(choicesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Termina il dialogo, nasconde il pannello e resetta lo stato minimo.
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;
        isShowingLines = false;
        currentConversation = null;
        currentNodeIndex = -1;
        currentLineIndex = 0;

        ClearChoices();

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        // Qui potresti anche lanciare un evento per notificare altri sistemi:
        // es. sbloccare movimento player, ecc.
    }
}
