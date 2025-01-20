# Unity AI Conversational Package

This Unity package provides an AI-driven conversational system for NPCs using the Google Gemini API. It allows NPCs to respond to player input with unique, context-aware answers, simulating dynamic interactions. If the API is unavailable, fallback responses are provided from a local JSON file. The system also includes an option to simulate no internet connectivity for testing.

## Instructions to Add Your Google Gemini API

### 1. Obtain your API key:
- Go to the [Google Gemini API documentation](https://cloud.google.com/gemini) and follow the steps to create a project and get an API key.

### 2. Add the API Key to the Package:
- Download your API Key file or copy the API key string.
- In the `Assets/AICity_Package/Scripts/AI_Scripts/GeminiManager`, add the API key in the `JSON_KEY_TEMPLATE.json` file.

### 3. Configure the API Endpoint (if needed):
- The default API endpoint is set to `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent`. If you are using a different version or endpoint, change it in the script.

### 4. Ensure Correct Dependencies:
- Ensure you have the necessary Unity packages, such as TextMeshPro, installed. If not, add them through the Unity Package Manager.

Now your package will be ready to communicate with Google Gemini, process responses, and handle player interactions with NPCs.
