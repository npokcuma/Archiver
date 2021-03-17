using System.Collections.Generic;

namespace GZipTestPogonyshev.Converters
{
    internal class StringToOperationTypeConverter
    {
        private readonly Dictionary<string, OperationType> stringOperationTypeDictionary;

        public StringToOperationTypeConverter() {
            stringOperationTypeDictionary = new Dictionary<string, OperationType> {
                {"compress", OperationType.Compress},
                {"decompress", OperationType.Decompress}
            };
        }

        public OperationType GetOperationType(string stringOperationType) {
            return stringOperationTypeDictionary[stringOperationType];
        }
    }
}
