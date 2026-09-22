# Validation

A lightweight validation library for Unity. All methods return a `ValidationResult` which carries a success flag and, on failure, a human-readable message describing why validation failed. `ValidationResult` implicitly converts to `bool`, so existing call sites that treat the result as a boolean continue to work without changes.

```csharp
using Validation;

// Boolean usage
if (Validate.Email(input)) { ... }

// Rich usage — inspect the failure reason
ValidationResult result = Validate.Email(input);
if (!result.IsValid) {
    Debug.LogWarning(result.Message);
}
```

---

## Null / Empty Guards

### `Validate.NotNull<T>(T value)`
Passes if `value` is not null. Works for any reference type including Unity objects.

### `Validate.NotNullOrEmpty(string str)`
Passes if `str` is neither null nor an empty string. Whitespace-only strings pass; use `NotNullOrWhitespace` if you need to reject those too.

### `Validate.NotNullOrWhitespace(string str)`
Passes if `str` is neither null, empty, nor contains only whitespace.

---

## Numeric Range

### `Validate.InRange(int value, int minimum, int maximum)`
### `Validate.InRange(float value, float minimum, float maximum)`
Passes if `value` is between `minimum` and `maximum` inclusive. Throws `ArgumentException` if `minimum > maximum`.

### `Validate.Positive(int value)` / `Validate.Positive(float value)`
Passes if `value` is greater than zero.

### `Validate.NonNegative(int value)` / `Validate.NonNegative(float value)`
Passes if `value` is zero or greater.

---

## String Length

### `Validate.NumberOfLetters(string str, int minimum, int maximum)`
Passes if the character count of `str` is between `minimum` and `maximum` inclusive. Throws `ArgumentException` if `minimum > maximum`.

### `Validate.MinimumNumberOfLetters(string str, int minimum)`
Passes if the character count of `str` is at least `minimum`.

### `Validate.MaximumNumberOfLetters(string str, int maximum)`
Passes if the character count of `str` does not exceed `maximum`.

---

## Word Count

### `Validate.NumberOfWords(string str, int minimum, int maximum)`
Passes if the word count of `str` is between `minimum` and `maximum` inclusive. Throws `ArgumentException` if `minimum > maximum`.

### `Validate.MinimumNumberOfWords(string str, int minimum)`
Passes if the word count of `str` is at least `minimum`.

### `Validate.MaximumNumberOfWords(string str, int maximum)`
Passes if the word count of `str` does not exceed `maximum`.

---

## String Content

### `Validate.Matches(string str, Regex pattern)`
Passes if `str` matches the supplied compiled `Regex`. Prefer this overload when the same pattern is reused across many calls — compile the `Regex` once and pass it in to avoid repeated construction costs.

### `Validate.Matches(string str, string pattern)`
Passes if `str` matches the supplied pattern string. A new `Regex` is constructed on each call; for hot paths or repeated use, compile the pattern yourself and use the `Regex` overload instead.

### `Validate.AlphanumericOnly(string str)`
Passes if `str` contains only ASCII letters (A–Z, a–z) and digits (0–9). Fails for spaces, punctuation, and Unicode characters.

### `Validate.DoesNotContain(string str, string substring)`
Passes if `str` does not contain `substring`.

---

## Numeric Strings

### `Validate.IsInteger(string str)`
Passes if `str` can be parsed as a 32-bit integer.

### `Validate.IsFloat(string str)`
Passes if `str` can be parsed as a single-precision floating-point number (invariant culture).

---

## Format Validators

### `Validate.Email(string str)`
Passes if `str` matches a basic email pattern. Known limitations: allows consecutive dots in the local part, does not support Unicode local parts, and does not enforce RFC 5321 length limits. Not suitable for security-sensitive validation.

### `Validate.URL(string str)`
Passes if `str` matches a permissive URL pattern. Accepts `http://`, `https://`, and scheme-less URLs (e.g. `www.example.com`). Not strict RFC 3986 compliance.

### `Validate.IPv4Format(string str)`
Passes if `str` matches the dotted-decimal IPv4 format. Does not validate that each octet is within 0–255. Use `IPv4` for full range validation.

### `Validate.IPv4(string str)`
Passes if `str` is a valid IPv4 address with each octet constrained to 0–255.

### `Validate.IPv6(string str)`
Passes if `str` is a valid IPv6 address. Handles full, compressed (`::`) and IPv4-mapped (`::ffff:`) notations.

---

## Collections

### `Validate.NotEmpty<T>(ICollection<T> collection)`
Passes if `collection` is not null and contains at least one element.

### `Validate.CountInRange<T>(ICollection<T> collection, int minimum, int maximum)`
Passes if the element count of `collection` is between `minimum` and `maximum` inclusive. Throws `ArgumentException` if `minimum > maximum`.

---

## Composite Validation

### `Validate.All(params ValidationResult[] results)`
Evaluates all supplied results and returns a single passing `ValidationResult` only if every result is valid. If one or more fail, their messages are joined with `"; "` into a single failing result — all failures are visible at once rather than short-circuiting on the first.

```csharp
ValidationResult result = Validate.All(
    Validate.NotNullOrEmpty(username),
    Validate.AlphanumericOnly(username),
    Validate.MinimumNumberOfLetters(username, 3)
);
if (!result.IsValid) {
    Debug.LogWarning(result.Message);
}
```

### `ValidationResult.And(ValidationResult other)`
Returns `this` if it is invalid; otherwise returns `other`. Useful for chaining checks where the second only makes sense when the first passed.

```csharp
ValidationResult result = Validate.NotNullOrEmpty(username)
    .And(Validate.AlphanumericOnly(username));
```
