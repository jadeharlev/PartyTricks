namespace Services {
    public interface IEconomyService {
        public void ApplyRewards(PlayerMinigameResult[] results);
        public int GetRewardForPlace(int place);
    }
}