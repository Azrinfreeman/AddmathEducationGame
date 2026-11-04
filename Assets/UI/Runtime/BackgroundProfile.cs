using UnityEngine;

namespace FunMath.UI
{
    [CreateAssetMenu(menuName = "Fun Math/Background Profile")]
    public sealed class BackgroundProfile : ScriptableObject
    {
        public enum MotionMode { Still, Explore, Focus }
        [Header("Scene character")]
        public MotionMode mode = MotionMode.Explore;
        public Color sky = new Color32(220, 244, 249, 255);
        public Color farHill = new Color32(172, 219, 180, 255);
        public Color nearHill = new Color32(100, 181, 153, 255);
        public Color cloud = Color.white;
        [Header("Motion in reference UI units")]
        [Tooltip("Clouds move gently around their authored position without crossing the reading region.")]
        [Range(0, 60)] public float cloudTravel = 32;
        [Range(18, 40)] public float cloudPeriod = 24;
        [Range(0, 5)] public float cornerSway = 2;
        [Range(4, 8)] public float swayPeriod = 6;
        [Range(.5f, 3)] public float discoveryCooldown = 1.2f;
    }
}
