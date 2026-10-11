namespace BookCatalog.Domain.Common.Results
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            ArgumentNullException.ThrowIfNull(error);

            if (isSuccess && error != Error.None ||
                !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid result state.", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success()
        {
            return new Result(true, Error.None);
        }

        public static Result Failure(Error error)
        {
            return new Result(false, error);
        }

    }

    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public T Value => IsSuccess ? _value! 
            : throw new InvalidOperationException( "A failed result does not contain a value.");

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, Error.None);
        }

        public new static Result<T> Failure(Error error)
        {
            return new Result<T>(default, false, error);
        }
    }
}
