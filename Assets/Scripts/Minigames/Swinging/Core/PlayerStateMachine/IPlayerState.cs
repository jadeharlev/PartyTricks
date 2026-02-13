namespace VineSwinging.Core
{
    public interface IPlayerState {
        void Enter(PlayerContext playerContext, SwingConfig swingConfig);
        void Update(PlayerContext playerContext, SwingConfig swingConfig, float deltaTime, bool releasePressed);
        void Exit(PlayerContext playerContext);
    }
}