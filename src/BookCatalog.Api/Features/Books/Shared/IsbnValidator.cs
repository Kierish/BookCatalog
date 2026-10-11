namespace BookCatalog.Api.Features.Books.Shared
{
    public static class IsbnValidator
    {
        public static bool IsValid(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return true;

            var cleaned = isbn.Replace("-", "").Replace(" ", "");

            if (cleaned.Length == 10) return IsValidIsbn10(cleaned);
            if (cleaned.Length == 13) return IsValidIsbn13(cleaned);

            return false;
        }

        private static bool IsValidIsbn10(string isbn)
        {
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn[i])) return false;
                sum += (isbn[i] - '0') * (10 - i);
            }

            char last = char.ToUpperInvariant(isbn[9]);
            if (last == 'X') sum += 10;
            else if (char.IsDigit(last)) sum += last - '0';
            else return false;

            return sum % 11 == 0;
        }

        private static bool IsValidIsbn13(string isbn)
        {
            if (!isbn.StartsWith("978") && !isbn.StartsWith("979")) return false;

            int sum = 0;
            for (int i = 0; i < 13; i++)
            {
                if (!char.IsDigit(isbn[i])) return false;
                int digit = isbn[i] - '0';
                sum += i % 2 == 0 ? digit : digit * 3;
            }

            return sum % 10 == 0;
        }
    }
}
