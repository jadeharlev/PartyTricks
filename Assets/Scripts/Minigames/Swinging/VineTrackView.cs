using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineTrackView : MonoBehaviour {
        [SerializeField] private GameObject vineAnchorPrefab;

        public (float[] positions, float[] phaseOffsets, float[] periods) SpawnVines(int count, float spacing, float anchorY, SwingConfig config, float periodVariation) {
            float[] xPositions = new float[count];
            float[] phaseOffsets = new float[count];
            float[] periods = new float[count];
            
            for (int i = 0; i < count; i++) {
                xPositions[i] = i * spacing;
                phaseOffsets[i] = Random.Range(0f, 2f*Mathf.PI);
                periods[i] = config.Period * Random.Range(1f-periodVariation, 1f+periodVariation);
                
                var anchor = Instantiate(vineAnchorPrefab, transform);
                anchor.transform.localPosition = new Vector3(xPositions[i], anchorY);
                
                var vineView = anchor.GetComponent<VineView>();
                vineView.Initialize(config.Amplitude, config.RopeLength, periods[i], phaseOffsets[i]);
            }

            return (xPositions, phaseOffsets, periods);
        }
    }
}