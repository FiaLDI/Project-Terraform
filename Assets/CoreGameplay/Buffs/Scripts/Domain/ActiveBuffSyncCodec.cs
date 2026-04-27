namespace Features.Buffs.Domain
{
    public static class ActiveBuffSyncCodec
    {
        private const char Separator = '|';

        public static string Encode(string buffId, int stacks)
        {
            return $"{buffId}{Separator}{stacks}";
        }

        public static bool TryDecode(string entry, out string buffId, out int stacks)
        {
            buffId = null;
            stacks = 0;

            if (string.IsNullOrEmpty(entry))
                return false;

            int separatorIndex = entry.LastIndexOf(Separator);
            if (separatorIndex <= 0 || separatorIndex >= entry.Length - 1)
                return false;

            buffId = entry.Substring(0, separatorIndex);
            return !string.IsNullOrEmpty(buffId) &&
                int.TryParse(entry.Substring(separatorIndex + 1), out stacks) &&
                stacks > 0;
        }
    }
}
