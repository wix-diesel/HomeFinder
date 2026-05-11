using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HomeFinder.Application.Contracts
{
    public class AvatarDto
    {
        public bool IsSet { get; set; } = false;
        public Stream Content { get; set; } = Stream.Null;
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; }
    }
}
