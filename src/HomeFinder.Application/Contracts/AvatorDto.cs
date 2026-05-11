using System;
using System.Collections.Generic;
using System.Text;

namespace HomeFinder.Application.Contracts
{
    public class AvatarDto
    {
        public bool IsSet { get; set; } = false;
        public Stream Content { get; set; } 
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; }
    }
}
