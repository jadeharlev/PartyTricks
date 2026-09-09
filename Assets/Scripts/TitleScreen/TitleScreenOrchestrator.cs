using System.Collections;
using Input.ControllerConnection;
using Services;
using UnityEngine;

namespace TitleScreen {
    public class TitleScreenOrchestrator : MonoBehaviour
    {
        [SerializeField] private SplashScreen SplashScreen;
        [SerializeField] private IntroDisplay IntroDisplay;
        [SerializeField] private MainMenu MainMenu;
        [SerializeField] private ControllerConnectionSystem ControllerConnectionSystem;
        [SerializeField] private bool ShowIntro;
        [SerializeField] private bool ShowSplashScreen;

        private IGameFlowService gameFlowService;
    
        private void Awake() {
            gameFlowService = ServiceLocatorAccessor.GetService<IGameFlowService>();
            SplashScreen.gameObject.SetActive(false);
            IntroDisplay.gameObject.SetActive(false);
            MainMenu.gameObject.SetActive(false);
            ControllerConnectionSystem.gameObject.SetActive(false);
        }

        private IEnumerator Start() {
            if(gameFlowService.ShouldShowSplashScreen() && ShowSplashScreen) {
                yield return RunPhase(SplashScreen);
                gameFlowService.MarkSplashScreenShown();
            }
        
            if (ShowIntro) yield return RunPhase(IntroDisplay);
        
            yield return RunPhase(MainMenu);
            yield return RunPhase(ControllerConnectionSystem);
            gameFlowService.StartGame();
        }

        private IEnumerator RunPhase(ITitleScreenPhase phase) {
            bool complete = false;
            void OnComplete() => complete = true;
            phase.OnPhaseComplete += OnComplete;
        
            ((MonoBehaviour)phase).gameObject.SetActive(true);
            yield return new WaitUntil(() => complete);
        
            phase.OnPhaseComplete -= OnComplete;
            ((MonoBehaviour)phase).gameObject.SetActive(false);
        }
    }
}
