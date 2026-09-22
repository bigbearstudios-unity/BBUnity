using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BBUnity.Extensions;

namespace Validation {
    public static class Validate {

        private static readonly Regex EmailRegex = new Regex(
            @"(^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$)",
            RegexOptions.Compiled);

        private static readonly Regex UrlRegex = new Regex(
            @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w\.-]*)*\/?$",
            RegexOptions.Compiled);

        private static readonly Regex IPv4FormatRegex = new Regex(
            @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",
            RegexOptions.Compiled);

        private static readonly Regex IPv4Regex = new Regex(
            @"^([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])$",
            RegexOptions.Compiled);

        private static readonly Regex AlphanumericRegex = new Regex(
            @"^[a-zA-Z0-9]+$",
            RegexOptions.Compiled);

        private static readonly Regex IPv6Regex = new Regex(
            @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$",
            RegexOptions.Compiled);

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="toValidate"/>
        /// matches a basic email pattern. Returns a failing result for null, empty, or
        /// whitespace-only strings.
        ///
        /// Known limitations: allows consecutive dots in the local part (e.g. test..@mail.com),
        /// does not support Unicode local parts, and does not enforce RFC 5321 length limits.
        /// Not suitable for security-sensitive validation.
        /// </summary>
        public static ValidationResult Email(string toValidate) {
            if (toValidate == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!EmailRegex.IsMatch(toValidate)) { return ValidationResult.Fail("Invalid email address format"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="toValidate"/>
        /// matches a permissive URL pattern. Accepts http://, https://, and scheme-less URLs
        /// (e.g. www.example.com, testsite.com). Returns a failing result for null, empty, or
        /// whitespace-only strings.
        ///
        /// Known limitations: this is a heuristic pattern and is not strict RFC 3986 compliance.
        /// Not suitable for security-sensitive validation.
        /// </summary>
        public static ValidationResult URL(string toValidate) {
            if (toValidate == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!UrlRegex.IsMatch(toValidate)) { return ValidationResult.Fail("Invalid URL format"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="toValidate"/>
        /// matches the dotted-decimal IPv4 format (e.g. 192.168.1.1). Does not validate that
        /// each octet is within the 0–255 range — a value like 999.999.999.999 will pass.
        /// Returns a failing result for null, empty, or whitespace-only strings.
        ///
        /// Use <see cref="IPv4"/> if you need full octet range validation.
        /// </summary>
        public static ValidationResult IPv4Format(string toValidate) {
            if (toValidate == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!IPv4FormatRegex.IsMatch(toValidate)) { return ValidationResult.Fail("Invalid IPv4 format"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="toValidate"/> is a
        /// valid IPv4 address with each octet constrained to 0–255 (e.g. 255.255.255.255 passes,
        /// 300.1.1.1 fails). Returns a failing result for null, empty, or whitespace-only strings.
        ///
        /// Use <see cref="IPv4Format"/> for a faster check that validates format only, without
        /// range validation.
        /// </summary>
        public static ValidationResult IPv4(string toValidate) {
            if (toValidate == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!IPv4Regex.IsMatch(toValidate)) { return ValidationResult.Fail("Invalid IPv4 address: each octet must be between 0 and 255"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="toValidate"/> is a
        /// valid IPv6 address. Handles full, compressed (::), and IPv4-mapped (::ffff:) notations.
        /// Returns a failing result for null, empty, or whitespace-only strings.
        /// </summary>
        public static ValidationResult IPv6(string toValidate) {
            if (toValidate == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!IPv6Regex.IsMatch(toValidate)) { return ValidationResult.Fail("Invalid IPv6 address"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the word count of
        /// <paramref name="str"/> is between <paramref name="minimum"/> and
        /// <paramref name="maximum"/> inclusive. Returns a failing result for null, empty,
        /// or whitespace-only strings.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static ValidationResult NumberOfWords(string str, int minimum, int maximum) {
            if (minimum > maximum) {
                throw new ArgumentException($"minimum ({minimum}) must not be greater than maximum ({maximum})");
            }
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }

            int numberOfWords = str.NumberOfWords();

            if (numberOfWords < minimum) {
                return ValidationResult.Fail($"Word count {numberOfWords} is below the minimum of {minimum}");
            }

            if (numberOfWords > maximum) {
                return ValidationResult.Fail($"Word count {numberOfWords} exceeds the maximum of {maximum}");
            }

            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the word count of
        /// <paramref name="str"/> is at least <paramref name="minimum"/>. Returns a failing
        /// result for null, empty, or whitespace-only strings.
        /// </summary>
        public static ValidationResult MinimumNumberOfWords(string str, int minimum) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            int numberOfWords = str.NumberOfWords();
            if (numberOfWords < minimum) {
                return ValidationResult.Fail($"Word count {numberOfWords} is below the minimum of {minimum}");
            }

            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the word count of
        /// <paramref name="str"/> does not exceed <paramref name="maximum"/>. Returns a failing
        /// result for null strings. An empty or whitespace-only string has a word count of zero
        /// and will pass for any non-negative maximum.
        /// </summary>
        public static ValidationResult MaximumNumberOfWords(string str, int maximum) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            int numberOfWords = str.NumberOfWords();
            if (numberOfWords > maximum) {
                return ValidationResult.Fail($"Word count {numberOfWords} exceeds the maximum of {maximum}");
            }

            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the character count of
        /// <paramref name="str"/> is between <paramref name="minimum"/> and
        /// <paramref name="maximum"/> inclusive. Returns a failing result for null or empty strings.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static ValidationResult NumberOfLetters(string str, int minimum, int maximum) {
            if (minimum > maximum) {
                throw new ArgumentException($"minimum ({minimum}) must not be greater than maximum ({maximum})");
            }
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }

            int numberOfLetters = str.Length;

            if (numberOfLetters < minimum) {
                return ValidationResult.Fail($"Character count {numberOfLetters} is below the minimum of {minimum}");
            }

            if (numberOfLetters > maximum) {
                return ValidationResult.Fail($"Character count {numberOfLetters} exceeds the maximum of {maximum}");
            }

            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the character count of
        /// <paramref name="str"/> is at least <paramref name="minimum"/>. Returns a failing
        /// result for null or empty strings.
        /// </summary>
        public static ValidationResult MinimumNumberOfLetters(string str, int minimum) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            int numberOfLetters = str.Length;

            if (numberOfLetters < minimum) {
                return ValidationResult.Fail($"Character count {numberOfLetters} is below the minimum of {minimum}");
            }

            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the character count of
        /// <paramref name="str"/> does not exceed <paramref name="maximum"/>. Returns a failing
        /// result for null strings. An empty string has a character count of zero and will pass
        /// for any non-negative maximum.
        /// </summary>
        public static ValidationResult MaximumNumberOfLetters(string str, int maximum) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            int numberOfLetters = str.Length;

            if (numberOfLetters > maximum) {
                return ValidationResult.Fail($"Character count {numberOfLetters} exceeds the maximum of {maximum}");
            }

            return ValidationResult.Valid;
        }

        // ── Null / empty guards ──────────────────────────────────────────────

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is not
        /// null. Works for any reference type including Unity objects.
        /// </summary>
        public static ValidationResult NotNull<T>(T value) where T : class {
            if (value == null) { return ValidationResult.Fail("Value cannot be null"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> is neither
        /// null nor an empty string. Whitespace-only strings pass; use
        /// <see cref="NotNullOrWhitespace"/> if you need to reject those too.
        /// </summary>
        public static ValidationResult NotNullOrEmpty(string str) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (str.Length == 0) { return ValidationResult.Fail("Value cannot be empty"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> is neither
        /// null, empty, nor contains only whitespace characters.
        /// </summary>
        public static ValidationResult NotNullOrWhitespace(string str) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (str.Trim().Length == 0) { return ValidationResult.Fail("Value cannot be empty or whitespace"); }
            return ValidationResult.Valid;
        }

        // ── Numeric range ────────────────────────────────────────────────────

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// between <paramref name="minimum"/> and <paramref name="maximum"/> inclusive.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static ValidationResult InRange(int value, int minimum, int maximum) {
            if (minimum > maximum) {
                throw new ArgumentException($"minimum ({minimum}) must not be greater than maximum ({maximum})");
            }
            if (value < minimum) { return ValidationResult.Fail($"Value {value} is below the minimum of {minimum}"); }
            if (value > maximum) { return ValidationResult.Fail($"Value {value} exceeds the maximum of {maximum}"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// between <paramref name="minimum"/> and <paramref name="maximum"/> inclusive.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static ValidationResult InRange(float value, float minimum, float maximum) {
            if (minimum > maximum) {
                throw new ArgumentException($"minimum ({minimum}) must not be greater than maximum ({maximum})");
            }
            if (value < minimum) { return ValidationResult.Fail($"Value {value} is below the minimum of {minimum}"); }
            if (value > maximum) { return ValidationResult.Fail($"Value {value} exceeds the maximum of {maximum}"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// greater than zero.
        /// </summary>
        public static ValidationResult Positive(int value) {
            if (value <= 0) { return ValidationResult.Fail($"Value {value} must be greater than zero"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// greater than zero.
        /// </summary>
        public static ValidationResult Positive(float value) {
            if (value <= 0f) { return ValidationResult.Fail($"Value {value} must be greater than zero"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// zero or greater.
        /// </summary>
        public static ValidationResult NonNegative(int value) {
            if (value < 0) { return ValidationResult.Fail($"Value {value} must be zero or greater"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="value"/> is
        /// zero or greater.
        /// </summary>
        public static ValidationResult NonNegative(float value) {
            if (value < 0f) { return ValidationResult.Fail($"Value {value} must be zero or greater"); }
            return ValidationResult.Valid;
        }

        // ── String content ───────────────────────────────────────────────────

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> matches
        /// the supplied compiled <paramref name="pattern"/>. Prefer this overload when the same
        /// pattern is reused across many calls — compile the <see cref="Regex"/> once and pass it
        /// in to avoid repeated construction costs.
        /// </summary>
        public static ValidationResult Matches(string str, Regex pattern) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!pattern.IsMatch(str)) { return ValidationResult.Fail("Value does not match the required pattern"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> matches
        /// the supplied <paramref name="pattern"/> string. A new <see cref="Regex"/> is
        /// constructed on each call; for hot paths or repeated use, compile the pattern yourself
        /// and use the <see cref="Matches(string, Regex)"/> overload instead.
        /// </summary>
        public static ValidationResult Matches(string str, string pattern) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!Regex.IsMatch(str, pattern)) { return ValidationResult.Fail("Value does not match the required pattern"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> contains
        /// only ASCII letters (A–Z, a–z) and digits (0–9). Returns a failing result for null,
        /// empty, or strings containing spaces, punctuation, or Unicode characters.
        /// </summary>
        public static ValidationResult AlphanumericOnly(string str) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!AlphanumericRegex.IsMatch(str)) { return ValidationResult.Fail("Value must contain only alphanumeric characters (A–Z, a–z, 0–9)"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> does not
        /// contain <paramref name="substring"/>. Returns a failing result for a null
        /// <paramref name="str"/>. A null or empty <paramref name="substring"/> will always
        /// cause a failing result because every string contains the empty string.
        /// </summary>
        public static ValidationResult DoesNotContain(string str, string substring) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (str.Contains(substring)) { return ValidationResult.Fail($"Value must not contain \"{substring}\""); }
            return ValidationResult.Valid;
        }

        // ── Numeric strings ──────────────────────────────────────────────────

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> can be
        /// parsed as a 32-bit integer. Returns a failing result for null or non-integer strings.
        /// </summary>
        public static ValidationResult IsInteger(string str) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!int.TryParse(str, out _)) { return ValidationResult.Fail("Value is not a valid integer"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="str"/> can be
        /// parsed as a single-precision floating-point number using the invariant culture.
        /// Returns a failing result for null or non-numeric strings.
        /// </summary>
        public static ValidationResult IsFloat(string str) {
            if (str == null) { return ValidationResult.Fail("Value cannot be null"); }
            if (!float.TryParse(str, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) {
                return ValidationResult.Fail("Value is not a valid floating-point number");
            }
            return ValidationResult.Valid;
        }

        // ── Collections ──────────────────────────────────────────────────────

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if <paramref name="collection"/> is
        /// not null and contains at least one element.
        /// </summary>
        public static ValidationResult NotEmpty<T>(ICollection<T> collection) {
            if (collection == null) { return ValidationResult.Fail("Collection cannot be null"); }
            if (collection.Count == 0) { return ValidationResult.Fail("Collection cannot be empty"); }
            return ValidationResult.Valid;
        }

        /// <summary>
        /// Returns a passing <see cref="ValidationResult"/> if the element count of
        /// <paramref name="collection"/> is between <paramref name="minimum"/> and
        /// <paramref name="maximum"/> inclusive. Returns a failing result for a null collection.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static ValidationResult CountInRange<T>(ICollection<T> collection, int minimum, int maximum) {
            if (minimum > maximum) {
                throw new ArgumentException($"minimum ({minimum}) must not be greater than maximum ({maximum})");
            }
            if (collection == null) { return ValidationResult.Fail("Collection cannot be null"); }
            int count = collection.Count;
            if (count < minimum) { return ValidationResult.Fail($"Collection count {count} is below the minimum of {minimum}"); }
            if (count > maximum) { return ValidationResult.Fail($"Collection count {count} exceeds the maximum of {maximum}"); }
            return ValidationResult.Valid;
        }

        // ── Composite ────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates all supplied <paramref name="results"/> and returns a single passing
        /// <see cref="ValidationResult"/> only if every result is valid. If one or more fail,
        /// their messages are joined with "; " into a single failing result, so all failures
        /// are visible at once rather than short-circuiting on the first.
        /// </summary>
        public static ValidationResult All(params ValidationResult[] results) {
            var failures = new List<string>();
            foreach (ValidationResult result in results) {
                if (!result.IsValid) { failures.Add(result.Message); }
            }
            if (failures.Count == 0) { return ValidationResult.Valid; }
            return ValidationResult.Fail(string.Join("; ", failures));
        }
    }
}
