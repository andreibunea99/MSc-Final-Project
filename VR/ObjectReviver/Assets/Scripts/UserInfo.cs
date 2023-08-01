using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    UserResponse user;
    private string apiUrl = "http://192.168.1.217:5000";
    public GameObject authenticationComponent;
    public GameObject modelSelectionComponent;
    public GameObject followerSelectionComponent;
    public InputField emailInputField;
    public InputField passwordInputField;
    private float lastSubmitTime = 0f;
    private float submitDelay = 1f;
    public string user_email = null;
    public GameObject followerRowPrefab;
    public Transform followersPanel;
    public string currentFollower;
    public float rowSpacing = 0f; // Spacing between follower rows
    private GameObject canvasGameObject;
    private ModelPreviewsDisplay modelPreviewsDisplay;


    private void Start()
    {
        canvasGameObject = GameObject.Find("Canvas");
        modelPreviewsDisplay = canvasGameObject.GetComponent<ModelPreviewsDisplay>();
    }

    public void submit()
    {
        // Check if enough time has passed since the last submit
        Debug.Log(apiUrl);
        if (Time.time - lastSubmitTime >= submitDelay)
        {
            Debug.LogError(emailInputField.text);
            Debug.LogError(passwordInputField.text);

            StartCoroutine(LoginUser(emailInputField.text, passwordInputField.text));

            lastSubmitTime = Time.time; // Update the last submit time
        }
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
            user_email = email;
            modelPreviewsDisplay.changeEmail(email);
            // modelSelectionComponent.changeEmail(email);
            string responseJson = request.downloadHandler.text;
            UserResponse userData = JsonUtility.FromJson<UserResponse>(responseJson);
            Debug.Log("Login successful: " + userData.firstName + " " + userData.lastName);

            user = userData;

            // Hide the authentication component
            authenticationComponent.SetActive(false);

            // Activate the model selection component
            modelSelectionComponent.SetActive(true);

            StartCoroutine(GetFollowers(email));
        }
        else
        {
            // Login failed
            Debug.Log("Login failed: " + request.error);
        }
    }

    public string getEmail()
    {
        return user_email;
    }

    public void navigateToFollowers()
    {
        modelSelectionComponent.SetActive(false);
        followerSelectionComponent.SetActive(true);
    }

    public void navigateToModels()
    {
        followerSelectionComponent.SetActive(false);
        modelSelectionComponent.SetActive(true);
    }

    IEnumerator GetFollowers(string email)
    {
        string requestUrl = apiUrl + "/followers/" + email;
        UnityWebRequest request = UnityWebRequest.Get(requestUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            FollowersResponse followersData = JsonUtility.FromJson<FollowersResponse>(responseJson);

            // Clear existing follower rows
            ClearFollowerRows();

            // Calculate initial position for the first follower row
            float yPos = followersPanel.GetComponent<RectTransform>().rect.height / 2f - followerRowPrefab.GetComponent<RectTransform>().rect.height / 2f;


            foreach (FollowerData follower in followersData.followers)
            {
                // Instantiate the follower row prefab
                GameObject followerRow = Instantiate(followerRowPrefab, followersPanel);

                // Get the Text components of the instantiated row
                Text usernameText = followerRow.transform.Find("UsernameText").GetComponent<Text>();
                Text userEmailText = followerRow.transform.Find("UserEmailText").GetComponent<Text>();

                // Set the follower's data to the Text components
                usernameText.text = follower.firstName + " " + follower.lastName;
                userEmailText.text = follower.email;

                // Add click functionality to the follower row
                Button rowButton = followerRow.GetComponent<Button>();
                string followerEmail = follower.email; // Capture the email in a local variable
                rowButton.onClick.AddListener(() => OnFollowerClick(followerEmail));

                // Set the position of the follower row
                RectTransform rowRectTransform = followerRow.GetComponent<RectTransform>();
                Vector3 rowPosition = new Vector3(0f, yPos, 0f);
                rowRectTransform.anchoredPosition = rowPosition;

                // Update the y position for the next follower row
                yPos -= rowRectTransform.rect.height + rowSpacing;
            }
        }
        else
        {
            Debug.Log("Error getting followers: " + request.error);
        }
    }

    private void ClearFollowerRows()
    {
        foreach (Transform child in followersPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnFollowerClick(string email)
    {
        currentFollower = email;
        Debug.Log("Current Follower: " + currentFollower);
    }


    [System.Serializable]
    public class UserResponse
    {
        public string firstName;
        public string lastName;
        public string email;
    }

    [System.Serializable]
    public class FollowerData
    {
        public string firstName;
        public string lastName;
        public string email;
    }

    [System.Serializable]
    public class FollowersResponse
    {
        public List<FollowerData> followers;
    }
}
