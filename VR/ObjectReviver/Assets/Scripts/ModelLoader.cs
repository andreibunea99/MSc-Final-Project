using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ModelLoader : MonoBehaviour
{
    public string apiUrl = "http://192.168.1.66:5000"; // Replace with your server URL
    public string userEmail = "andreialexbunea@yahoo.com"; // Replace with the desired user email
    public string saveFolder = "SavedModels"; // Replace with the desired save folder name
    public Text feedbackText;

    private void Start()
    {

    }

    public void EnableImporting()
    {
        Debug.Log("Importing requst");
        StartCoroutine(ImportModels());
    }

    private IEnumerator ImportModels()
    {
        string savedModelsPath = Path.Combine(Application.persistentDataPath, "SavedModels");
        if (Directory.Exists(savedModelsPath))
        {
            // Delete the folder and all its contents
            Directory.Delete(savedModelsPath, true);
            Debug.Log("Deleted existing SavedModels folder.");
        }

        // Create the "SavedModels" folder if it doesn't exist
        Directory.CreateDirectory(savedModelsPath);
        Debug.Log("Created SavedModels folder.");

        Debug.Log(apiUrl);

        string url = apiUrl + "/previews/" + userEmail;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            ModelData[] models = JsonHelper.FromJson<ModelData>(responseJson);


            foreach (ModelData model in models)
            {
                string modelFolder = Path.Combine(savedModelsPath, model.name);
                Directory.CreateDirectory(modelFolder);

                //StartCoroutine(DownloadAndSaveModel(apiUrl + model.obj, modelFolder));
                StartCoroutine(DownloadAndSaveImage(apiUrl + model.preview, modelFolder));
                //StartCoroutine(DownloadAndSaveMtl(apiUrl + model.mtl, modelFolder));

                /*foreach (TextureData texture in model.textures)
                {
                    string texturePath = apiUrl + "/files/" + userEmail + "/" + model.name + "/" + texture.filename;
                    StartCoroutine(DownloadAndSaveTexture(texturePath, modelFolder));
                }*/
            }

            yield return new WaitForSeconds(1);
            feedbackText.text = "Import Complete! Now Generate the models!";
        }
        else
        {
            Debug.LogError("Failed to fetch models: " + request.error);
        }
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
