using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public LayerMask npcLayer; // Layer for detecting NPCs
    public Transform playerCamera; // Reference to player's camera
    public GameObject interactionUI; // Interaction UI with input and response fields
    public float interactionDistance = 5f; // Max distance for interaction

    private NPCWander currentNPC; // Current NPC being interacted with
    public bool isInteracting = false; // Make it public

    void Start()
    {
        // Set player camera if not assigned
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
            }
            else
            {
                Debug.LogError("Player camera not assigned or Main Camera not found.");
            }
        }

        // Ensure UI components are assigned
        if (interactionUI == null)
        {
            Debug.LogError("Interaction UI is missing.");
        }

        interactionUI?.SetActive(false); // Hide UI at start
    }

    void Update()
    {
        if (isInteracting)
        {
            HandleInteractionInput();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptInteraction();
        }
    }

    private void AttemptInteraction()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, npcLayer))
        {
            NPCWander npc = hit.collider.GetComponent<NPCWander>();
            if (npc != null)
            {
                StartInteraction(npc);
            }
        }
    }

    private void StartInteraction(NPCWander npc)
    {
        currentNPC = npc;
        isInteracting = true;
        currentNPC.StartInteraction(transform);

        interactionUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // When "Enter" is pressed, trigger the action of sending a message to the NPC or AI system
            UnityAndGeminiV3 unityAndGeminiScript = FindAnyObjectByType<UnityAndGeminiV3>();
            if (unityAndGeminiScript != null)
            {
                unityAndGeminiScript.SendChat(); // Send chat via UnityAndGeminiV3 script
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndInteraction();
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;
        currentNPC?.EndInteraction();

        interactionUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
