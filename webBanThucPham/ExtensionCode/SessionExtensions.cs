using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace webBanThucPham.ExtensionCode
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            if (string.IsNullOrEmpty(value))
            {
                return typeof(T).IsValueType ? Activator.CreateInstance<T>() : default!;
            }

            return JsonConvert.DeserializeObject<T>(value)!;
        }

    }
}
