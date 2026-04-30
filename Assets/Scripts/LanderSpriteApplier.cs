using UnityEngine;

public class LanderSpriteApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer landerSpriteRenderer;

    private void Start()
    {
        ApplySelectedLanderSprite();
    }

    private void ApplySelectedLanderSprite()
    {
        // Load selected lander index from PlayerPrefs
        int selectedLander = PlayerPrefs.GetInt("SelectedLander", 0);

        // Load the corresponding sprite from Resources
        Sprite selectedSprite = LoadLanderSprite(selectedLander);

        if (selectedSprite != null && landerSpriteRenderer != null)
        {
            landerSpriteRenderer.sprite = selectedSprite;
            Debug.Log("Applied lander sprite: " + selectedSprite.name);
        }
        else
        {
            Debug.LogWarning("Failed to apply lander sprite!");
        }
    }

    private Sprite LoadLanderSprite(int landerIndex)
    {
        // Load sprites from Resources/Landers/ folder
        string spritePath = "";

        switch (landerIndex)
        {
            case 0:
                spritePath = "Landers/Lander1";
                break;
            case 1:
                spritePath = "Landers/Lander2";
                break;
            case 2:
                spritePath = "Landers/Lander3";
                break;
            default:
                spritePath = "Landers/Lander1";
                break;
        }

        Sprite sprite = Resources.Load<Sprite>(spritePath);

        if (sprite == null)
        {
            Debug.LogError("Could not load sprite from: Resources/" + spritePath);
        }

        return sprite;
    }
}