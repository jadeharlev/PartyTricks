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

        public readonly int CoinsPerGap;
        public readonly int VineScoreValue;
        public readonly float CoinArcHeight;

        public SwingConfig(float amplitude, float ropeLength, float period, float launchForce, float grabRadius,
            float fallThresholdY, float respawnDelay, float vineSpacing, float gravity, int coinsPerGap, int vineScoreValue, float coinArcHeight) {
            Amplitude = amplitude;
            RopeLength = ropeLength;
            Period = period;
            LaunchForce = launchForce;
            GrabRadius = grabRadius;
            FallThresholdY = fallThresholdY;
            RespawnDelay = respawnDelay;
            VineSpacing = vineSpacing;
            Gravity = gravity;
            CoinsPerGap = coinsPerGap;
            VineScoreValue = vineScoreValue;
            CoinArcHeight = coinArcHeight;
        }
    }
}