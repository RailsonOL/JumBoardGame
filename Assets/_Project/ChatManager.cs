using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    [Header("UI Elements")]
    public TMP_Text chatText; // Chat text (using TextMeshPro)
    public TMP_InputField inputField; // Text input field (using TextMeshPro)
    public ScrollRect scrollRect; // For scrolling the chat (chatText is inside)
    public GameObject chatPanel; // Panel containing the Scroll View and InputField

    // Formatted text settings
    public float lineSpacing = 2f; // Line spacing for the chat text
    private readonly string systemName = "[Game]";
    private const string PlayerNameFormat = "<b><color=red>[{0}]</color></b>: {1}"; // {0} is the player name, {1} is the message
    private const string SystemMessageFormat = "<b><color=purple>[Game]</color></b> <color={0}>{1}</color>"; // {0} is the color code, {1} is the message
    private const string DefaultSystemMessageColor = "yellow"; // Default color for system messages

    private List<string> chatLog = new(); // List of chat messages
    private const int maxMessages = 50; // Maximum number of messages in the chat

    private PlayerObjectController localPlayer;
    private Coroutine hideChatCoroutine;
    private float hideChatDelay = 2f; // Default time to hide the chat after inactivity

    #region Initialization
    private void Awake()
    {
        // Set up the Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize the chat
        chatText.text = "";
        SetLineSpacing(lineSpacing);
        chatPanel.SetActive(false); // Start with the chat hidden

        // Set up the InputField events
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            inputField.onSelect.AddListener(OnInputFieldSelected);
        }

        // Set up the click event for the chat panel
        if (scrollRect != null)
        {
            scrollRect.gameObject.AddComponent<Button>().onClick.AddListener(OnChatClicked);
        }

        SendSystemMessage("Chat initialized.");
    }
    #endregion

    #region Chat Visibility Management
    private void Update()
    {
        // Check if the user clicked outside the InputField and ScrollRect
        if (Input.GetMouseButtonDown(0)) // 0 is the left mouse button
        {
            if (!IsPointerOverUIElement())
            {
                // If the click was outside the InputField and ScrollRect, hide the chat
                HideChat();
            }
        }
    }

    private void ShowChat()
    {
        chatPanel.SetActive(true);

        // Restart the timer to hide the chat
        if (hideChatCoroutine != null)
        {
            StopCoroutine(hideChatCoroutine);
        }
        hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
    }

    private void HideChat()
    {
        if (hideChatCoroutine != null)
        {
            StopCoroutine(hideChatCoroutine);
        }

        // Only hide the chat if the InputField is not focused AND the mouse is not over the chat
        if (!inputField.isFocused && !IsPointerOverUIElement())
        {
            chatPanel.SetActive(false);
        }
    }

    private IEnumerator HideChatAfterDelay()
    {
        yield return new WaitForSeconds(hideChatDelay);

        // Only hide the chat if the InputField is not focused AND the mouse is not over the chat
        if (!inputField.isFocused && !IsPointerOverUIElement())
        {
            chatPanel.SetActive(false);
        }
    }
    #endregion

    #region Chat Message Handling
    public void SetPlayer(PlayerObjectController player)
    {
        localPlayer = player;
        Debug.Log("Local player set in ChatManager.");
    }

    // Method to send a message (called by the player)
    public new void SendMessage(string message)
    {
        if (localPlayer != null && localPlayer.isLocalPlayer)
        {
            localPlayer.CmdSendMessage(message);
        }
    }

    // Method to send a system notification
    // Method to send a system notification
    public void SendSystemMessage(string message, string color = DefaultSystemMessageColor)
    {
        if (isServer)
        {
            // Format the system message with the specified color
            string formattedMessage = string.Format(SystemMessageFormat, color, message);

            // Send the formatted message to all clients
            RpcReceiveMessage(systemName, formattedMessage);
        }
    }

    // Method to receive a message (synchronized with all clients)
    [ClientRpc]
    public void RpcReceiveMessage(string playerName, string message)
    {
        string formattedMessage;

        if (playerName == systemName)
        {
            formattedMessage = message; // System messages are already formatted
        }
        else
        {
            formattedMessage = string.Format(PlayerNameFormat, playerName, message); // Format the player message
        }

        // Add the formatted message to the end of the list
        chatLog.Add(formattedMessage);

        // Limit the number of messages in the log
        if (chatLog.Count > maxMessages)
        {
            chatLog.RemoveAt(0); // Remove the oldest message
        }

        // Update the chat text
        UpdateChatText();

        // Scroll the chat to the most recent message (bottom)
        StartCoroutine(ScrollToBottom());

        // Show the chat when a message is received
        ShowChat();

        // Restart the timer to hide the chat
        if (hideChatCoroutine != null)
        {
            StopCoroutine(hideChatCoroutine);
        }
        hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
    }

    private void UpdateChatText()
    {
        chatText.text = string.Join("\n", chatLog);
    }
    #endregion

    #region Input Field Handling
    private void OnInputFieldEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(text))
            {
                SendMessage(text);
                inputField.text = ""; // Clear the input field

                // Refocus the InputField after sending the message
                inputField.ActivateInputField();

                // Restart the timer to hide the chat
                if (hideChatCoroutine != null)
                {
                    StopCoroutine(hideChatCoroutine);
                }
                hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
            }
        }
    }

    private void OnInputFieldSelected(string text)
    {
        ShowChat();
    }

    private void OnChatClicked()
    {
        ShowChat();
    }
    #endregion

    #region Utility Methods

    public void SetLineSpacing(float spacing)
    {
        if (chatText != null)
        {
            chatText.lineSpacing = spacing;
        }
    }

    // Check if the pointer is over a UI element
    private bool IsPointerOverUIElement()
    {
        // Create a pointer event for the current mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // List to store raycast results
        List<RaycastResult> results = new List<RaycastResult>();

        // Perform the raycast to check if the pointer is over a UI element
        EventSystem.current.RaycastAll(eventData, results);

        // Return true if the pointer is over the InputField or the Scroll Rect
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == inputField.gameObject || result.gameObject == scrollRect.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    // Set the chat hide delay
    public void SetHideChatDelay(float delay)
    {
        hideChatDelay = delay;
    }

    // Scroll the chat to the bottom
    private IEnumerator ScrollToBottom()
    {
        // Force layout update
        Canvas.ForceUpdateCanvases();

        // Adjust the scroll to the bottom
        scrollRect.verticalNormalizedPosition = 0f;

        // Wait for a frame to ensure the layout is updated
        yield return null;

        // Adjust the scroll again to ensure it's at the bottom
        scrollRect.verticalNormalizedPosition = 0f;
    }
    #endregion
}