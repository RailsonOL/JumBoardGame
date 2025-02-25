using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerHand : MonoBehaviour
{
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
    private List<GameObject> cardsInHand = new List<GameObject>();
    private GameObject draggedCard;
    private int draggedCardIndex;
    private bool isDragging = false;
    private bool isInitialized = false;

    private PlayerObjectController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerObjectController>();
        if (SceneManager.GetActiveScene().name == "game")
        {
            InitializeHand();
            InitializeActivationPanel();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCardPositions();
        }
    }

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

    public void InitializeHand()
    {
        if (isInitialized) return; // Evita inicialização múltipla

        ClearHand();

        if (playerController != null && playerController.SelectedEssent != null)
        {
            EssentData essentData = playerController.SelectedEssent.data;
            if (essentData != null)
            {
                foreach (Card card in essentData.initialCards)
                {
                    AddCardToHand(card);
                }
            }
        }

        InitializeActivationPanel(); // Configura o activationPanel
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

    private void ActivateCard(GameObject card)
    {
        Debug.Log($"Card activated: {card.name}");

        // Remove a carta da mão
        RemoveCardFromHand(card);

        // Obtém o componente da carta
        CardController cardController = card.GetComponent<CardController>();
        if (cardController != null)
        {
            // Obtém o cardData
            Card cardData = cardController.GetCardData();

            // Verifica se o cardData é um EffectCard
            if (cardData is Card effectCard)
            {
                // Envia o efeito da carta ao PlayerObjectController
                PlayerObjectController player = GetComponentInParent<PlayerObjectController>();
                if (player != null && player.SelectedEssent != null)
                {
                    effectCard.Execute(player.SelectedEssent); // Aplica o efeito ao ídolo do jogador
                }
            }
            else
            {
                Debug.LogWarning("A carta não é um EffectCard.");
            }
        }
        else
        {
            Debug.LogWarning("CardController não encontrado na carta.");
        }
    }

    // Calculate the new card index based on the X position
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
}