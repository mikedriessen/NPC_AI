using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

[System.Serializable]
public class Response
{
    public Candidate[] candidates;
}

public class ChatRequest
{
    public Content[] contents;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

[System.Serializable]
public class FallbackResponseWrapper
{
    public FallbackResponse[] responses;
}

[System.Serializable]
public class FallbackResponse
{
    public string key;
    public string[] responses;
}

public class UnityAndGeminiV3 : MonoBehaviour
{
    [Header("JSON API Configuration")]
    public TextAsset jsonApi;
    private string apiKey = "";
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent"; // Edit to the chosen model

    [Header("ChatBot Function")]
    public TMP_InputField inputField;
    public TMP_Text uiText;
    private Content[] chatHistory;

    [Header("Fallback Function")]
    private FallbackResponseWrapper fallbackResponseWrapper;

    [Header("Simulate No Internet")]
    public bool simulateNoInternet = false;  // Toggle to simulate no internet

    private void Start()
    {
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;
        chatHistory = new Content[] { };

        // Load fallback responses from Resources
        TextAsset fallbackJson = Resources.Load<TextAsset>("npc_responses");
        if (fallbackJson != null)
        {
            fallbackResponseWrapper = JsonUtility.FromJson<FallbackResponseWrapper>(fallbackJson.text);
        }
        else
        {
            Debug.LogError("Fallback JSON not found in Resources folder.");
        }
    }

    public void SendChat()
    {
        string userMessage = inputField.text;
        StartCoroutine(SendChatRequestToGemini(userMessage));
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        string url = $"{apiEndpoint}?key={apiKey}";

        // Create the message from the user
        Content userContent = new Content
        {
            role = "user",
            parts = new Part[] { new Part { text = newMessage } }
        };

        // Add the new user message to the chat history
        List<Content> contentsList = new List<Content>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray(); // Update chat history

        // Create the chat request object
        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

        // Convert to JSON format
        string jsonData = JsonUtility.ToJson(chatRequest);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create UnityWebRequest with POST method
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success || simulateNoInternet)
            {
                // Fallback response if there is no internet
                string fallbackResponse = GetFallbackResponse(newMessage);
                uiText.text = fallbackResponse;
            }
            else
            {
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;

                    // Limit the response to 500 characters
                    uiText.text = LimitResponseToMaxLength(reply, 500);
                }
                else
                {
                    // Fallback response if no valid response is received
                    string fallbackResponse = GetFallbackResponse(newMessage);
                    uiText.text = fallbackResponse;
                }
            }
        }
    }


    private string LimitResponseToMaxLength(string response, int maxLength)
    {
        if (response.Length > maxLength)
        {
            return response.Substring(0, maxLength);
        }
        return response;
    }

    private string EnsureResponseEndWithPeriod(string response)
    {
        if (response.Length > 0 && !response.EndsWith("."))
        {
            return response + ".";
        }
        return response;
    }

    private string GetFallbackResponse(string playerMessage)
    {
        if (fallbackResponseWrapper != null)
        {
            // Iterate through each fallback response set
            foreach (var responseGroup in fallbackResponseWrapper.responses)
            {
                // Check if any of the words in the player's message match the keys in the fallback JSON
                if (responseGroup.key.Split(' ').Any(key => playerMessage.ToLower().Contains(key.ToLower())))
                {
                    // Randomly select a fallback response that fits the 500 character limit
                    string selectedResponse = responseGroup.responses[Random.Range(0, responseGroup.responses.Length)];
                    return EnsureResponseEndWithPeriod(LimitResponseToMaxLength(selectedResponse, 500)); // Ensure fallback ends with a period
                }
            }
        }
        return "Sorry, I don't understand."; // Default fallback if no match
    }
}
