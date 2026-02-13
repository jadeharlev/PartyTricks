using System;

namespace VineSwinging.Core {
    public static class GrabEvaluator {
        public static int CheckGrab(float playerX, float playerY, float[] vineXPositions, float vineAnchorY,
            SwingConfig config, int minVineIndex, float[] vinePhaseOffsets, float[] vinePeriods, float elapsedTime) {
            for (int i = minVineIndex; i < vineXPositions.Length; i++) {
                float vinePhase = vinePhaseOffsets[i] + (float)(2*Math.PI / vinePeriods[i]) * elapsedTime;

                int sampleCount = 3; // samples along the rope/vine to check 
                for (int s = 0; s < sampleCount; s++) {
                    float sampleFraction = (s + 1f) / sampleCount;
                    float sampleLength = config.RopeLength * sampleFraction;
                    var (offsetX, offsetY) =
                        SwingSimulation.GetSwingPosition(vinePhase, config.Amplitude, sampleLength);
                
                float sampleX = vineXPositions[i] + offsetX;
                float sampleY = vineAnchorY + offsetY;

                float dx = playerX - sampleX;
                float dy = playerY - sampleY;
                if (dx * dx + dy * dy <= config.GrabRadius * config.GrabRadius) {
                    return i;
                }
                }
            }

            return -1;
        }
    }
}