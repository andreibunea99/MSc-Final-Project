using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class SpawnModels : MonoBehaviour
{
    public string apiUrl = "http://192.168.1.217:5000"; // Replace with your server URL

    public void SpawnModel(string modelName)
    {
        Debug.Log("AOLO VERE INTRU AICI FOR SOME REASON");
        GetModelData(modelName);
    }

    private IEnumerator GetModelData(string modelName)
    {
        Debug.Log("INTRU AICI");
        string email = "andreialexftw@gmail.com"; // Replace with the desired user email

        string url = apiUrl + "/model/" + modelName + "/" + email;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            ModelData modelData = JsonUtility.FromJson<ModelData>(responseJson);

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

    [System.Serializable]
    private class ModelData
    {
        public string obj;
        public string mtl;
        public TextureData[] textures;
    }

    [System.Serializable]
    private class TextureData
    {
        public string filename;
        public string path;
    }
}
