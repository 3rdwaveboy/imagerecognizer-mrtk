namespace _Scripts {
    public struct RecognitionResult {
        public ResultType Type;
        public string Message;
        public string Product;
        public float Loss;

        public RecognitionResult(ResultType typeType, string message, string product, float loss) {
            Type = typeType;
            Message = message;
            Product = product;
            Loss = loss;
        }
    }

    public enum ResultType {
        Failed,
        Success
    }
}