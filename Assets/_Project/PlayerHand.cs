using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PlayerHand : MonoBehaviour
{
    [Header("Card Settings")]
    public List<GameObject> cardPrefabs; // List of card prefabs
    public int maxCards = 5; // Maximum number of cards in hand

    [Header("Fan Layout")]
    public bool useFanLayout = true; // Enable/disable fan layout
    public float fanSpacing = 70f; // Spacing for fan layout
    public float fanSpacingMultiplier = 1.5f; // Multiplier for fanSpacing
    public float fanSpreadMultiplier = 1.5f; // Multiplier to expand the fan
    public float maxRotation = 30f; // Maximum rotation of cards

    [Header("Straight Layout (No Fan)")]
    public float flatSpacing = 100f; // Spacing for straight layout
    public float flatSpacingMultiplier = 1.5f; // Multiplier for flatSpacing

    [Header("Highlight Effects")]
    public float hoverHeight = 20f; // Height the card "jumps" when mouse hovers over
    public float highlightedHeight = 40f; // Height of highlighted card
    public float highlightedScale = 1.4f; // Scale of highlighted card
    public float draggedScale = 1.1f; // Scale of card when being dragged

    [Header("Animation Settings")]
    public float animationDuration = 0.3f; // Duration of animations
    public float releaseAnimationDuration = 0.1f; // Duration of animation when releasing the card

    [Header("Card Push")]
    public float pushDistance = 30f; // Distance cards are pushed to the side
    public float pushDistanceMultiplier = 1f; // Multiplier for pushDistance

    private List<GameObject> cardsInHand = new List<GameObject>();
    private GameObject draggedCard; // Card being dragged
    private int draggedCardIndex; // Index of the card being dragged
    private bool isDragging = false; // Tracks if a card is being dragged



    void Start()
    {
        // Adiciona cartas à mão no início, baseado na lista de prefabs
        InitializeHand();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCardPositions();
        }
    }

    // Inicializa a mão com as cartas da lista de prefabs
    private void InitializeHand()
    {
        // Limpa a mão atual (se houver cartas)
        ClearHand();

        // Adiciona cartas à mão, limitando ao número máximo de cartas
        for (int i = 0; i < Mathf.Min(cardPrefabs.Count, maxCards); i++)
        {
            AddCardToHand(cardPrefabs[i]);
        }
    }

    // Limpa todas as cartas da mão
    private void ClearHand()
    {
        foreach (GameObject card in cardsInHand)
        {
            Destroy(card);
        }
        cardsInHand.Clear();
    }

    // Adiciona uma carta à mão
    public void AddCardToHand(GameObject cardPrefab)
    {
        if (cardsInHand.Count >= maxCards)
        {
            Debug.LogWarning("Mão cheia! Não é possível adicionar mais cartas.");
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, transform);
        cardsInHand.Add(newCard);

        // Adiciona os eventos de mouse à carta
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

    private void UpdateCardPositions(bool isHighlighting = false, int highlightedIndex = -1)
    {
        int cardCount = cardsInHand.Count;
        float totalWidth;
        float startX;

        if (useFanLayout)
        {
            // Layout de leque
            totalWidth = (cardCount - 1) * (fanSpacing * fanSpacingMultiplier);
            startX = -totalWidth / 2;
        }
        else
        {
            // Layout reto (sem leque)
            totalWidth = (cardCount - 1) * (flatSpacing * flatSpacingMultiplier);
            startX = -totalWidth / 2;
        }

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cardsInHand[i];
            RectTransform cardTransform = card.GetComponent<RectTransform>();

            // Ignora a carta arrastada durante o reposicionamento
            if (card == draggedCard) continue;

            // Posição X
            float xPos = startX + i * (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier);

            // Rotação (invertida para o leque ficar de baixo para cima)
            float rotation = 0f;
            if (useFanLayout && cardCount > 1) // Aplica rotação apenas se o leque estiver ativado e houver mais de uma carta
            {
                rotation = Mathf.Lerp(maxRotation * fanSpreadMultiplier, -maxRotation * fanSpreadMultiplier, (float)i / (cardCount - 1));
            }

            // Escala da carta
            Vector3 scale = Vector3.one;

            // Altura da carta
            float yPos = 0f;

            // Se estiver destacando uma carta
            if (isHighlighting)
            {
                if (i == highlightedIndex)
                {
                    // Carta destacada: aumenta a escala, zera a rotação e levanta
                    scale = Vector3.one * highlightedScale;
                    rotation = 0f;
                    yPos = highlightedHeight;
                }
                else if (i < highlightedIndex)
                {
                    // Cartas à esquerda da destacada: empurra para a esquerda
                    xPos -= pushDistance * pushDistanceMultiplier;
                }
                else if (i > highlightedIndex)
                {
                    // Cartas à direita da destacada: empurra para a direita
                    xPos += pushDistance * pushDistanceMultiplier;
                }
            }

            // Usar DOTween para animar a posição, rotação e escala
            cardTransform.DOAnchorPos(new Vector2(xPos, yPos), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOLocalRotate(new Vector3(0, 0, rotation), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOScale(scale, animationDuration).SetEase(Ease.OutBack);

            // Ajuste de profundidade (opcional)
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

        // Evento de entrar com o mouse
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnCardHoverEnter(card));
        trigger.triggers.Add(entryEnter);

        // Evento de sair com o mouse
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

        // Evento de começar a arrastar
        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryBeginDrag.callback.AddListener((data) => OnCardBeginDrag(card));
        trigger.triggers.Add(entryBeginDrag);

        // Evento de arrastar
        EventTrigger.Entry entryDrag = new EventTrigger.Entry();
        entryDrag.eventID = EventTriggerType.Drag;
        entryDrag.callback.AddListener((data) => OnCardDrag(card));
        trigger.triggers.Add(entryDrag);

        // Evento de terminar de arrastar
        EventTrigger.Entry entryEndDrag = new EventTrigger.Entry();
        entryEndDrag.eventID = EventTriggerType.EndDrag;
        entryEndDrag.callback.AddListener((data) => OnCardEndDrag(card));
        trigger.triggers.Add(entryEndDrag);
    }

    private void OnCardHoverEnter(GameObject card)
    {
        // Ignora o efeito de destacar se uma carta estiver sendo arrastada
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
        isDragging = true; // Indica que uma carta está sendo arrastada
        card.transform.SetAsLastSibling(); // Coloca a carta arrastada por cima das outras

        // Ajusta a escala da carta ao segurar
        RectTransform cardTransform = card.GetComponent<RectTransform>();
        cardTransform.DOScale(draggedScale, animationDuration).SetEase(Ease.OutBack);
    }

    private void OnCardDrag(GameObject card)
    {
        if (draggedCard != null)
        {
            // Move a carta com o mouse
            RectTransform cardTransform = card.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, Input.mousePosition, null, out localPoint);
            cardTransform.anchoredPosition = localPoint;

            // Calcula a nova posição na lista em tempo real
            int newIndex = CalculateNewCardIndex(localPoint.x);
            if (newIndex != draggedCardIndex)
            {
                // Atualiza a lista e reorganiza as cartas
                cardsInHand.Remove(draggedCard);
                cardsInHand.Insert(newIndex, draggedCard);
                draggedCardIndex = newIndex;

                // Animação de empurrar as cartas ao lado
                UpdateCardPositionsWithPushAnimation();
            }
        }
    }

    private void OnCardEndDrag(GameObject card)
    {
        if (draggedCard != null)
        {
            // Retorna a escala da carta ao normal
            RectTransform cardTransform = draggedCard.GetComponent<RectTransform>();
            cardTransform.DOScale(Vector3.one, releaseAnimationDuration).SetEase(Ease.OutBack);

            // Atualiza as posições das cartas
            UpdateCardPositions();
            draggedCard = null;
            isDragging = false; // Indica que a carta não está mais sendo arrastada
        }
    }

    private void UpdateCardPositionsWithPushAnimation()
    {
        int cardCount = cardsInHand.Count;
        float totalWidth;
        float startX;

        if (useFanLayout)
        {
            // Layout de leque
            totalWidth = (cardCount - 1) * (fanSpacing * fanSpacingMultiplier);
            startX = -totalWidth / 2;
        }
        else
        {
            // Layout reto (sem leque)
            totalWidth = (cardCount - 1) * (flatSpacing * flatSpacingMultiplier);
            startX = -totalWidth / 2;
        }

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cardsInHand[i];
            RectTransform cardTransform = card.GetComponent<RectTransform>();

            // Ignora a carta arrastada durante o reposicionamento
            if (card == draggedCard) continue;

            // Posição X
            float xPos = startX + i * (useFanLayout ? fanSpacing * fanSpacingMultiplier : flatSpacing * flatSpacingMultiplier);

            // Rotação (invertida para o leque ficar de baixo para cima)
            float rotation = 0f;
            if (useFanLayout && cardCount > 1) // Aplica rotação apenas se o leque estiver ativado e houver mais de uma carta
            {
                rotation = Mathf.Lerp(maxRotation * fanSpreadMultiplier, -maxRotation * fanSpreadMultiplier, (float)i / (cardCount - 1));
            }

            // Escala da carta
            Vector3 scale = Vector3.one;

            // Altura da carta
            float yPos = 0f;

            // Verifica se a carta está ao lado da carta arrastada
            if (i == draggedCardIndex - 1 || i == draggedCardIndex + 1)
            {
                if (i < draggedCardIndex)
                {
                    // Empurra a carta à esquerda
                    xPos -= pushDistance * pushDistanceMultiplier;
                }
                else if (i > draggedCardIndex)
                {
                    // Empurra a carta à direita
                    xPos += pushDistance * pushDistanceMultiplier;
                }
            }

            // Usar DOTween para animar a posição, rotação e escala
            cardTransform.DOAnchorPos(new Vector2(xPos, yPos), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOLocalRotate(new Vector3(0, 0, rotation), animationDuration).SetEase(Ease.OutBack);
            cardTransform.DOScale(scale, animationDuration).SetEase(Ease.OutBack);

            // Ajuste de profundidade (opcional)
            cardTransform.SetSiblingIndex(i);
        }
    }

    // Calcula o novo índice da carta com base na posição X
    private int CalculateNewCardIndex(float xPos)
    {
        int cardCount = cardsInHand.Count;
        float totalWidth;
        float startX;

        if (useFanLayout)
        {
            // Layout de leque
            totalWidth = (cardCount - 1) * (fanSpacing * fanSpacingMultiplier);
            startX = -totalWidth / 2;
        }
        else
        {
            // Layout reto (sem leque)
            totalWidth = (cardCount - 1) * (flatSpacing * flatSpacingMultiplier);
            startX = -totalWidth / 2;
        }

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