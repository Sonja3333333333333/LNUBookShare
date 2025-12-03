using MediatR;

namespace LNUBookShareBLL.Features.Files
{
    public class UploadImageCommand : IRequest<string>
    {
        public string? FileName { get; set; }

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? ImageData { get; set; }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
    }
}