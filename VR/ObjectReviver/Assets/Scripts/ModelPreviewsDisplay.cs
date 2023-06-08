using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Dummiesman;

public class ModelPreviewsDisplay : MonoBehaviour
{
    public string saveFolder = "SavedModels"; // Replace with the desired save folder name
    public GameObject previewPrefab;
    public Transform panelParent;
    public Text feedbackText;
    public string apiUrl = "http://192.168.1.66:5000"; // Replace with your server URL
    public string userEmail = "andreialexbunea@yahoo.com"; // Replace with the desired user email
    private GameObject spawnedObject;
    public float translationIncrement = 0.1f;
    public float rotationIncrement = 90.0f;

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
        feedbackText.text = "Model selected! Now Click the \"Spawn model\" button!";
        StartCoroutine(GetModelData(modelName));
    }

    private IEnumerator GetModelData(string modelName)
    {
        Debug.Log("INTRU AICI");
        string email = userEmail; // Replace with the desired user email

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

        }
        else
        {
            Debug.LogError("Failed to fetch model data: " + request.error);
        }
    }

    private void DestroySpawnedObject()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    public void SpawnObject()
    {
        DestroySpawnedObject();
        string currentModelPath = Path.Combine(Application.persistentDataPath, "CurrentModel");

        // Load the OBJ file path
        string objFilePath = Path.Combine(currentModelPath, "texturedMesh.obj");

        // Load the MTL file path
        string mtlFilePath = Path.Combine(currentModelPath, "texturedMesh.mtl");

        //feedbackText.text = objFilePath + " (Size: " + new FileInfo(objFilePath).Length + " bytes)\n" + mtlFilePath + " (Size: " + new FileInfo(mtlFilePath).Length + " bytes)\n";

        // Print directory contents
        string[] files = Directory.GetFiles(currentModelPath);
        string directoryContent = "CurrentModel directory contents:\n";
        foreach (string file in files)
        {
            directoryContent += Path.GetFileName(file) + " (Size: " + new FileInfo(file).Length + " bytes)\n";
        }
        //feedbackText.text += directoryContent;

        try
        {
            spawnedObject = new OBJLoader().Load(objFilePath, mtlFilePath);

            // Set the position and scale of the imported object
            spawnedObject.transform.position = new Vector3(0, 0, 2);
            spawnedObject.transform.localScale = Vector3.one;

            // Add necessary components for rendering
            MeshRenderer meshRenderer = spawnedObject.AddComponent<MeshRenderer>();
            //MeshFilter meshFilter = spawnedObject.AddComponent<MeshFilter>();

            // Load the mesh from the OBJ file
            //ObjImporter objImporter = new ObjImporter();
            //Mesh loadedMesh = objImporter.ImportFile(objFilePath);
            //meshFilter.mesh = loadedMesh;

            // Place the object on the floor
            PlaceObjectOnFloor(spawnedObject);

            // Display the contents of the "CurrentModel" directory
            /*string[] files = Directory.GetFiles(currentModelPath);
            string directoryContent = "CurrentModel directory contents:\n";
            foreach (string file in files)
            {
                directoryContent += Path.GetFileName(file) + "\n";
            }
            feedbackText.text = directoryContent + "Model Spawned!";
            feedbackText.text += " Loaded!";*/
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to spawn model: " + ex.Message);
            feedbackText.text = ex.Message + "\n" + objFilePath + "\n" + mtlFilePath;
        }
    }

    private void PlaceObjectOnFloor(GameObject obj)
    {
        // Find the floor's position
        RaycastHit hit;
        if (Physics.Raycast(obj.transform.position, Vector3.down, out hit))
        {
            // Calculate the position where the object should be placed
            Vector3 objectPosition = hit.point;
            Renderer renderer = obj.GetComponent<Renderer>();
            Vector3 objectBounds = renderer.bounds.extents;

            // Adjust the object's position based on its size
            objectPosition.y += objectBounds.y;

            // Set the object's position to be on the floor
            obj.transform.position = objectPosition;
        }
        else
        {
            Debug.LogWarning("Floor not found. Object will be spawned at default position.");
        }
    }

    public void moveUp()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Translate(Vector3.up * translationIncrement);
        }
    }

    public void moveDown()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Translate(Vector3.down * translationIncrement);
        }
    }

    public void rotateXLeft()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.left * 10.0f, Space.Self);
        }
    }

    public void rotateXRight()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.right * 10.0f, Space.Self);
        }
    }

    public void rotateYLeft()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.down * 10.0f, Space.Self);
        }
    }

    public void rotateYRight()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.up * 10.0f, Space.Self);
        }
    }

    public void rotateZLeft()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.forward * 10.0f, Space.Self);
        }
    }

    public void rotateZRight()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.Rotate(Vector3.back * 10.0f, Space.Self);
        }
    }



    IEnumerator DownloadAndSaveModel(string url, string saveFolder)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        //feedbackText.text = saveFolder + " " + url;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string savePath = Path.Combine(saveFolder, "texturedMesh.obj");
            //feedbackText.text += savePath;
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
            string savePath = Path.Combine(saveFolder, "texturedMesh.mtl");
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
