using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>
    /// Brings the rocket's existing flame sprite to life: a small idle flame that grows, flickers and
    /// leans away from the direction of travel while the rocket moves. Visual only; it reads the
    /// Rigidbody2D velocity and never writes to physics. Uses scaled time, so pausing freezes it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RocketThruster : MonoBehaviour
    {
        public Image flame;
        [Range(.2f, 1f)] public float idleSize = .55f;
        [Range(0, 25)] public float maxLeanDegrees = 12;
        [Tooltip("Horizontal speed (units per second) that counts as full throttle.")]
        public float fullSpeed = 30;

        private Rigidbody2D body;
        private Vector3 restScale = Vector3.one;
        private float throttle;
        private float lean;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (flame) restScale = flame.rectTransform.localScale;
        }

        private void OnEnable() { if (flame) flame.gameObject.SetActive(true); }

        private void OnDisable()
        {
            if (!flame) return;
            flame.rectTransform.localScale = restScale;
            flame.rectTransform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (!flame || !body) return;
            float vx = body.linearVelocity.x;
            float targetThrottle = Mathf.Clamp01(Mathf.Abs(vx) / Mathf.Max(1f, fullSpeed));
            float targetLean = Mathf.Clamp(-vx / Mathf.Max(1f, fullSpeed), -1f, 1f) * maxLeanDegrees;
            float follow = 1 - Mathf.Exp(-12f * Time.deltaTime);
            throttle = Mathf.Lerp(throttle, targetThrottle, follow);
            lean = Mathf.Lerp(lean, targetLean, follow);

            bool still = AdventureTheme.ReduceMotion;
            // Flicker is a fast noise on length; reduced motion keeps a steady flame.
            float flicker = still ? 0 : (Mathf.PerlinNoise(Time.time * 22f, 0) - .5f) * .25f;
            float size = Mathf.Lerp(idleSize, 1f, throttle) + flicker;
            flame.rectTransform.localScale = new Vector3(restScale.x * (1 - .15f * throttle), restScale.y * size, restScale.z);
            flame.rectTransform.localRotation = Quaternion.Euler(0, 0, still ? 0 : lean);
        }
    }
}
