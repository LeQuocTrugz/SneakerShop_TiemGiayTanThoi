using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Web_StoreGiay.Extensions
{
    public static class SessionExtensions
    {
        // Phương thức mở rộng để lưu đối tượng vào Session dưới dạng JSON
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonConvert.SerializeObject(value);  // Chuyển đối tượng thành JSON
            session.SetString(key, json);  // Lưu JSON vào Session
        }

        // Phương thức mở rộng để lấy đối tượng từ Session dưới dạng JSON
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);  // Lấy giá trị JSON từ Session
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);  // Chuyển JSON về đối tượng
        }
    }
}
