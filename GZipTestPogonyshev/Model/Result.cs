using System;

namespace GZipTestPogonyshev.Model {

    public class Result {

        protected Result(bool success, string error) {
            Contracts.Require(success || !string.IsNullOrEmpty(error), $"Invalid precondition {nameof(success)}, {nameof(error)}");
            Contracts.Require(!success || string.IsNullOrEmpty(error), $"Invalid precondition {nameof(success)}, {nameof(error)}");

            Success = success;
            Error = error;
        }

        public bool Success { get; }

        public string Error { get; }

        public bool Failure => !Success;

        public static Result Fail(string message) {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message) {
            return new Result<T>(default, false, message);
        }

        public static Result Ok() {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value) {
            return new Result<T>(value, true, string.Empty);
        }
    }

    public class Result<T> : Result {

        private T value;

        protected internal Result(T value, bool success, string error)
            : base(success, error) {
            Contracts.Require(value != null || !success, $"Invalid precondition {nameof(value)}, {nameof(success)}");

            Value = value;
        }

        public T Value {
            get {
                Contracts.Require(Success, $"Invalid precondition {nameof(Success)}");

                return value;
            }
            private set => this.value = value;
        }

    }

    public static class Contracts {

        public static void Require(bool precondition, string exceptionMessage) {
            if (!precondition) {
                throw new InvalidOperationException(exceptionMessage);
            }
        }

    }

}