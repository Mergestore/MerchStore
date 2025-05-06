using System.Text;
using System.Text.Json;

namespace MerchStore.WebUI.Infrastructure;

// Den här klassen säger hur egenskaper ska döpas om till snake_case
public class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            // Om en bokstav är versal (stor bokstav), lägg till "_"
            if (i > 0 && char.IsUpper(name[i]))
            {
                builder.Append('_');
            }

            // Gör bokstaven till liten och lägg till i resultatet
            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }
}
