using MongoDB.Bson;
public class UsersMatchBuilder
{
    public static string MatchBuilder(UserFilter filter)
    {
        var conditions = new List<string>();

        if (filter.Active.HasValue)
            conditions.Add($@"""activo"": {filter.Active.Value.ToString().ToLower()}");

        if (!string.IsNullOrEmpty(filter.Role))
            conditions.Add($@"""IDTipoUsuario._id"": ""{filter.Role}""");

        if (conditions.Count == 0)
            return "";

        return "{  \"$match\": { " + string.Join(", ", conditions) + " } }";
    }
}

