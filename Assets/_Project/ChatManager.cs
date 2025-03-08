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
    public TMP_Text chatText;
    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public GameObject chatPanel;
    public GameObject chatPreviewPanel;
    public TMP_Text chatPreviewText;

    // Formatted text settings
    public float lineSpacing = 6f; // Line spacing for the chat text
    private readonly string systemName = "[Game]";
    private const string PlayerNameFormat = "<b><color=red>[{0}]</color></b>: {1}"; // {0} is the player name, {1} is the message
    private const string SystemMessageFormat = "<b><color=purple>[Game]</color></b> <color={0}>{1}</color>"; // {0} is the color code, {1} is the message
    private const string DefaultSystemMessageColor = "yellow"; // Default color for system messages

    private List<string> chatLog = new(); // List of chat messages
    private List<string> chatPreviewLog = new(); // List of messages in the preview panel
    private const int maxMessages = 50; // Maximum number of messages in the chat
    private const int maxPreviewMessages = 5; // Maximum number of messages in the preview panel

    private PlayerObjectController localPlayer;
    private Coroutine hideChatCoroutine;
    private float hideChatDelay = 3f; // Default time to hide the chat after inactivity

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
        chatText.text = "";
        SetLineSpacing(lineSpacing);
        chatPanel.SetActive(false); // Start with the chat hidden
        chatPreviewPanel.SetActive(false); // Start with the preview hidden

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
        chatPreviewPanel.SetActive(false);

        if (hideChatCoroutine != null)
        {
            StopCoroutine(hideChatCoroutine);
        }
        hideChatCoroutine = StartCoroutine(HideChatAfterDelay());
    }

    private void ShowChatPreview()
    {
        chatPreviewPanel.SetActive(true);
        chatPanel.SetActive(false);

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

        if (!inputField.isFocused && !IsPointerOverUIElement())
        {
            chatPanel.SetActive(false);
            chatPreviewPanel.SetActive(false);
        }
    }

    private IEnumerator HideChatAfterDelay()
    {
        yield return new WaitForSeconds(hideChatDelay);

        if (!inputField.isFocused && !IsPointerOverUIElement())
        {
            chatPanel.SetActive(false);
            chatPreviewPanel.SetActive(false);
            chatPreviewLog.Clear();
        }
    }
    #endregion

    #region Chat Message Handling
    public void SetPlayer(PlayerObjectController player)
    {
        localPlayer = player;
    }

    public new void SendMessage(string message)
    {
        if (localPlayer != null && localPlayer.isLocalPlayer)
        {
            localPlayer.CmdSendMessage(message);
        }
    }

    public void SendSystemMessage(string message, string color = DefaultSystemMessageColor)
    {
        if (isServer)
        {
            string formattedMessage = string.Format(SystemMessageFormat, color, message);

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

        chatLog.Add(formattedMessage);

        if (chatLog.Count > maxMessages)
        {
            chatLog.RemoveAt(0); // Remove the oldest message
        }

        UpdateChatText();

        StartCoroutine(ScrollToBottom());

        chatPreviewLog.Add(formattedMessage);

        if (chatPreviewLog.Count > maxPreviewMessages)
        {
            chatPreviewLog.RemoveAt(0);
        }

        UpdateChatPreviewText();

        if (!chatPanel.activeSelf)
        {
            ShowChatPreview();
        }
        else
        {
            ShowChat();
        }

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

    private void UpdateChatPreviewText()
    {
        chatPreviewText.text = string.Join("\n", chatPreviewLog);
    }
    #endregion

    #region Input Field Handling
    private void OnInputFieldEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (text.StartsWith("/"))
                {
                    // Se for um comando, processe-o
                    CommandHandler.Instance.ProcessCommand(text);
                }
                else
                {
                    // Se não for um comando, envie como mensagem de chat normal
                    SendMessage(text);
                }

                inputField.text = ""; // Limpa o campo de entrada

                // Refoca o InputField após enviar a mensagem
                inputField.ActivateInputField();

                // Reinicia o timer para esconder o chat
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

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();

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

    public void SetHideChatDelay(float delay)
    {
        hideChatDelay = delay;
    }

    private IEnumerator ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();

        // Adjust the scroll to the bottom
        scrollRect.verticalNormalizedPosition = 0f;

        yield return null;

        // Adjust the scroll again to ensure it's at the bottom
        scrollRect.verticalNormalizedPosition = 0f;
    }
    #endregion
}