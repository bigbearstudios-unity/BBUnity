using System;
using NUnit.Framework;

using BBUnity.Support.Validation;
using BBUnity.Support.Testing;

namespace BBUnity.Support.Validation.Tests {

    public class ValidatorTests {

        [Test]
        public void IPv4Format_ShouldPassOnValidIps() {
            UnityAssert.IsTrue(new string[] {
                "1.1.1.1",
                "255.255.255.255"
            }, s => Validator.IPv4Format(s));
        }

        [Test]
        public void IPv4Format_ShouldFailOnInvalidIPs() {
            UnityAssert.IsFalse(new string[] {
                "a.1.1.1",
                "a.b.c.d",
                "1-1-1-1"
            }, s => Validator.IPv4Format(s));
        }

        [Test]
        public void IPv4_ShouldPassOnValidIps() {
            UnityAssert.IsTrue(new string[] {
                "1.1.1.1",
                "255.255.255.255"
            }, s => Validator.IPv4(s));
        }

        [Test]
        public void IPv4_ShouldFailOnInvalidIPs() {
            UnityAssert.IsFalse(new string[] {
                "a.1.1.1",
                "a.b.c.d",
                "1-1-1-1",
                "125.125.125.300"
            }, s => Validator.IPv4(s));
        }

        [Test]
        public void IPv6_ShouldPassOnValidIps() {
            UnityAssert.IsTrue(new string[] {
                "2001:470:9b36:1::2",
                "2001:cdba:0000:0000:0000:0000:3257:9652",
                "2001:cdba:0:0:0:0:3257:9652",
                "2001:cdba::3257:9652"
            }, s => Validator.IPv6(s));
        }

        [Test]
        public void IPv6_ShouldFailOnInvalidIps() {
            UnityAssert.IsFalse(new string[] {
                "1200::AB00:1234::2552:7777:1313",
                "1200:0000:AB00:1234:O000:2552:7777:1313"
            }, s => Validator.IPv6(s));
        }

        [Test]
        public void Email_ShouldPassOnValidEmailAddress() {
            UnityAssert.IsTrue(new string[] {
                "test@mail.com"
            }, s => Validator.Email(s));
        }

        [Test]
        public void Email_ShouldFailOnInvalidEmailAddress() {
            UnityAssert.IsFalse(new string[] {
                "testmail.com",
                "@testmail.com",
                "test@mailcom"
            }, s => Validator.Email(s));
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
            }, s => Validator.URL(s));
        }

        [Test]
        public void URL_ShouldFailOnInvalidURLs() {
            UnityAssert.IsFalse(new string[] {
                "testmailcom",
                "http:testmailcom"
            }, s => Validator.URL(s));
        }

        [Test]
        public void NumberOfWords_ShouldPass_IfMinimumAndMaximumIsMet() {
            Assert.IsTrue(Validator.NumberOfWords("hello", 1, 1));
            Assert.IsTrue(Validator.NumberOfWords("hello motto", 2, 2));
            Assert.IsTrue(Validator.NumberOfWords("hello motto hello", 3, 3));
            Assert.IsTrue(Validator.NumberOfWords("hello motto hello motto", 4, 4));
        }

        [Test]
        public void NumberOfWords_ShouldFail_IfMinimumIsNotMet() {
            Assert.IsFalse(Validator.NumberOfWords("hello", 2, 3));
            Assert.IsFalse(Validator.NumberOfWords("hello motto", 3, 3));
        }

        [Test]
        public void NumberOfWords_ShouldFail_IfMaximumIsNotMet() {
            Assert.IsFalse(Validator.NumberOfWords("hello motto", 1, 1));
            Assert.IsFalse(Validator.NumberOfWords("hello motto yes please", 1, 3));
        }

        [Test]
        public void MinimumNumberOfWords_ShouldPass_IfMinimumIsMet() {
            Assert.IsTrue(Validator.MinimumNumberOfWords("one", 1));
            Assert.IsTrue(Validator.MinimumNumberOfWords("one two three", 2));
        }

        [Test]
        public void MaximumNumberOfWords_ShouldPass_IfMaximumIsMet() {
            Assert.IsTrue(Validator.MaximumNumberOfWords("one", 1));
            Assert.IsTrue(Validator.MaximumNumberOfWords("one two three", 3));
        }

        [Test]
        public void NumberOfLetters_ShouldPass_IfMinimumAndMaximumIsMet() {
            Assert.IsTrue(Validator.NumberOfLetters("hello", 1, 5));
        }

        /*
         * MaximumNumberOfWords — failure case (issue 12)
         */

        [Test]
        public void MaximumNumberOfWords_ShouldFail_IfMaximumIsExceeded() {
            Assert.IsFalse(Validator.MaximumNumberOfWords("one two three", 2));
            Assert.IsFalse(Validator.MaximumNumberOfWords("one two three four", 3));
        }

        /*
         * MinimumNumberOfWords — failure case
         */

        [Test]
        public void MinimumNumberOfWords_ShouldFail_IfMinimumIsNotMet() {
            Assert.IsFalse(Validator.MinimumNumberOfWords("one", 2));
            Assert.IsFalse(Validator.MinimumNumberOfWords("one two", 3));
        }

        /*
         * MinimumNumberOfLetters and MaximumNumberOfLetters (issue 6)
         */

        [Test]
        public void MinimumNumberOfLetters_ShouldPass_IfMinimumIsMet() {
            Assert.IsTrue(Validator.MinimumNumberOfLetters("hello", 5));
            Assert.IsTrue(Validator.MinimumNumberOfLetters("hello world", 3));
        }

        [Test]
        public void MinimumNumberOfLetters_ShouldFail_IfMinimumIsNotMet() {
            Assert.IsFalse(Validator.MinimumNumberOfLetters("hi", 5));
            Assert.IsFalse(Validator.MinimumNumberOfLetters("a", 3));
        }

        [Test]
        public void MaximumNumberOfLetters_ShouldPass_IfMaximumIsMet() {
            Assert.IsTrue(Validator.MaximumNumberOfLetters("hello", 5));
            Assert.IsTrue(Validator.MaximumNumberOfLetters("hi", 10));
        }

        [Test]
        public void MaximumNumberOfLetters_ShouldFail_IfMaximumIsExceeded() {
            Assert.IsFalse(Validator.MaximumNumberOfLetters("hello world", 5));
            Assert.IsFalse(Validator.MaximumNumberOfLetters("abcdef", 3));
        }

        /*
         * Null input (issue 9)
         */

        [Test]
        public void AllFormatMethods_ShouldReturnFalse_ForNull() {
            Assert.IsFalse(Validator.Email(null));
            Assert.IsFalse(Validator.URL(null));
            Assert.IsFalse(Validator.IPv4Format(null));
            Assert.IsFalse(Validator.IPv4(null));
            Assert.IsFalse(Validator.IPv6(null));
        }

        [Test]
        public void AllCountMethods_ShouldReturnFalse_ForNull() {
            Assert.IsFalse(Validator.NumberOfWords(null, 1, 5));
            Assert.IsFalse(Validator.MinimumNumberOfWords(null, 1));
            Assert.IsFalse(Validator.MaximumNumberOfWords(null, 5));
            Assert.IsFalse(Validator.NumberOfLetters(null, 1, 5));
            Assert.IsFalse(Validator.MinimumNumberOfLetters(null, 1));
            Assert.IsFalse(Validator.MaximumNumberOfLetters(null, 5));
        }

        /*
         * Empty and whitespace-only input (issue 9)
         */

        [Test]
        public void AllFormatMethods_ShouldReturnFalse_ForEmptyString() {
            Assert.IsFalse(Validator.Email(""));
            Assert.IsFalse(Validator.URL(""));
            Assert.IsFalse(Validator.IPv4Format(""));
            Assert.IsFalse(Validator.IPv4(""));
            Assert.IsFalse(Validator.IPv6(""));
        }

        [Test]
        public void AllFormatMethods_ShouldReturnFalse_ForWhitespaceOnlyString() {
            Assert.IsFalse(Validator.Email("   "));
            Assert.IsFalse(Validator.URL("   "));
            Assert.IsFalse(Validator.IPv4Format("   "));
            Assert.IsFalse(Validator.IPv4("   "));
            Assert.IsFalse(Validator.IPv6("   "));
        }

        /*
         * Inverted min/max range (issue 9)
         */

        [Test]
        public void NumberOfWords_ShouldThrow_WhenMinimumExceedsMaximum() {
            Assert.Throws<ArgumentException>(() => Validator.NumberOfWords("hello world", 5, 1));
        }

        [Test]
        public void NumberOfLetters_ShouldThrow_WhenMinimumExceedsMaximum() {
            Assert.Throws<ArgumentException>(() => Validator.NumberOfLetters("hello", 10, 3));
        }

        /*
         * ValidationResult
         */

        [Test]
        public void ValidationResult_ShouldBeValid_OnSuccess() {
            ValidationResult result = Validator.Email("test@mail.com");
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.Message);
        }

        [Test]
        public void ValidationResult_ShouldBeInvalid_WithMessage_OnFailure() {
            ValidationResult result = Validator.Email("notanemail");
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.Message);
            Assert.IsNotEmpty(result.Message);
        }

        [Test]
        public void ValidationResult_ShouldCarryNullMessage_OnNullInput() {
            ValidationResult result = Validator.Email(null);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Value cannot be null", result.Message);
        }

        [Test]
        public void ValidationResult_WordCount_ShouldDescribeFailureReason() {
            ValidationResult tooFew = Validator.MinimumNumberOfWords("one", 3);
            Assert.IsFalse(tooFew.IsValid);
            StringAssert.Contains("1", tooFew.Message);
            StringAssert.Contains("3", tooFew.Message);

            ValidationResult tooMany = Validator.MaximumNumberOfWords("one two three", 2);
            Assert.IsFalse(tooMany.IsValid);
            StringAssert.Contains("3", tooMany.Message);
            StringAssert.Contains("2", tooMany.Message);
        }

        [Test]
        public void ValidationResult_ImplicitBoolConversion_Works() {
            Assert.IsTrue(Validator.Email("test@mail.com"));
            Assert.IsFalse(Validator.Email("notanemail"));
        }

        /*
         * NotNull
         */

        [Test]
        public void NotNull_ShouldPass_ForNonNullReference() {
            Assert.IsTrue(Validator.NotNull("hello"));
            Assert.IsTrue(Validator.NotNull(new object()));
        }

        [Test]
        public void NotNull_ShouldFail_ForNull() {
            Assert.IsFalse(Validator.NotNull<string>(null));
            Assert.IsFalse(Validator.NotNull<object>(null));
        }

        /*
         * NotNullOrEmpty
         */

        [Test]
        public void NotNullOrEmpty_ShouldPass_ForNonEmptyString() {
            Assert.IsTrue(Validator.NotNullOrEmpty("hello"));
            Assert.IsTrue(Validator.NotNullOrEmpty("   "));
        }

        [Test]
        public void NotNullOrEmpty_ShouldFail_ForNullOrEmpty() {
            Assert.IsFalse(Validator.NotNullOrEmpty(null));
            Assert.IsFalse(Validator.NotNullOrEmpty(""));
        }

        /*
         * NotNullOrWhitespace
         */

        [Test]
        public void NotNullOrWhitespace_ShouldPass_ForNonWhitespaceString() {
            Assert.IsTrue(Validator.NotNullOrWhitespace("hello"));
            Assert.IsTrue(Validator.NotNullOrWhitespace("  x  "));
        }

        [Test]
        public void NotNullOrWhitespace_ShouldFail_ForNullEmptyOrWhitespace() {
            Assert.IsFalse(Validator.NotNullOrWhitespace(null));
            Assert.IsFalse(Validator.NotNullOrWhitespace(""));
            Assert.IsFalse(Validator.NotNullOrWhitespace("   "));
        }

        /*
         * InRange (int)
         */

        [Test]
        public void InRange_Int_ShouldPass_WhenValueIsWithinBounds() {
            Assert.IsTrue(Validator.InRange(5, 1, 10));
            Assert.IsTrue(Validator.InRange(1, 1, 10));
            Assert.IsTrue(Validator.InRange(10, 1, 10));
        }

        [Test]
        public void InRange_Int_ShouldFail_WhenValueIsOutsideBounds() {
            Assert.IsFalse(Validator.InRange(0, 1, 10));
            Assert.IsFalse(Validator.InRange(11, 1, 10));
        }

        [Test]
        public void InRange_Int_ShouldThrow_WhenMinimumExceedsMaximum() {
            Assert.Throws<ArgumentException>(() => Validator.InRange(5, 10, 1));
        }

        /*
         * InRange (float)
         */

        [Test]
        public void InRange_Float_ShouldPass_WhenValueIsWithinBounds() {
            Assert.IsTrue(Validator.InRange(0.5f, 0f, 1f));
            Assert.IsTrue(Validator.InRange(0f, 0f, 1f));
            Assert.IsTrue(Validator.InRange(1f, 0f, 1f));
        }

        [Test]
        public void InRange_Float_ShouldFail_WhenValueIsOutsideBounds() {
            Assert.IsFalse(Validator.InRange(-0.1f, 0f, 1f));
            Assert.IsFalse(Validator.InRange(1.1f, 0f, 1f));
        }

        [Test]
        public void InRange_Float_ShouldThrow_WhenMinimumExceedsMaximum() {
            Assert.Throws<ArgumentException>(() => Validator.InRange(0.5f, 1f, 0f));
        }

        /*
         * Positive
         */

        [Test]
        public void Positive_Int_ShouldPass_WhenValueIsAboveZero() {
            Assert.IsTrue(Validator.Positive(1));
            Assert.IsTrue(Validator.Positive(100));
        }

        [Test]
        public void Positive_Int_ShouldFail_WhenValueIsZeroOrBelow() {
            Assert.IsFalse(Validator.Positive(0));
            Assert.IsFalse(Validator.Positive(-1));
        }

        [Test]
        public void Positive_Float_ShouldPass_WhenValueIsAboveZero() {
            Assert.IsTrue(Validator.Positive(0.1f));
            Assert.IsTrue(Validator.Positive(100f));
        }

        [Test]
        public void Positive_Float_ShouldFail_WhenValueIsZeroOrBelow() {
            Assert.IsFalse(Validator.Positive(0f));
            Assert.IsFalse(Validator.Positive(-0.1f));
        }

        /*
         * NonNegative
         */

        [Test]
        public void NonNegative_Int_ShouldPass_WhenValueIsZeroOrAbove() {
            Assert.IsTrue(Validator.NonNegative(0));
            Assert.IsTrue(Validator.NonNegative(5));
        }

        [Test]
        public void NonNegative_Int_ShouldFail_WhenValueIsNegative() {
            Assert.IsFalse(Validator.NonNegative(-1));
            Assert.IsFalse(Validator.NonNegative(-100));
        }

        [Test]
        public void NonNegative_Float_ShouldPass_WhenValueIsZeroOrAbove() {
            Assert.IsTrue(Validator.NonNegative(0f));
            Assert.IsTrue(Validator.NonNegative(0.5f));
        }

        [Test]
        public void NonNegative_Float_ShouldFail_WhenValueIsNegative() {
            Assert.IsFalse(Validator.NonNegative(-0.1f));
            Assert.IsFalse(Validator.NonNegative(-100f));
        }

        /*
         * Matches (Regex overload)
         */

        [Test]
        public void Matches_Regex_ShouldPass_WhenStringMatchesPattern() {
            var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
            Assert.IsTrue(Validator.Matches("1234", pattern));
        }

        [Test]
        public void Matches_Regex_ShouldFail_WhenStringDoesNotMatchPattern() {
            var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
            Assert.IsFalse(Validator.Matches("abcd", pattern));
            Assert.IsFalse(Validator.Matches("123", pattern));
        }

        [Test]
        public void Matches_Regex_ShouldFail_ForNull() {
            var pattern = new System.Text.RegularExpressions.Regex(@"^\d{4}$");
            Assert.IsFalse(Validator.Matches(null, pattern));
        }

        /*
         * Matches (string overload)
         */

        [Test]
        public void Matches_String_ShouldPass_WhenStringMatchesPattern() {
            Assert.IsTrue(Validator.Matches("hello123", @"^[a-z0-9]+$"));
        }

        [Test]
        public void Matches_String_ShouldFail_WhenStringDoesNotMatchPattern() {
            Assert.IsFalse(Validator.Matches("Hello!", @"^[a-z0-9]+$"));
        }

        [Test]
        public void Matches_String_ShouldFail_ForNull() {
            Assert.IsFalse(Validator.Matches(null, @"^[a-z]+$"));
        }

        /*
         * AlphanumericOnly
         */

        [Test]
        public void AlphanumericOnly_ShouldPass_ForAlphanumericStrings() {
            Assert.IsTrue(Validator.AlphanumericOnly("hello"));
            Assert.IsTrue(Validator.AlphanumericOnly("Hello123"));
            Assert.IsTrue(Validator.AlphanumericOnly("ABC"));
        }

        [Test]
        public void AlphanumericOnly_ShouldFail_WhenStringContainsNonAlphanumericCharacters() {
            Assert.IsFalse(Validator.AlphanumericOnly("hello world"));
            Assert.IsFalse(Validator.AlphanumericOnly("hello!"));
            Assert.IsFalse(Validator.AlphanumericOnly("café"));
            Assert.IsFalse(Validator.AlphanumericOnly(""));
        }

        [Test]
        public void AlphanumericOnly_ShouldFail_ForNull() {
            Assert.IsFalse(Validator.AlphanumericOnly(null));
        }

        /*
         * DoesNotContain
         */

        [Test]
        public void DoesNotContain_ShouldPass_WhenSubstringIsAbsent() {
            Assert.IsTrue(Validator.DoesNotContain("hello world", "foo"));
            Assert.IsTrue(Validator.DoesNotContain("username", "admin"));
        }

        [Test]
        public void DoesNotContain_ShouldFail_WhenSubstringIsPresent() {
            Assert.IsFalse(Validator.DoesNotContain("hello world", "world"));
            Assert.IsFalse(Validator.DoesNotContain("superadmin", "admin"));
        }

        [Test]
        public void DoesNotContain_ShouldFail_ForNullString() {
            Assert.IsFalse(Validator.DoesNotContain(null, "foo"));
        }

        /*
         * IsInteger
         */

        [Test]
        public void IsInteger_ShouldPass_ForValidIntegerStrings() {
            Assert.IsTrue(Validator.IsInteger("0"));
            Assert.IsTrue(Validator.IsInteger("42"));
            Assert.IsTrue(Validator.IsInteger("-10"));
        }

        [Test]
        public void IsInteger_ShouldFail_ForNonIntegerStrings() {
            Assert.IsFalse(Validator.IsInteger("3.14"));
            Assert.IsFalse(Validator.IsInteger("abc"));
            Assert.IsFalse(Validator.IsInteger(""));
        }

        [Test]
        public void IsInteger_ShouldFail_ForNull() {
            Assert.IsFalse(Validator.IsInteger(null));
        }

        /*
         * IsFloat
         */

        [Test]
        public void IsFloat_ShouldPass_ForValidFloatStrings() {
            Assert.IsTrue(Validator.IsFloat("3.14"));
            Assert.IsTrue(Validator.IsFloat("0"));
            Assert.IsTrue(Validator.IsFloat("-1.5"));
            Assert.IsTrue(Validator.IsFloat("1e3"));
        }

        [Test]
        public void IsFloat_ShouldFail_ForNonFloatStrings() {
            Assert.IsFalse(Validator.IsFloat("abc"));
            Assert.IsFalse(Validator.IsFloat(""));
        }

        [Test]
        public void IsFloat_ShouldFail_ForNull() {
            Assert.IsFalse(Validator.IsFloat(null));
        }

        /*
         * NotEmpty<T>
         */

        [Test]
        public void NotEmpty_ShouldPass_WhenCollectionHasElements() {
            Assert.IsTrue(Validator.NotEmpty(new System.Collections.Generic.List<int> { 1, 2 }));
            Assert.IsTrue(Validator.NotEmpty(new int[] { 0 }));
        }

        [Test]
        public void NotEmpty_ShouldFail_WhenCollectionIsEmpty() {
            Assert.IsFalse(Validator.NotEmpty(new System.Collections.Generic.List<int>()));
            Assert.IsFalse(Validator.NotEmpty(new string[] { }));
        }

        [Test]
        public void NotEmpty_ShouldFail_WhenCollectionIsNull() {
            Assert.IsFalse(Validator.NotEmpty<int>(null));
        }

        /*
         * CountInRange<T>
         */

        [Test]
        public void CountInRange_ShouldPass_WhenCountIsWithinBounds() {
            Assert.IsTrue(Validator.CountInRange(new int[] { 1, 2, 3 }, 1, 5));
            Assert.IsTrue(Validator.CountInRange(new int[] { 1 }, 1, 1));
        }

        [Test]
        public void CountInRange_ShouldFail_WhenCountIsOutsideBounds() {
            Assert.IsFalse(Validator.CountInRange(new int[] { }, 1, 3));
            Assert.IsFalse(Validator.CountInRange(new int[] { 1, 2, 3, 4 }, 1, 3));
        }

        [Test]
        public void CountInRange_ShouldFail_WhenCollectionIsNull() {
            Assert.IsFalse(Validator.CountInRange<int>(null, 1, 5));
        }

        [Test]
        public void CountInRange_ShouldThrow_WhenMinimumExceedsMaximum() {
            Assert.Throws<ArgumentException>(() => Validator.CountInRange(new int[] { 1, 2 }, 5, 1));
        }

        /*
         * All (composite)
         */

        [Test]
        public void All_ShouldPass_WhenAllResultsAreValid() {
            ValidationResult result = Validator.All(
                Validator.NotNullOrEmpty("hello"),
                Validator.Positive(5),
                Validator.AlphanumericOnly("hello")
            );
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void All_ShouldFail_WhenAnyResultIsInvalid() {
            ValidationResult result = Validator.All(
                Validator.NotNullOrEmpty("hello"),
                Validator.Positive(-1),
                Validator.AlphanumericOnly("hello")
            );
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void All_ShouldCollectAllFailureMessages() {
            ValidationResult result = Validator.All(
                Validator.Positive(-1),
                Validator.AlphanumericOnly("hello!"),
                Validator.NotNullOrEmpty("")
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
            ValidationResult first = Validator.NotNullOrEmpty("hello");
            ValidationResult second = Validator.AlphanumericOnly("hello");
            Assert.IsTrue(first.And(second).IsValid);
        }

        [Test]
        public void And_ShouldReturnFirstResult_WhenFirstIsInvalid() {
            ValidationResult first = Validator.NotNullOrEmpty("");
            ValidationResult second = Validator.AlphanumericOnly("hello");
            ValidationResult chained = first.And(second);
            Assert.IsFalse(chained.IsValid);
            Assert.AreEqual(first.Message, chained.Message);
        }

        [Test]
        public void And_ShouldReturnSecondResult_WhenFirstIsValidAndSecondIsInvalid() {
            ValidationResult first = Validator.NotNullOrEmpty("hello!");
            ValidationResult second = Validator.AlphanumericOnly("hello!");
            ValidationResult chained = first.And(second);
            Assert.IsFalse(chained.IsValid);
            Assert.AreEqual(second.Message, chained.Message);
        }
    }
}
