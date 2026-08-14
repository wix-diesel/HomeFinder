using System.Text.Json;
using System.Text.Json.Serialization;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Utils;

/// <summary>
/// 不正な ThemeMode 文字列を null に変換するカスタム JSON コンバーター。
/// 既知の値（"light"/"dark"）以外が指定された場合は null を返し、
/// モデルバインド後の検証で契約どおりのエラーを返せるようにする。
/// </summary>
public class ThemeModeJsonConverter : JsonConverter<ThemeMode?>
{
    public override ThemeMode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (Enum.TryParse<ThemeMode>(value, ignoreCase: true, out var result) &&
                Enum.IsDefined(typeof(ThemeMode), result))
            {
                return result;
            }

            // 不正値は null に落として、Required 検証でエラーを返す
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(ThemeMode), intValue))
            {
                return (ThemeMode)intValue;
            }

            return null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, ThemeMode? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(JsonNamingPolicy.CamelCase.ConvertName(value.Value.ToString()));
        }
    }
}
