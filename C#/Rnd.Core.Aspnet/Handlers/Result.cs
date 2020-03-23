using System;

namespace Rnd.Core.Aspnet.Handlers
{
    public class Result
    {
        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && error != string.Empty)
                throw new InvalidOperationException("Success result must not have any errors.");

            if (!isSuccess && error == string.Empty)
                throw new InvalidOperationException("Fail result must have error specified.");

            IsSuccess = isSuccess;
            Error = error;
        }
        public bool IsSuccess { get; }
        public string Error { get; }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default, false, message);
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }
    }


    public class Result<T> : Result
    {
        readonly T value;

        internal protected Result(T value, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException("Fail results shouldn't contain a value.");

                return value;
            }
        }
    }
}