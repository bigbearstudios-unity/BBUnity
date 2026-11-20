namespace BBUnity.Support.Validation {

    /// <summary>
    /// The result of a validation check. Carries a success flag and, on failure,
    /// a human-readable message describing why validation failed.
    ///
    /// Implicitly converts to <c>bool</c>, so existing call sites that treat the
    /// result as a boolean (e.g. <c>if (Validator.Email(str))</c>) continue to work
    /// without modification.
    ///
    /// <code>
    /// // Boolean usage — unchanged from the pre-ValidationResult API
    /// if (Validator.Email(input)) { ... }
    ///
    /// // Rich usage — inspect the failure reason
    /// ValidationResult result = Validator.Email(input);
    /// if (!result.IsValid) {
    ///     Debug.LogWarning(result.Message);
    /// }
    /// </code>
    /// </summary>
    public readonly struct ValidationResult {

        /// <summary>A cached result representing a successful validation.</summary>
        public static readonly ValidationResult Valid = new ValidationResult(true, null);

        /// <summary>True if the validated value passed all checks.</summary>
        public bool IsValid { get; }

        /// <summary>
        /// Human-readable description of why validation failed.
        /// <c>null</c> when <see cref="IsValid"/> is <c>true</c>.
        /// </summary>
        public string Message { get; }

        private ValidationResult(bool isValid, string message) {
            IsValid = isValid;
            Message = message;
        }

        /// <summary>Creates a failed <see cref="ValidationResult"/> with the given message.</summary>
        public static ValidationResult Fail(string message) =>
            new ValidationResult(false, message);

        /// <summary>Allows a <see cref="ValidationResult"/> to be used directly as a <c>bool</c>.</summary>
        public static implicit operator bool(ValidationResult result) => result.IsValid;

        /// <summary>
        /// Returns this result if it is invalid; otherwise returns <paramref name="other"/>.
        /// Useful for chaining checks where the second only makes sense when the first passed.
        /// <code>
        /// ValidationResult result = Validator.NotNullOrEmpty(name).And(Validator.AlphanumericOnly(name));
        /// </code>
        /// </summary>
        public ValidationResult And(ValidationResult other) {
            if (!IsValid) { return this; }
            return other;
        }

        public override string ToString() => IsValid ? "Valid" : $"Invalid: {Message}";
    }
}
