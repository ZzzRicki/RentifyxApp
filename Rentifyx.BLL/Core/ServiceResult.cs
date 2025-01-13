namespace Rentifyx.BLL.Core
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public ServiceResult()
        {
            Success = true;
        }
    }
}
