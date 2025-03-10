using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class PlayerHand : NetworkBehaviour
{
    #region Variables
    [Header("Card Settings")]
    public GameObject cardPrefab; // Agora temos apenas um prefab de carta
    public int maxCards = 5;

    [Header("Fan Layout")]
    public bool useFanLayout = true;
    public float fanSpacing = 70f;
    public float fanSpacingMultiplier = 1.5f;
    public float fanSpreadMultiplier = 1.5f;
    public float maxRotation = 30f;

    [Header("Straight Layout (No Fan)")]
    public float flatSpacing = 100f;
    public float flatSpacingMultiplier = 1.5f;

    [Header("Highlight Effects")]
    public float hoverHeight = 20f;
    public float highlightedHeight = 40f;
    public float highlightedScale = 1.4f;
    public float draggedScale = 1.1f;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public float releaseAnimationDuration = 0.1f;

    [Header("Card Push")]
    public float pushDistance = 30f;
    public float pushDistanceMultiplier = 1f;

    [Header("Activation Panel")]
    public GameObject activationPanel;
    public float panelFadeInDuration = 0.3f;
    public float panelFadeOutDuration = 0.2f;
    private CanvasGroup activationPanelCanvasGroup;
    private RectTransform activationPanelRect;
    private bool isCardOverActivationArea = false;

    [Header("Panel Reference")]
    public RectTransform handPanel; // Referência ao painel onde as cartas serão renderizadas
    [SerializeField] private List<GameObject> cardsInHand = new List<GameObject>();
    private AttackTargetSelection attackTargetSelection;

    private GameObject draggedCard;
    private int draggedCardIndex;
    private bool isDragging = false;
    [HideInInspector] public bool isInitialized = false;

    private PlayerObjectController playerController;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        playerController = GetComponentInParent<PlayerObjectController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCardPositions();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && isLocalPlayer) // When the "Game" scene is loaded
        {
            handPanel.gameObject.SetActive(true);
            InitializeActivationPanel();

            attackTargetSelection = FindFirstObjectByType<AttackTargetSelection>();
            if (attackTargetSelection == null)
            {
                Debug.LogError("AttackTargetSelection não encontrado na cena!");
            }
        }
    }
    #endregion

    #region Hand Initialization and Management
    public void InitializeHand(List<int> cardIds)
    {
        if (isInitialized) return; // Evita inicialização múltipla

        ClearHand();

        foreach (int cardId in cardIds)
        {
            Card cardFromManager = CardManager.Instance.GetCardById(cardId);
            if (cardFromManager != null)
            {
                AddCardToHand(cardFromManager);
            }
            else
            {
                Debug.LogWarning($"Card with ID {cardId} not found in CardManager!");
            }
        }

        isInitialized = true;
    }

    private void ClearHand()
    {
        foreach (GameObject card in cardsInHand)
        {
            Destroy(card);
        }
        cardsInHand.Clear();
    }

    public void AddCardToHand(Card cardData)
    {
        if (cardsInHand.Count >= maxCards)
        {
            Debug.LogWarning("Mão cheia! Não é possível adicionar mais cartas.");
            return;
        }

        // Instancia a carta como filha do painel (handPanel)
        GameObject newCard = Instantiate(cardPrefab, handPanel);
        CardController cardController = newCard.GetComponent<CardController>();
        if (cardController != null)
        {
            cardController.SetCardData(cardData);
        }

        cardsInHand.Add(newCard);

        AddHoverEffect(newCard);
        AddDragEffect(newCard);

        UpdateCardPositions();
    }

    public void RemoveCardFromHand(GameObject card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            Destroy(card);
            UpdateCardPositions();
        }
    }

    public void RemoveCardFromHandByID(int cardID)
    {
        Debug.Log($"Carta com ID {cardID} foi executada com sucesso.");
        // Remove a carta da mão
        GameObject cardToRemove = cardsInHand.FirstOrDefault(card => CardController.GetCardIDFromGameObject(card) == cardID);
        if (cardToRemove != null)
        {
            RemoveCardFromHand(cardToRemove);
        }
        else
        {
            Debug.LogWarning($"Carta com ID {cardID} não encontrada na mão do jogador.");
        }
    }
    #endregion

    #region Card Positioning and Layout
    private void UpdateCardPositions(bool isHighlighting = false, int highlightedIndex = -1, bool isPushing = false)
    {
        int cardCount = cardsInHand.Count;
        float totalWidth;
        float startX;

        if (useFanLayout)
        {
            totalWidth = (cardCount - 1) * (fanSpacing * fanSpacingMultiplier);
            startX = -totalWidth / 2;
        }
        else
        {
            totalWidth = (cardCount - 1) * (flatSpacing * flatSpacingMultiplier);
            startX = -totalWidth / 2;
        }

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cardsInHand[i];
            RectTransform cardTransform = card.GetComponent<RectTransform>();

            if (card == draggedCard) continue;

            float xPos = startX + i * (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier);

            float rotation = 0f;
            if (useFanLayout && cardCount > 1)
            {
                rotation = Mathf.Lerp(maxRotation * fanSpreadMultiplier, -maxRotation * fanSpreadMultiplier, (float)i / (cardCount - 1));
            }

            Vector3 scale = Vector3.one;
            float yPos = 0f;

            // Handle highlighting effect
            if (isHighlighting && i == highlightedIndex)
            {
                scale = Vector3.one * highlightedScale;
                rotation = 0f;
                yPos = highlightedHeight;
            }

            // Handle pushing effect (when dragging or highlighting)
            if ((isHighlighting && i != highlightedIndex) || isPushing)
            {
                if (isHighlighting)
                {
                    // Push cards away from highlighted card
                    if (i < highlightedIndex)
                    {
                        xPos -= pushDistance * pushDistanceMultiplier;
                    }
                    else if (i > highlightedIndex)
                    {
                        xPos += pushDistance * pushDistanceMultiplier;
                    }
                }
                else if (isPushing)
                {
                    // Push cards adjacent to dragged card
                    if (i == draggedCardIndex - 1)
                    {
                        xPos -= pushDistance * pushDistanceMultiplier;
                    }
                    else if (i == draggedCardIndex + 1)
                    {
                        xPos += pushDistance * pushDistanceMultiplier;
                    }
                }
            }

            // Apply animations
            cardTransform.SetParent(handPanel);
            cardTransform.DOAnchorPos(new Vector2(xPos, yPos), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOLocalRotate(new Vector3(0, 0, rotation), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOScale(scale, animationDuration).SetEase(Ease.OutBack);

            cardTransform.SetSiblingIndex(i);
        }
    }
    #endregion

    #region Card Interaction Effects
    private void AddHoverEffect(GameObject card)
    {
        EventTrigger trigger = card.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = card.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnCardHoverEnter(card));
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => OnCardHoverExit(card));
        trigger.triggers.Add(entryExit);
    }

    private void AddDragEffect(GameObject card)
    {
        EventTrigger trigger = card.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = card.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryBeginDrag.callback.AddListener((data) => OnCardBeginDrag(card));
        trigger.triggers.Add(entryBeginDrag);

        EventTrigger.Entry entryDrag = new EventTrigger.Entry();
        entryDrag.eventID = EventTriggerType.Drag;
        entryDrag.callback.AddListener((data) => OnCardDrag(card));
        trigger.triggers.Add(entryDrag);

        EventTrigger.Entry entryEndDrag = new EventTrigger.Entry();
        entryEndDrag.eventID = EventTriggerType.EndDrag;
        entryEndDrag.callback.AddListener((data) => OnCardEndDrag(card));
        trigger.triggers.Add(entryEndDrag);
    }

    private void OnCardHoverEnter(GameObject card)
    {
        if (isDragging) return;

        int highlightedIndex = cardsInHand.IndexOf(card);
        if (highlightedIndex != -1)
        {
            UpdateCardPositions(true, highlightedIndex);
        }
    }

    private void OnCardHoverExit(GameObject card)
    {
        UpdateCardPositions(false);
    }

    private void OnCardBeginDrag(GameObject card)
    {
        draggedCard = card;
        draggedCardIndex = cardsInHand.IndexOf(card);
        isDragging = true;
        card.transform.SetAsLastSibling();

        ShowActivationPanel();

        RectTransform cardTransform = card.GetComponent<RectTransform>();
        cardTransform.DOScale(draggedScale, animationDuration).SetEase(Ease.OutBack);
    }

    private void OnCardDrag(GameObject card)
    {
        if (draggedCard != null)
        {
            RectTransform cardTransform = card.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handPanel, Input.mousePosition, null, out localPoint); // Usa handPanel em vez de transform
            cardTransform.anchoredPosition = localPoint;

            isCardOverActivationArea = IsCardOverActivationPanel(Input.mousePosition);
            UpdateActivationPanelFeedback(isCardOverActivationArea);

            if (!isCardOverActivationArea)
            {
                int newIndex = CalculateNewCardIndex(localPoint.x);
                if (newIndex != draggedCardIndex)
                {
                    cardsInHand.Remove(draggedCard);
                    cardsInHand.Insert(newIndex, draggedCard);
                    draggedCardIndex = newIndex;
                    UpdateCardPositions(false, -1, true);
                }
            }
        }
    }

    private void OnCardEndDrag(GameObject card)
    {
        if (draggedCard != null)
        {
            if (isCardOverActivationArea)
            {
                ActivateCard(draggedCard);
            }
            else
            {
                RectTransform cardTransform = draggedCard.GetComponent<RectTransform>();
                cardTransform.DOScale(Vector3.one, releaseAnimationDuration).SetEase(Ease.OutBack);
                UpdateCardPositions();
            }

            HideActivationPanel();

            draggedCard = null;
            isDragging = false;
            isCardOverActivationArea = false;
        }
    }
    #endregion

    #region Activation Panel Management
    private void InitializeActivationPanel()
    {
        if (activationPanel != null)
        {
            activationPanelCanvasGroup = activationPanel.GetComponent<CanvasGroup>();
            if (activationPanelCanvasGroup == null)
            {
                activationPanelCanvasGroup = activationPanel.AddComponent<CanvasGroup>();
            }

            activationPanelRect = activationPanel.GetComponent<RectTransform>();

            activationPanelCanvasGroup.alpha = 0f;
            activationPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Activation Panel not assigned in inspector!");
        }
    }

    private void ShowActivationPanel()
    {
        if (activationPanel != null)
        {
            activationPanel.SetActive(true);
            activationPanelCanvasGroup.DOFade(1f, panelFadeInDuration);
        }
    }

    private void HideActivationPanel()
    {
        if (activationPanel != null)
        {
            activationPanelCanvasGroup.DOFade(0f, panelFadeOutDuration).OnComplete(() =>
            {
                activationPanel.SetActive(false);
            });
        }
    }

    private bool IsCardOverActivationPanel(Vector2 mousePosition)
    {
        if (activationPanelRect == null) return false;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            activationPanelRect, mousePosition, null, out localPoint))
        {
            return activationPanelRect.rect.Contains(localPoint);
        }
        return false;
    }

    private void UpdateActivationPanelFeedback(bool isOver)
    {
        if (activationPanel != null)
        {
            activationPanel.transform.DOScale(
                isOver ? 1.1f : 1f,
                0.2f
            ).SetEase(Ease.OutBack);
        }
    }
    #endregion

    #region Card Activation
    private void ActivateCard(GameObject card)
    {
        if (playerController.isOurTurn == false)
        {
            Debug.LogWarning("Not the player's turn! Card cannot be activated.");
            return;
        }

        int cardID = CardController.GetCardIDFromGameObject(card);
        Card cardData = CardManager.Instance.GetCardById(cardID);

        if (cardData == null)
        {
            Debug.LogWarning($"Card with ID {cardID} not found in CardManager!");
            return;
        }

        // Verificar se a carta requer seleção de alvo
        if (cardData.requiresTargetSelection)
        {
            // Obter a lista de alvos disponíveis
            List<Essent> targets = GetTargetsForCard(cardData);
            if (targets.Count > 0)
            {
                // Iniciar a corrotina para esperar pela seleção do alvo
                StartCoroutine(WaitForTargetSelection(cardID, targets));
            }
            else
            {
                Debug.LogWarning("No valid targets found for this card.");
            }
        }
        else
        {
            // Aplicar o efeito da carta diretamente (sem seleção de alvo)
            GameManager.Instance.CmdExecuteCardEffectByID(cardID);
        }
    }

    private IEnumerator WaitForTargetSelection(int cardID, List<Essent> targets)
    {
        // Mostrar os alvos para o jogador
        attackTargetSelection.ShowTargets(targets);

        // Esperar até que um alvo seja selecionado
        bool isTargetSelected = false;
        attackTargetSelection.OnTargetSelectedEvent += target =>
        {
            playerController.GetSelectedEssentLocal().CmdSetSelectedTarget(target.essentID);
            isTargetSelected = true;
        };

        yield return new WaitUntil(() => isTargetSelected);

        // Agora que o alvo foi selecionado, aplicar o efeito da carta
        GameManager.Instance.CmdExecuteCardEffectByID(cardID);
    }
    private List<Essent> GetTargetsForCard(Card card)
    {
        List<Essent> targets = new();

        // Check if the card requires target selection
        if (card.requiresTargetSelection)
        {
            // Get the current player's Essent
            Essent currentEssent = playerController.GetSelectedEssentLocal();

            // Example: For a direct attack card, target all Essents except the player's
            targets = FindObjectsByType<Essent>(FindObjectsSortMode.None)
                .Where(e => e != currentEssent) // Exclude the player's Essent
                .ToList();

            // You can add more conditions here based on the card's logic
            // For example, if the card has a range limit, you can filter targets by distance
        }

        return targets;
    }
    #endregion

    #region Utility Functions
    private int CalculateNewCardIndex(float xPos)
    {
        int cardCount = cardsInHand.Count;
        float totalWidth = (cardCount - 1) * (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier);
        float startX = -totalWidth / 2;

        for (int i = 0; i < cardCount; i++)
        {
            float cardX = startX + i * (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier);
            if (xPos < cardX + (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier) / 2)
            {
                return i;
            }
        }

        return cardCount - 1;
    }
    #endregion
}