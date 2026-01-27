using System.Collections;
using UnityEngine;

namespace Input {
    public abstract class AIInputHandlerBase : MonoBehaviour, IDirectionalTwoButtonInputHandler {
        protected Vector2 currentNavigate;
        protected bool selectIsPressed;

        protected float minNavigationDuration = 0.3f;
        protected float maxNavigationDuration = 2f;
        protected float minIdleDuration = 0.2f;
        protected float maxIdleDuration = 1.5f;
        protected float selectionProbability = 0.1f;

        private void OnEnable() {
            StartCoroutine(AIRoutine());
        }

        private IEnumerator AIRoutine() {
            while (true) {
                currentNavigate = GetRandomNavigationVector();
                yield return new WaitForSeconds(Random.Range(minNavigationDuration, maxNavigationDuration));

                currentNavigate = Vector2.zero;
                yield return new WaitForSeconds(Random.Range(minIdleDuration, maxIdleDuration));

                if (Random.value < selectionProbability) {
                    selectIsPressed = true;
                    yield return null;
                    selectIsPressed = false;
                }
            }
        }

        protected abstract Vector2 GetRandomNavigationVector();

        public Vector2 GetNavigate() => currentNavigate;

        public bool SelectIsPressed() => selectIsPressed;

        public bool CancelIsPressed() => false;

        public bool IsActive() => true;
    }
}