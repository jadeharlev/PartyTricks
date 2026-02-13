using System;

namespace VineSwinging.Core {
    public class SwingSimulation {
        public static (float offsetX, float offsetY) GetSwingPosition(float phase, float amplitude, float ropeLength) {
            double theta = amplitude * Math.Sin(phase);
            float offsetX = ropeLength * (float)Math.Sin(theta);
            float offsetY = -ropeLength * (float)Math.Cos(theta);
            return (offsetX, offsetY);
        }
        
        public static (float vx, float vy) GetReleaseVelocity(float phase, float amplitude, float period, float launchForce, float ropeLength) {
            double theta = amplitude * Math.Sin(phase);
            double angularVelocity = amplitude * Math.Cos(phase) * (2*Math.PI/period);
            float vx = (float)(ropeLength * Math.Cos(theta) * angularVelocity);
            float vy = (float)(ropeLength * Math.Sin(theta) * angularVelocity);
            return (vx*launchForce, vy*launchForce);
        }
    }
}