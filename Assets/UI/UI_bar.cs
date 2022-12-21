using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_bar : MonoBehaviour {
    [SerializeField] private RectTransform rtBackground;
    [SerializeField] private RectTransform rtFill;
    [SerializeField] public float fillAmount;

    private void Update() {
        rtFill.sizeDelta = new Vector2(rtBackground.rect.width * fillAmount, rtBackground.rect.height);
    }
}
