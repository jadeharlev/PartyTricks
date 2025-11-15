public class DireDodgingResultsState : IDireDodgingState {
    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Results State.", LogLevel.Verbose);
    }

    public void OnUpdate() {

    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Results State.", LogLevel.Verbose);
    }
}