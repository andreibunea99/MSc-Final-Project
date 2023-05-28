using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ModelPreviewsDisplay : MonoBehaviour
{
    public string saveFolder = "SavedModels"; // Replace with the desired save folder name
    public GameObject previewPrefab;
    public Transform panelParent;

    private void Start()
    {
        StartCoroutine(LoadModelPreviews());
    }

    private IEnumerator LoadModelPreviews()
    {
        // Wait for ModelLoader to finish saving the preview images
        yield return new WaitForEndOfFrame();

        string savedModelsPath = Path.Combine(Application.persistentDataPath, saveFolder);

        if (Directory.Exists(savedModelsPath))
        {
            string[] modelDirectories = Directory.GetDirectories(savedModelsPath);

            /*if (panelParent.childCount > 0)
            {
                Destroy(panelParent.GetChild(0).gameObject);
            }*/

            foreach (string modelDirectory in modelDirectories)
            {
                string imagePath = Path.Combine(modelDirectory, "preview.jpg");
                if (File.Exists(imagePath))
                {
                    Texture2D texture = LoadTexture(imagePath);
                    CreatePreviewImage(texture, modelDirectory);
                }
            }
            Destroy(panelParent.GetChild(0).gameObject);
        }
        else
        {
            Debug.LogError("SavedModels folder not found.");
        }

        // Add GridLayoutGroup component to the panel's parent (Canvas)
        GridLayoutGroup gridLayoutGroup = panelParent.gameObject.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            // If the GridLayoutGroup component is not found, add it
            gridLayoutGroup = panelParent.gameObject.AddComponent<GridLayoutGroup>();
        }

    }

    private Texture2D LoadTexture(string imagePath)
    {
        byte[] imageData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    private void CreatePreviewImage(Texture2D texture, string modelDirectory)
    {
        GameObject previewObject = Instantiate(previewPrefab, panelParent);
        Image image = previewObject.GetComponent<Image>();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        Button button = previewObject.AddComponent<Button>();
        button.onClick.AddListener(() => OnPreviewClicked(modelDirectory));
    }

    private void OnPreviewClicked(string modelDirectory)
    {
        string modelName = Path.GetFileName(modelDirectory);
        Debug.Log("The model '" + modelName + "' has been clicked.");
    }
}
