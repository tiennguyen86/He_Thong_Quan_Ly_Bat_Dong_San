using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Helpers;

public static class SessionExtensions
{
    // Phép thuật Biến Đồ vật thành Chữ (JSON) để cất vào Session
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    // Phép thuật Biến Chữ (JSON) trở lại thành Đồ vật để lấy ra xài
    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}