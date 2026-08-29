namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>
    /// Represents the outcome of an operation without throwing exceptions for business errors.
    /// </summary>
    public struct Result
    {
        private readonly bool _isSuccess;
        private readonly string _errorMessage;

        private Result(bool isSuccess, string errorMessage)
        {
            _isSuccess = isSuccess;
            _errorMessage = errorMessage;
        }

        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        public bool IsSuccess
        {
            get { return _isSuccess; }
        }

        /// <summary>Gets the failure description when the operation failed; otherwise null.</summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>Creates a successful result.</summary>
        public static Result Success()
        {
            return new Result(true, null);
        }

        /// <summary>Creates a failed result with the given failure description.</summary>
        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }
}
