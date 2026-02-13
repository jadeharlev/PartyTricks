using System;

namespace VineSwinging.Core {
    public static class GrabEvaluator {
        public static int CheckGrab(float playerX, float playerY, float[] vineXPositions, float vineAnchorY,
            SwingConfig config, int minVineIndex, float[] vinePhaseOffsets, float[] vinePeriods, float elapsedTime) {
            for (int i = minVineIndex; i < vineXPositions.Length; i++) {
                float vinePhase = vinePhaseOffsets[i]
                    + (float)(2*Math.PI / vinePeriods[i]) * elapsedTime;
                var (offsetX, offsetY) =
                    SwingSimulation.GetSwingPosition(vinePhase, config.Amplitude, config.RopeLength);
                
                float vineEndX = vineXPositions[i] + offsetX;
                float vineEndY = vineAnchorY + offsetY;
                
                float dx = playerX - vineEndX;
                float dy = playerY - vineEndY;
                if (dx * dx + dy * dy <= config.GrabRadius * config.GrabRadius) {
                    return i;
                }
            }

            return -1;
        }
    }
}