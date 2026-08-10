namespace Sektor.DarkestDungeon.Lan.Contracts.Results
{
    /// <summary>
    /// Represents the outcome of an operation carrying a value on success, without throwing
    /// exceptions for business errors.
    /// </summary>
    public sealed class Result<T>
    {
        private readonly T _value;
        private readonly string _errorMessage;

        private Result(T value, string errorMessage)
        {
            _value = value;
            _errorMessage = errorMessage;
        }

        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        public bool IsSuccess
        {
            get { return _errorMessage == null; }
        }

        /// <summary>Gets the produced value; only valid when the operation succeeded.</summary>
        public T Value
        {
            get { return _value; }
        }

        /// <summary>Gets the failure description when the operation failed; otherwise null.</summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>Creates a successful result carrying the given value.</summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(value, null);
        }

        /// <summary>Creates a failed result with the given failure description.</summary>
        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(default(T), errorMessage);
        }
    }
}
