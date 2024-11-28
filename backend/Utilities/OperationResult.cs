namespace CHESSPROJ.Utilities
{
    public class OperationResult<T> where T : class
    {
        public T Value { get; private set; }
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        private OperationResult(T value, bool isSuccess, string errorMessage = null)
        {
            Value = value;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        public static OperationResult<T> Success(T value) => new OperationResult<T>(value, true);
        public static OperationResult<T> Failure(string errorMessage) => new OperationResult<T>(null, false, errorMessage);
    }
}
