using System;

namespace VineSwinging.Core {
    public struct CoinPosition {
        public float RelativeXPosition; // 0.0 when releasing, 1.0 at next vine
        public float RelativeYPosition;
    }
    public class CoinTrailGenerator {
        public static CoinPosition[][] GenerateAllTrails(int vineCount, SwingConfig config, int seed) {
            float swingReach = config.RopeLength * (float)(Math.Sin(config.Amplitude));
            float safeMargin = 1.5f; // estimated value; player extents + coin radius
            float flightStartX = (swingReach + safeMargin) / config.VineSpacing;
            float flightEndX = 1f - flightStartX;
            float baseYPosition = -config.RopeLength * 0.5f;
            CoinPosition[][] trails = new CoinPosition[vineCount-1][];
            
            for (int i = 0; i < vineCount - 1; i++) {
                trails[i] = new CoinPosition[config.CoinsPerGap];
                for (int coinIndex = 0; coinIndex < config.CoinsPerGap; coinIndex++) {
                    float fractionAlongArc = (coinIndex + 1f) / (config.CoinsPerGap + 1f);
                    trails[i][coinIndex] = new CoinPosition
                    {
                        RelativeXPosition = flightStartX + fractionAlongArc * (flightEndX - flightStartX),
                        RelativeYPosition = baseYPosition + config.CoinArcHeight * 4f * fractionAlongArc * (1f - fractionAlongArc)
                    };
                }
            }
            return trails;
        }
    }
}