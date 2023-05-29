using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ModelPreviewsDisplay : MonoBehaviour
{
    public string saveFolder = "SavedModels"; // Replace with the desired save folder name
    public GameObject previewPrefab;
    public Transform panelParent;
    public Text feedbackText;
    public string apiUrl = "http://192.168.1.65:5000"; // Replace with your server URL
    public string userEmail = "andreialexftw@gmail.com"; // Replace with the desired user email

    private void Start()
    {
        //StartCoroutine(LoadModelPreviews());
    }

    public void EnablePreviews()
    {
        StartCoroutine(LoadModelPreviews());
    }

    private IEnumerator LoadModelPreviews()
    {
        // Wait for ModelLoader to finish saving the preview images
        yield return new WaitForEndOfFrame();

        Debug.Log("!!!!!!!!!!!!!!!IMPORTING THE MODELS!!!!!!!!!!!!!");

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

        feedbackText.text = "Models have been generated. Click one to spawn!";
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
        StartCoroutine(GetModelData(modelName));
    }

    private IEnumerator GetModelData(string modelName)
    {
        Debug.Log("INTRU AICI");
        string email = "andreialexftw@gmail.com"; // Replace with the desired user email

        string url = apiUrl + "/model/" + modelName + "/" + email;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        string currentModelPath = Path.Combine(Application.persistentDataPath, "CurrentModel");
        if (Directory.Exists(currentModelPath))
        {
            // Delete the folder and all its contents
            Directory.Delete(currentModelPath, true);
            Debug.Log("Deleted existing currentModel folder.");
        }

        // Create the "SavedModels" folder if it doesn't exist
        Directory.CreateDirectory(currentModelPath);
        Debug.Log("Created SavedModels folder.");

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            ModelData modelData = JsonUtility.FromJson<ModelData>(responseJson);

            //string modelFolder = currentModelPath;

            StartCoroutine(DownloadAndSaveModel(apiUrl + modelData.obj, currentModelPath));
            //StartCoroutine(DownloadAndSaveImage(apiUrl + modelData.preview, currentModelPath));
            StartCoroutine(DownloadAndSaveMtl(apiUrl + modelData.mtl, currentModelPath));

            foreach (TextureData texture in modelData.textures)
            {
                string texturePath = apiUrl + "/files/" + userEmail + "/" + modelData.name + "/" + texture.filename;
                StartCoroutine(DownloadAndSaveTexture(texturePath, currentModelPath));
            }

            // Use the model data to spawn the object
            SpawnObject(modelData.obj, modelData.mtl, modelData.textures);
        }
        else
        {
            Debug.LogError("Failed to fetch model data: " + request.error);
        }
    }

    private void SpawnObject(string objUrl, string mtlUrl, TextureData[] textures)
    {
        // Implement the OBJLoader and texture loading logic to spawn the object using the provided URLs and textures
        // ...

        // For example, you can use the following code to spawn a primitive cube as a placeholder:
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = Vector3.zero;
        cube.transform.localScale = Vector3.one;
    }

    IEnumerator DownloadAndSaveModel(string url, string saveFolder)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string savePath = Path.Combine(saveFolder, Path.GetFileName(url));
            File.WriteAllBytes(savePath, request.downloadHandler.data);
            Debug.Log("Model saved: " + savePath);
        }
        else
        {
            Debug.LogError("Failed to download model: " + request.error);
        }
    }

    IEnumerator DownloadAndSaveTexture(string url, string saveFolder)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            byte[] textureBytes = texture.EncodeToPNG();

            string savePath = Path.Combine(saveFolder, Path.GetFileName(url));
            File.WriteAllBytes(savePath, textureBytes);
            Debug.Log("Texture saved: " + savePath);
        }
        else
        {
            Debug.LogError("Failed to download texture: " + request.error + " " + url);
        }
    }

    IEnumerator DownloadAndSaveImage(string url, string saveFolder)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            byte[] imageBytes = texture.EncodeToJPG();

            string savePath = Path.Combine(saveFolder, "preview.jpg");
            File.WriteAllBytes(savePath, imageBytes);
            Debug.Log("Preview image saved: " + savePath);
        }
        else
        {
            Debug.LogError("Failed to download preview image: " + request.error);
        }
    }

    IEnumerator DownloadAndSaveMtl(string url, string saveFolder)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string savePath = Path.Combine(saveFolder, Path.GetFileName(url));
            File.WriteAllText(savePath, request.downloadHandler.text);
            Debug.Log("MTL file saved: " + savePath);
        }
        else
        {
            Debug.LogError("Failed to download MTL file: " + request.error);
        }
    }

    [System.Serializable]
    private class ModelData
    {
        public string obj;
        public string preview;
        public TextureData[] textures;
        public string name;
        public string mtl;
    }

    [System.Serializable]
    private class TextureData
    {
        public string filename;
        public string path;
    }

    [System.Serializable]
    public class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
