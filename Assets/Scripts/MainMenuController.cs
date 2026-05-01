using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button defaultSelectedButton; // The Play button

    private void Start()
    {
        // Select the default button on start
        if (defaultSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
        }
    }
}