using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class IntroDisplay : MonoBehaviour {
    [SerializeField]
    private UIDocument uiDocument;
    public GameObject MainMenuDocument;
    private Button continueButton;
    private void Awake() { 
        VisualElement root = uiDocument.rootVisualElement; 
        MainMenuDocument.SetActive(false); 
        continueButton = root.Query<Button>("ContinueButton");
        continueButton.clicked += OnContinueClick;
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void OnDestroy() {
        continueButton.clicked -= OnContinueClick;
    }

    public void OnContinueClick() {
        MainMenuDocument.SetActive(true);
        Destroy(gameObject);
    }
    
    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }
    
    private void FocusFirstButton() {
        if (continueButton != null) {
            continueButton.Focus();
        }
    }
}
