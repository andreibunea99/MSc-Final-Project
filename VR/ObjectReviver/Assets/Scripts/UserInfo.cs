using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    UserResponse user;
    private string apiUrl = "http://192.168.1.66:5000";
    public GameObject authenticationComponent;
    public GameObject modelSelectionComponent;
    public InputField emailInputField;
    public InputField passwordInputField;


    public void submit()
    {
        Debug.LogError(emailInputField.text);
        Debug.LogError(passwordInputField.text);

        StartCoroutine(LoginUser(emailInputField.text, passwordInputField.text));
    }

    IEnumerator LoginUser(string email, string password)
    {
        Debug.Log("Starting login...");
        // Create the request JSON data
        string requestData = "{\"email\": \"" + email + "\", \"password\": \"" + password + "\"}";

        // Create the request headers
        Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
        requestHeaders.Add("Content-Type", "application/json");

        Debug.Log(requestHeaders);
        // Create the login request
        UnityWebRequest request = UnityWebRequest.Post(apiUrl + "/login", requestData);
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestData));
        request.downloadHandler = new DownloadHandlerBuffer();
        foreach (KeyValuePair<string, string> header in requestHeaders)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }

        // Send the login request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Login successful
            string responseJson = request.downloadHandler.text;
            UserResponse userData = JsonUtility.FromJson<UserResponse>(responseJson);
            Debug.Log("Login successful: " + userData.firstName + " " + userData.lastName);

            user = userData;

            // Hide the authentication component
            authenticationComponent.SetActive(false);

            // Activate the model selection component
            modelSelectionComponent.SetActive(true);
        }
        else
        {
            // Login failed
            Debug.Log("Login failed: " + request.error);
        }
    }

    [System.Serializable]
    public class UserResponse
    {
        public string firstName;
        public string lastName;
        public string email;
    }
}
