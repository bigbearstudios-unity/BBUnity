using System;
using NUnit.Framework;

using Validation;
using TestSupport;

public class ValidateTests {

    [Test]
    public void IPv4Format_ShouldPassOnValidIps() {
        UnityAssert.IsTrue(new string[] {
            "1.1.1.1",
            "255.255.255.255"
        }, s => Validate.IPv4Format(s));
    }

    [Test]
    public void IPv4Format_ShouldFailOnInvalidIPs() {
        UnityAssert.IsFalse(new string[] {
            "a.1.1.1",
            "a.b.c.d",
            "1-1-1-1"
        }, s => Validate.IPv4Format(s));
    }

    [Test]
    public void IPv4_ShouldPassOnValidIps() {
        UnityAssert.IsTrue(new string[] {
            "1.1.1.1",
            "255.255.255.255"
        }, s => Validate.IPv4(s));
    }

    [Test]
    public void IPv4_ShouldFailOnInvalidIPs() {
        UnityAssert.IsFalse(new string[] {
            "a.1.1.1",
            "a.b.c.d",
            "1-1-1-1",
            "125.125.125.300"
        }, s => Validate.IPv4(s));
    }

    [Test]
    public void IPv6_ShouldPassOnValidIps() {
        UnityAssert.IsTrue(new string[] {
            "2001:470:9b36:1::2",
            "2001:cdba:0000:0000:0000:0000:3257:9652",
            "2001:cdba:0:0:0:0:3257:9652",
            "2001:cdba::3257:9652"
        }, s => Validate.IPv6(s));
    }

    [Test]
    public void IPv6_ShouldFailOnInvalidIps() {
        UnityAssert.IsFalse(new string[] {
            "1200::AB00:1234::2552:7777:1313",
            "1200:0000:AB00:1234:O000:2552:7777:1313"
        }, s => Validate.IPv6(s));
    }

    [Test]
    public void Email_ShouldPassOnValidEmailAddress() {
        UnityAssert.IsTrue(new string[] {
            "test@mail.com"
        }, s => Validate.Email(s));
    }

    [Test]
    public void Email_ShouldFailOnInvalidEmailAddress() {
        UnityAssert.IsFalse(new string[] {
            "testmail.com",
            "@testmail.com",
            "test@mailcom"
        }, s => Validate.Email(s));
    }

    [Test]
    public void URL_ShouldPassOnValidURLs() {
        UnityAssert.IsTrue(new string[] {
            "https://testsite.com",
            "http://testsite.com",
            "www.testsite.com",
            "testsite.com",
            "test.de",
            "https://github.com/atestproject"
        }, s => Validate.URL(s));
    }

    [Test]
    public void URL_ShouldFailOnInvalidURLs() {
        UnityAssert.IsFalse(new string[] {
            "testmailcom",
            "http:testmailcom"
        }, s => Validate.URL(s));
    }

    [Test]
    public void NumberOfWords_ShouldPass_IfMinimumAndMaximumIsMet() {
        Assert.IsTrue(Validate.NumberOfWords("hello", 1, 1));
        Assert.IsTrue(Validate.NumberOfWords("hello motto", 2, 2));
        Assert.IsTrue(Validate.NumberOfWords("hello motto hello", 3, 3));
        Assert.IsTrue(Validate.NumberOfWords("hello motto hello motto", 4, 4));
    }

    [Test]
    public void NumberOfWords_ShouldFail_IfMinimumIsNotMet() {
        Assert.IsFalse(Validate.NumberOfWords("hello", 2, 3));
        Assert.IsFalse(Validate.NumberOfWords("hello motto", 3, 3));
    }

    [Test]
    public void NumberOfWords_ShouldFail_IfMaximumIsNotMet() {
        Assert.IsFalse(Validate.NumberOfWords("hello motto", 1, 1));
        Assert.IsFalse(Validate.NumberOfWords("hello motto yes please", 1, 3));
    }

    [Test]
    public void MinimumNumberOfWords_ShouldPass_IfMinimumIsMet() {
        Assert.IsTrue(Validate.MinimumNumberOfWords("one", 1));
        Assert.IsTrue(Validate.MinimumNumberOfWords("one two three", 2));
    }

    [Test]
    public void MaximumNumberOfWords_ShouldPass_IfMaximumIsMet() {
        Assert.IsTrue(Validate.MaximumNumberOfWords("one", 1));
        Assert.IsTrue(Validate.MaximumNumberOfWords("one two three", 3));
    }

    [Test]
    public void NumberOfLetters_ShouldPass_IfMinimumAndMaximumIsMet() {
        Assert.IsTrue(Validate.NumberOfLetters("hello", 1, 5));
    }

    /*
     * MaximumNumberOfWords — failure case (issue 12)
     */

    [Test]
    public void MaximumNumberOfWords_ShouldFail_IfMaximumIsExceeded() {
        Assert.IsFalse(Validate.MaximumNumberOfWords("one two three", 2));
        Assert.IsFalse(Validate.MaximumNumberOfWords("one two three four", 3));
    }

    /*
     * MinimumNumberOfWords — failure case
     */

    [Test]
    public void MinimumNumberOfWords_ShouldFail_IfMinimumIsNotMet() {
        Assert.IsFalse(Validate.MinimumNumberOfWords("one", 2));
        Assert.IsFalse(Validate.MinimumNumberOfWords("one two", 3));
    }

    /*
     * MinimumNumberOfLetters and MaximumNumberOfLetters (issue 6)
     */

    [Test]
    public void MinimumNumberOfLetters_ShouldPass_IfMinimumIsMet() {
        Assert.IsTrue(Validate.MinimumNumberOfLetters("hello", 5));
        Assert.IsTrue(Validate.MinimumNumberOfLetters("hello world", 3));
    }

    [Test]
    public void MinimumNumberOfLetters_ShouldFail_IfMinimumIsNotMet() {
        Assert.IsFalse(Validate.MinimumNumberOfLetters("hi", 5));
        Assert.IsFalse(Validate.MinimumNumberOfLetters("a", 3));
    }

    [Test]
    public void MaximumNumberOfLetters_ShouldPass_IfMaximumIsMet() {
        Assert.IsTrue(Validate.MaximumNumberOfLetters("hello", 5));
        Assert.IsTrue(Validate.MaximumNumberOfLetters("hi", 10));
    }

    [Test]
    public void MaximumNumberOfLetters_ShouldFail_IfMaximumIsExceeded() {
        Assert.IsFalse(Validate.MaximumNumberOfLetters("hello world", 5));
        Assert.IsFalse(Validate.MaximumNumberOfLetters("abcdef", 3));
    }

    /*
     * Null input (issue 9)
     */

    [Test]
    public void AllFormatMethods_ShouldReturnFalse_ForNull() {
        Assert.IsFalse(Validate.Email(null));
        Assert.IsFalse(Validate.URL(null));
        Assert.IsFalse(Validate.IPv4Format(null));
        Assert.IsFalse(Validate.IPv4(null));
        Assert.IsFalse(Validate.IPv6(null));
    }

    [Test]
    public void AllCountMethods_ShouldReturnFalse_ForNull() {
        Assert.IsFalse(Validate.NumberOfWords(null, 1, 5));
        Assert.IsFalse(Validate.MinimumNumberOfWords(null, 1));
        Assert.IsFalse(Validate.MaximumNumberOfWords(null, 5));
        Assert.IsFalse(Validate.NumberOfLetters(null, 1, 5));
        Assert.IsFalse(Validate.MinimumNumberOfLetters(null, 1));
        Assert.IsFalse(Validate.MaximumNumberOfLetters(null, 5));
    }

    /*
     * Empty and whitespace-only input (issue 9)
     */

    [Test]
    public void AllFormatMethods_ShouldReturnFalse_ForEmptyString() {
        Assert.IsFalse(Validate.Email(""));
        Assert.IsFalse(Validate.URL(""));
        Assert.IsFalse(Validate.IPv4Format(""));
        Assert.IsFalse(Validate.IPv4(""));
        Assert.IsFalse(Validate.IPv6(""));
    }

    [Test]
    public void AllFormatMethods_ShouldReturnFalse_ForWhitespaceOnlyString() {
        Assert.IsFalse(Validate.Email("   "));
        Assert.IsFalse(Validate.URL("   "));
        Assert.IsFalse(Validate.IPv4Format("   "));
        Assert.IsFalse(Validate.IPv4("   "));
        Assert.IsFalse(Validate.IPv6("   "));
    }

    /*
     * Inverted min/max range (issue 9)
     */

    [Test]
    public void NumberOfWords_ShouldThrow_WhenMinimumExceedsMaximum() {
        Assert.Throws<ArgumentException>(() => Validate.NumberOfWords("hello world", 5, 1));
    }

    [Test]
    public void NumberOfLetters_ShouldThrow_WhenMinimumExceedsMaximum() {
        Assert.Throws<ArgumentException>(() => Validate.NumberOfLetters("hello", 10, 3));
    }

    /*
     * ValidationResult
     */

    [Test]
    public void ValidationResult_ShouldBeValid_OnSuccess() {
        ValidationResult result = Validate.Email("test@mail.com");
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.Message);
    }

    [Test]
    public void ValidationResult_ShouldBeInvalid_WithMessage_OnFailure() {
        ValidationResult result = Validate.Email("notanemail");
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Message);
        Assert.IsNotEmpty(result.Message);
    }

    [Test]
    public void ValidationResult_ShouldCarryNullMessage_OnNullInput() {
        ValidationResult result = Validate.Email(null);
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Value cannot be null", result.Message);
    }

    [Test]
    public void ValidationResult_WordCount_ShouldDescribeFailureReason() {
        ValidationResult tooFew = Validate.MinimumNumberOfWords("one", 3);
        Assert.IsFalse(tooFew.IsValid);
        StringAssert.Contains("1", tooFew.Message);
        StringAssert.Contains("3", tooFew.Message);

        ValidationResult tooMany = Validate.MaximumNumberOfWords("one two three", 2);
        Assert.IsFalse(tooMany.IsValid);
        StringAssert.Contains("3", tooMany.Message);
        StringAssert.Contains("2", tooMany.Message);
    }

    [Test]
    public void ValidationResult_ImplicitBoolConversion_Works() {
        Assert.IsTrue(Validate.Email("test@mail.com"));
        Assert.IsFalse(Validate.Email("notanemail"));
    }

    /*
     * NotNull
     */

    [Test]
    public void NotNull_ShouldPass_ForNonNullReference() {
        Assert.IsTrue(Validate.NotNull("hello"));
        Assert.IsTrue(Validate.NotNull(new object()));
    }

    [Test]
    public void NotNull_ShouldFail_ForNull() {
        Assert.IsFalse(Validate.NotNull<string>(null));
        Assert.IsFalse(Validate.NotNull<object>(null));
    }

    /*
     * NotNullOrEmpty
     */

    [Test]
    public void NotNullOrEmpty_ShouldPass_ForNonEmptyString() {
        Assert.IsTrue(Validate.NotNullOrEmpty("hello"));
        Assert.IsTrue(Validate.NotNullOrEmpty("   "));
    }

    [Test]
    public void NotNullOrEmpty_ShouldFail_ForNullOrEmpty() {
        Assert.IsFalse(Validate.NotNullOrEmpty(null));
        Assert.IsFalse(Validate.NotNullOrEmpty(""));
    }

    /*
     * NotNullOrWhitespace
     */

    [Test]
    public void NotNullOrWhitespace_ShouldPass_ForNonWhitespaceString() {
        Assert.IsTrue(Validate.NotNullOrWhitespace("hello"));
        Assert.IsTrue(Validate.NotNullOrWhitespace("  x  "));
    }

    [Test]
    public void NotNullOrWhitespace_ShouldFail_ForNullEmptyOrWhitespace() {
        Assert.IsFalse(Validate.NotNullOrWhitespace(null));
        Assert.IsFalse(Validate.NotNullOrWhitespace(""));
        Assert.IsFalse(Validate.NotNullOrWhitespace("   "));
    }

    /*
     * InRange (int)
     */

    [Test]
    public void InRange_Int_ShouldPass_WhenValueIsWithinBounds() {
        Assert.IsTrue(Validate.InRange(5, 1, 10));
        Assert.IsTrue(Validate.InRange(1, 1, 10));
        Assert.IsTrue(Validate.InRange(10, 1, 10));
    }

    [Test]
    public void InRange_Int_ShouldFail_WhenValueIsOutsideBounds() {
        Assert.IsFalse(Validate.InRange(0, 1, 10));
        Assert.IsFalse(Validate.InRange(11, 1, 10));
    }

    [Test]
    public void InRange_Int_ShouldThrow_WhenMinimumExceedsMaximum() {
        Assert.Throws<ArgumentException>(() => Validate.InRange(5, 10, 1));
    }

    /*
     * InRange (float)
     */

    [Test]
    public void InRange_Float_ShouldPass_WhenValueIsWithinBounds() {
        Assert.IsTrue(Validate.InRange(0.5f, 0f, 1f));
        Assert.IsTrue(Validate.InRange(0f, 0f, 1f));
        Assert.IsTrue(Validate.InRange(1f, 0f, 1f));
    }

    [Test]
    public void InRange_Float_ShouldFail_WhenValueIsOutsideBounds() {
        Assert.IsFalse(Validate.InRange(-0.1f, 0f, 1f));
        Assert.IsFalse(Validate.InRange(1.1f, 0f, 1f));
    }

    [Test]
    public void InRange_Float_ShouldThrow_WhenMinimumExceedsMaximum() {
        Assert.Throws<ArgumentException>(() => Validate.InRange(0.5f, 1f, 0f));
    }

    /*
     * Positive
     */

    [Test]
    public void Positive_Int_ShouldPass_WhenValueIsAboveZero() {
        Assert.IsTrue(Validate.Positive(1));
        Assert.IsTrue(Validate.Positive(100));
    }

    [Test]
    public void Positive_Int_ShouldFail_WhenValueIsZeroOrBelow() {
        Assert.IsFalse(Validate.Positive(0));
        Assert.IsFalse(Validate.Positive(-1));
    }

    [Test]
    public void Positive_Float_ShouldPass_WhenValueIsAboveZero() {
        Assert.IsTrue(Validate.Positive(0.1f));
        Assert.IsTrue(Validate.Positive(100f));
    }

    [Test]
    public void Positive_Float_ShouldFail_WhenValueIsZeroOrBelow() {
        Assert.IsFalse(Validate.Positive(0f));
        Assert.IsFalse(Validate.Positive(-0.1f));
    }

    /*
     * NonNegative
     */

    [Test]
    public void NonNegative_Int_ShouldPass_WhenValueIsZeroOrAbove() {
        Assert.IsTrue(Validate.NonNegative(0));
        Assert.IsTrue(Validate.NonNegative(5));
    }

    [Test]
    public void NonNegative_Int_ShouldFail_WhenValueIsNegative() {
        Assert.IsFalse(Validate.NonNegative(-1));
        Assert.IsFalse(Validate.NonNegative(-100));
    }

    [Test]
    public void NonNegative_Float_ShouldPass_WhenValueIsZeroOrAbove() {
        Assert.IsTrue(Validate.NonNegative(0f));
        Assert.IsTrue(Validate.NonNegative(0.5f));
    }

    [Test]
    public void NonNegative_Float_ShouldFail_WhenValueIsNegative() {
        Assert.IsFalse(Validate.NonNegative(-0.1f));
        Assert.IsFalse(Validate.NonNegative(-100f));
    }

    /*
     * Matches (Regex overload)
     */

    [Test]
    public void Matches_Regex_ShouldPass_WhenStringMatchesPattern() {
        var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
        Assert.IsTrue(Validate.Matches("1234", pattern));
    }

    [Test]
    public void Matches_Regex_ShouldFail_WhenStringDoesNotMatchPattern() {
        var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
        Assert.IsFalse(Validate.Matches("abcd", pattern));
        Assert.IsFalse(Validate.Matches("123", pattern));
    }

    [Test]
    public void Matches_Regex_ShouldFail_ForNull() {
        var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
        Assert.IsFalse(Validate.Matches(null, pattern));
    }

    /*
     * Matches (string overload)
     */

    [Test]
    public void Matches_String_ShouldPass_WhenStringMatchesPattern() {
        Assert.IsTrue(Validate.Matches("hello123", @"^[a-z0-9]+$"));
    }

    [Test]
    public void Matches_String_ShouldFail_WhenStringDoesNotMatchPattern() {
        Assert.IsFalse(Validate.Matches("Hello!", @"^[a-z0-9]+$"));
    }

    [Test]
    public void Matches_String_ShouldFail_ForNull() {
        Assert.IsFalse(Validate.Matches(null, @"^[a-z]+$"));
    }

    /*
     * AlphanumericOnly
     */

    [Test]
    public void AlphanumericOnly_ShouldPass_ForAlphanumericStrings() {
        Assert.IsTrue(Validate.AlphanumericOnly("hello"));
        Assert.IsTrue(Validate.AlphanumericOnly("Hello123"));
        Assert.IsTrue(Validate.AlphanumericOnly("ABC"));
    }

    [Test]
    public void AlphanumericOnly_ShouldFail_WhenStringContainsNonAlphanumericCharacters() {
        Assert.IsFalse(Validate.AlphanumericOnly("hello world"));
        Assert.IsFalse(Validate.AlphanumericOnly("hello!"));
        Assert.IsFalse(Validate.AlphanumericOnly("café"));
        Assert.IsFalse(Validate.AlphanumericOnly(""));
    }

    [Test]
    public void AlphanumericOnly_ShouldFail_ForNull() {
        Assert.IsFalse(Validate.AlphanumericOnly(null));
    }

    /*
     * DoesNotContain
     */

    [Test]
    public void DoesNotContain_ShouldPass_WhenSubstringIsAbsent() {
        Assert.IsTrue(Validate.DoesNotContain("hello world", "foo"));
        Assert.IsTrue(Validate.DoesNotContain("username", "admin"));
    }

    [Test]
    public void DoesNotContain_ShouldFail_WhenSubstringIsPresent() {
        Assert.IsFalse(Validate.DoesNotContain("hello world", "world"));
        Assert.IsFalse(Validate.DoesNotContain("superadmin", "admin"));
    }

    [Test]
    public void DoesNotContain_ShouldFail_ForNullString() {
        Assert.IsFalse(Validate.DoesNotContain(null, "foo"));
    }

    /*
     * IsInteger
     */

    [Test]
    public void IsInteger_ShouldPass_ForValidIntegerStrings() {
        Assert.IsTrue(Validate.IsInteger("0"));
        Assert.IsTrue(Validate.IsInteger("42"));
        Assert.IsTrue(Validate.IsInteger("-10"));
    }

    [Test]
    public void IsInteger_ShouldFail_ForNonIntegerStrings() {
        Assert.IsFalse(Validate.IsInteger("3.14"));
        Assert.IsFalse(Validate.IsInteger("abc"));
        Assert.IsFalse(Validate.IsInteger(""));
    }

    [Test]
    public void IsInteger_ShouldFail_ForNull() {
        Assert.IsFalse(Validate.IsInteger(null));
    }

    /*
     * IsFloat
     */

    [Test]
    public void IsFloat_ShouldPass_ForValidFloatStrings() {
        Assert.IsTrue(Validate.IsFloat("3.14"));
        Assert.IsTrue(Validate.IsFloat("0"));
        Assert.IsTrue(Validate.IsFloat("-1.5"));
        Assert.IsTrue(Validate.IsFloat("1e3"));
    }

    [Test]
    public void IsFloat_ShouldFail_ForNonFloatStrings() {
        Assert.IsFalse(Validate.IsFloat("abc"));
        Assert.IsFalse(Validate.IsFloat(""));
    }

    [Test]
    public void IsFloat_ShouldFail_ForNull() {
        Assert.IsFalse(Validate.IsFloat(null));
    }

    /*
     * NotEmpty<T>
     */

    [Test]
    public void NotEmpty_ShouldPass_WhenCollectionHasElements() {
        Assert.IsTrue(Validate.NotEmpty(new System.Collections.Generic.List<int> { 1, 2 }));
        Assert.IsTrue(Validate.NotEmpty(new int[] { 0 }));
    }

    [Test]
    public void NotEmpty_ShouldFail_WhenCollectionIsEmpty() {
        Assert.IsFalse(Validate.NotEmpty(new System.Collections.Generic.List<int>()));
        Assert.IsFalse(Validate.NotEmpty(new string[] { }));
    }

    [Test]
    public void NotEmpty_ShouldFail_WhenCollectionIsNull() {
        Assert.IsFalse(Validate.NotEmpty<int>(null));
    }

    /*
     * CountInRange<T>
     */

    [Test]
    public void CountInRange_ShouldPass_WhenCountIsWithinBounds() {
        Assert.IsTrue(Validate.CountInRange(new int[] { 1, 2, 3 }, 1, 5));
        Assert.IsTrue(Validate.CountInRange(new int[] { 1 }, 1, 1));
    }

    [Test]
    public void CountInRange_ShouldFail_WhenCountIsOutsideBounds() {
        Assert.IsFalse(Validate.CountInRange(new int[] { }, 1, 3));
        Assert.IsFalse(Validate.CountInRange(new int[] { 1, 2, 3, 4 }, 1, 3));
    }

    [Test]
    public void CountInRange_ShouldFail_WhenCollectionIsNull() {
        Assert.IsFalse(Validate.CountInRange<int>(null, 1, 5));
    }

    [Test]
    public void CountInRange_ShouldThrow_WhenMinimumExceedsMaximum() {
        Assert.Throws<ArgumentException>(() => Validate.CountInRange(new int[] { 1, 2 }, 5, 1));
    }

    /*
     * All (composite)
     */

    [Test]
    public void All_ShouldPass_WhenAllResultsAreValid() {
        ValidationResult result = Validate.All(
            Validate.NotNullOrEmpty("hello"),
            Validate.Positive(5),
            Validate.AlphanumericOnly("hello")
        );
        Assert.IsTrue(result.IsValid);
    }

    [Test]
    public void All_ShouldFail_WhenAnyResultIsInvalid() {
        ValidationResult result = Validate.All(
            Validate.NotNullOrEmpty("hello"),
            Validate.Positive(-1),
            Validate.AlphanumericOnly("hello")
        );
        Assert.IsFalse(result.IsValid);
    }

    [Test]
    public void All_ShouldCollectAllFailureMessages() {
        ValidationResult result = Validate.All(
            Validate.Positive(-1),
            Validate.AlphanumericOnly("hello!"),
            Validate.NotNullOrEmpty("")
        );
        Assert.IsFalse(result.IsValid);
        StringAssert.Contains("greater than zero", result.Message);
        StringAssert.Contains("alphanumeric", result.Message);
        StringAssert.Contains("empty", result.Message);
    }

    /*
     * ValidationResult.And
     */

    [Test]
    public void And_ShouldReturnSecondResult_WhenFirstIsValid() {
        ValidationResult first = Validate.NotNullOrEmpty("hello");
        ValidationResult second = Validate.AlphanumericOnly("hello");
        Assert.IsTrue(first.And(second).IsValid);
    }

    [Test]
    public void And_ShouldReturnFirstResult_WhenFirstIsInvalid() {
        ValidationResult first = Validate.NotNullOrEmpty("");
        ValidationResult second = Validate.AlphanumericOnly("hello");
        ValidationResult chained = first.And(second);
        Assert.IsFalse(chained.IsValid);
        Assert.AreEqual(first.Message, chained.Message);
    }

    [Test]
    public void And_ShouldReturnSecondResult_WhenFirstIsValidAndSecondIsInvalid() {
        ValidationResult first = Validate.NotNullOrEmpty("hello!");
        ValidationResult second = Validate.AlphanumericOnly("hello!");
        ValidationResult chained = first.And(second);
        Assert.IsFalse(chained.IsValid);
        Assert.AreEqual(second.Message, chained.Message);
    }
}