namespace Features.Multiplayer.SceneBinding
{
    public static class SceneBoundKey
    {
        public static string Make(string type, string id)
        {
            type = type?.Trim();
            id = id?.Trim();

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id))
                return string.Empty;

            return $"{type}::{id}";
        }
    }
}
