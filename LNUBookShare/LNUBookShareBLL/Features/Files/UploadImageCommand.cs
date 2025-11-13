using MediatR;

namespace LNUBookShareBLL.Features.Files
{
    
    public class UploadImageCommand : IRequest<string>
    {
        public string? FileName { get; set; }
        public byte[]? ImageData { get; set; }
    }
}