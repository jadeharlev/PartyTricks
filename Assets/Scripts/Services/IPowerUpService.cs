using CoreData;

namespace Services {
    public interface IPowerUpService {
        GamblingModifiers GetGamblingModifiers(PlayerProfile playerProfile);
        MovementModifiers GetMovementModifiers(PlayerProfile playerProfile);
        CombatModifiers GetCombatModifiers(PlayerProfile playerProfile);
    }
}
