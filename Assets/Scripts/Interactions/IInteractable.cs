public interface IInteractable
{
    string GetInteractionText();     // "Raccogli Chiave USB", "Parla con Paolo"
    void Interact(PlayerInteractor interactor); // logica dell'interazione
}