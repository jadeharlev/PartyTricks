using System;

namespace Services {
    public interface IPauseService {
        public bool IsPaused { get; }
        public void Pause();
        public void Resume();
        public void EnablePause();
        public void DisablePause();
        public void DoTimedPause(float lengthInSeconds, Action onComplete);
        public event Action OnPause;
        public event Action OnUnpause;
    }
}