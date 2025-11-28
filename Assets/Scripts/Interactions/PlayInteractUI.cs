using UnityEngine;

public class PlayInteractUI : MonoBehaviour
{

    [SerializeField] private GameObject containerGameObject;

    private void Show()
    {

        containerGameObject.SetActive(true);

    }

    private void Hide()
    {

        containerGameObject.SetActive(false);

    }

}
