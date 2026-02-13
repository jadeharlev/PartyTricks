namespace VineSwinging.Core {
    public readonly struct SwingConfig {
        public readonly float Amplitude;
        public readonly float RopeLength;
        public readonly float Period;
        public readonly float LaunchForce;
        public readonly float GrabRadius;
        public readonly float FallThresholdY;
        public readonly float RespawnDelay;
        public readonly float VineSpacing;
        public readonly float Gravity;

        public SwingConfig(float amplitude, float ropeLength, float period, float launchForce, float grabRadius,
            float fallThresholdY, float respawnDelay, float vineSpacing, float gravity) {
            Amplitude = amplitude;
            RopeLength = ropeLength;
            Period = period;
            LaunchForce = launchForce;
            GrabRadius = grabRadius;
            FallThresholdY = fallThresholdY;
            RespawnDelay = respawnDelay;
            VineSpacing = vineSpacing;
            Gravity = gravity;
        }
    }
}