using UnityEngine;

namespace MathFun.OnlineData
{
    /// <summary>
    /// The only place you change the server address.
    /// Create it with: Assets > Create > MathFun > API Configuration,
    /// and save it as Assets/OnlineData/Resources/OnlineData/ApiConfiguration.asset
    /// (the folder name matters: the game loads it from Resources).
    /// Never put passwords or database details here - this file is shipped inside the game.
    /// </summary>
    [CreateAssetMenu(menuName = "MathFun/API Configuration", fileName = "ApiConfiguration")]
    public class ApiConfiguration : ScriptableObject
    {
        public const string ResourcePath = "OnlineData/ApiConfiguration";

        [Tooltip("Full address of index.php on your server, e.g. https://example.com/api/index.php")]
        public string apiUrl = "https://example.com/api/index.php";

        [Tooltip("Turn off to play with local saves only (no network calls).")]
        public bool onlineEnabled = true;

        [Tooltip("Seconds to wait for the server before treating the request as offline.")]
        public int timeoutSeconds = 15;

        [Header("Limits (keep equal to Server/config/config.php)")]
        public int playerNameMaxLength = 40;
        public int playerAgeMin = 2;
        public int playerAgeMax = 18;
        public int passwordMinLength = 8;

        /// <summary>Builds e.g. https://example.com/api/index.php?route=/v1/players</summary>
        public string BuildUrl(string route)
        {
            return apiUrl + "?route=" + route;
        }

        /// <summary>Loads the asset, or returns a disabled in-memory copy so the game still runs.</summary>
        public static ApiConfiguration LoadOrDisabled()
        {
            ApiConfiguration found = Resources.Load<ApiConfiguration>(ResourcePath);
            if (found != null)
            {
                return found;
            }
            Debug.LogWarning("MathFun: no ApiConfiguration asset found; online features are off.");
            ApiConfiguration fallback = CreateInstance<ApiConfiguration>();
            fallback.onlineEnabled = false;
            return fallback;
        }
    }
}
