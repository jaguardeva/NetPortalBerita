using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PortalBeritaApp.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // Remove all accents/diacritics
            string str = RemoveAccent(phrase).ToLowerInvariant();

            // Replace invalid characters with hyphens
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces/hyphens into single hyphen
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();

            // Cut to max 150 chars
            if (str.Length > 150)
                str = str.Substring(0, 150).Trim();

            // Replace spaces with hyphen
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        private static string RemoveAccent(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
