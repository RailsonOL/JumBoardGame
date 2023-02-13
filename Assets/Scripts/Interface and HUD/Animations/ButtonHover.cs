using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //button animation scale on hover
    public float scale = 1.1f;
    public float speed = 10f;

    private Vector3 defaultScale;

    void Start() {
        defaultScale = transform.localScale;
    }

    //set default scale on re enable
    void OnDisable() {
        transform.localScale = defaultScale;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(AnimateButton(scale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(AnimateButton(1f));
    }

    IEnumerator AnimateButton(float targetScale)
    {
        Vector3 scale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.localScale = Vector3.Lerp(scale, defaultScale * targetScale, t);
            yield return 0;
        }
    }
}
