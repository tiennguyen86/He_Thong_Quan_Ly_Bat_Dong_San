using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Helpers;

/// <summary>
/// Extension method cho ISession để hỗ trợ serialization/deserialization
/// Cho phép lưu các đối tượng phức tạp vào Session dưới dạng JSON
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Lưu một đối tượng vào Session dưới dạng JSON string
    /// Giúp lưu các object phức tạp (list, class) vào Session
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của object cần lưu</typeparam>
    /// <param name="session">ISession object từ HttpContext</param>
    /// <param name="key">Tên key để tìm dữ liệu trong Session</param>
    /// <param name="value">Giá trị object cần lưu</param>
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    /// <summary>
    /// Lấy một object từ Session dựa vào JSON string
    /// Giúp khôi phục các object phức tạp từ Session
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu cần lấy</typeparam>
    /// <param name="session">ISession object từ HttpContext</param>
    /// <param name="key">Tên key để tìm dữ liệu trong Session</param>
    /// <returns>Object đã được deserialize, hoặc null/default nếu không tìm thấy</returns>
    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}