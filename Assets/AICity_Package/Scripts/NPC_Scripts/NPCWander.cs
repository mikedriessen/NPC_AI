using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCWander : MonoBehaviour
{
    [Header("Wandering Settings")]
    public float closeWanderRadius = 10f;
    public float farWanderRadius = 30f;
    public float idleTimeMin = 2f;
    public float idleTimeMax = 5f;
    public float runThreshold = 6f;
    public float destinationTimeout = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private float idleTimer;
    private float destinationTimer;
    private bool isIdling = true;
    private bool isInteracting = false;
    private Transform currentPlayer;

    // Store the responses loaded from the JSON file
    private Dictionary<string, List<string>> fallbackResponses = new Dictionary<string, List<string>>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        StartIdle();

        // Load the responses from the JSON file
        LoadResponsesFromJson();
    }

    void Update()
    {
        if (isInteracting)
        {
            if (currentPlayer != null)
            {
                FacePlayer(currentPlayer);
            }
        }
        else
        {
            if (isIdling)
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    SetNewDestination();
                }
            }
            else
            {
                destinationTimer += Time.deltaTime;

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartIdle();
                }
                else if (destinationTimer > destinationTimeout)
                {
                    SetNewDestination();
                }
            }
        }

        UpdateAnimator();
    }

    private void SetNewDestination()
    {
        Vector3 randomPoint = GetRandomPoint();
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, farWanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isIdling = false;
            destinationTimer = 0f;
        }
        else
        {
            StartIdle();
        }
    }

    private Vector3 GetRandomPoint()
    {
        float radius = Random.value < 0.5f ? closeWanderRadius : farWanderRadius;
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;
        return randomDirection;
    }

    private void StartIdle()
    {
        isIdling = true;
        idleTimer = Random.Range(idleTimeMin, idleTimeMax);
        agent.ResetPath();
    }

    private void UpdateAnimator()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    public void StartInteraction(Transform playerTransform)
    {
        isInteracting = true;
        currentPlayer = playerTransform;
        agent.isStopped = true;
        animator.SetFloat("Speed", 0f);
    }

    public void EndInteraction()
    {
        isInteracting = false;
        currentPlayer = null;
        agent.isStopped = false;
        SetNewDestination();
    }

    public void FacePlayer(Transform playerTransform)
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // Load responses from the JSON file in Resources
    private void LoadResponsesFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("npc_responses"); // Load JSON from Resources folder
        if (jsonText != null)
        {
            // Deserialize the JSON file into the ResponseContainer
            ResponseContainer responses = JsonUtility.FromJson<ResponseContainer>(jsonText.ToString());

            // Convert the loaded response data into a dictionary
            if (responses != null && responses.responses != null)
            {
                foreach (var entry in responses.responses)
                {
                    fallbackResponses[entry.key.ToLower()] = entry.responses;
                }
            }
        }
        else
        {
            Debug.LogError("Could not load NPC responses from JSON file.");
        }
    }

    // Helper class for deserializing the JSON data
    [System.Serializable]
    private class ResponseContainer
    {
        public ResponseEntry[] responses;
    }

    [System.Serializable]
    private class ResponseEntry
    {
        public string key;
        public List<string> responses;
    }

    public string GetResponseToPlayerMessage(string playerMessage)
    {
        if (IsConnectedToInternet())
        {
            return "This is a response from the API.";
        }
        else
        {
            return GetFallbackResponse(playerMessage);
        }
    }

    private bool IsConnectedToInternet()
    {
        return false;
    }

    private string GetFallbackResponse(string playerMessage)
    {
        playerMessage = playerMessage.ToLower();

        foreach (var key in fallbackResponses.Keys)
        {
            if (playerMessage.Contains(key.ToLower()))
            {
                List<string> responses = fallbackResponses[key];
                return responses[Random.Range(0, responses.Count)];
            }
        }

        return "I’m not sure I understand that.";
    }
}
