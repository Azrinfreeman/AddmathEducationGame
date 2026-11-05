using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathFun.OnlineData
{
    /// <summary>
    /// Builds a JSON request body. Unity's JsonUtility cannot write "null",
    /// and the server needs real nulls, so this small helper does it by hand.
    /// Example: new JsonBody().Add("name", "Ali").Add("age", 7).ToString()
    /// </summary>
    public sealed class JsonBody
    {
        private readonly StringBuilder text = new StringBuilder("{");
        private bool hasFields;

        public JsonBody Add(string key, string value)
        {
            return AddRaw(key, value == null ? "null" : Quote(value));
        }

        public JsonBody Add(string key, int value)
        {
            return AddRaw(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public JsonBody Add(string key, bool value)
        {
            return AddRaw(key, value ? "true" : "false");
        }

        /// <summary>Adds a whole number, or null when the value is below zero ("not set").</summary>
        public JsonBody AddNullableNumber(string key, int value)
        {
            return value < 0 ? AddRaw(key, "null") : Add(key, value);
        }

        public JsonBody AddStrings(string key, IEnumerable<string> values)
        {
            StringBuilder list = new StringBuilder("[");
            bool first = true;
            foreach (string value in values)
            {
                if (!first) list.Append(',');
                list.Append(Quote(value));
                first = false;
            }
            list.Append(']');
            return AddRaw(key, list.ToString());
        }

        public override string ToString()
        {
            return text + "}";
        }

        /// <summary>Returns the body with one more integer field, e.g. expected_revision.</summary>
        public static string WithInt(string json, string key, int value)
        {
            string withoutBrace = json.Substring(0, json.Length - 1);
            return withoutBrace + ",\"" + key + "\":" + value.ToString(CultureInfo.InvariantCulture) + "}";
        }

        private JsonBody AddRaw(string key, string rawValue)
        {
            if (hasFields) text.Append(',');
            text.Append(Quote(key)).Append(':').Append(rawValue);
            hasFields = true;
            return this;
        }

        private static string Quote(string value)
        {
            StringBuilder quoted = new StringBuilder("\"");
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': quoted.Append("\\\""); break;
                    case '\\': quoted.Append("\\\\"); break;
                    case '\n': quoted.Append("\\n"); break;
                    case '\r': quoted.Append("\\r"); break;
                    case '\t': quoted.Append("\\t"); break;
                    default:
                        if (c < ' ') quoted.Append("\\u").Append(((int)c).ToString("x4"));
                        else quoted.Append(c);
                        break;
                }
            }
            return quoted.Append('"').ToString();
        }
    }
}
