﻿using Microsoft.AspNetCore.Http;

namespace CAMS.Core.IServices.image
{
    public interface IImageService
    {
        Task<string> UploadImageOnServer(IFormFile image, bool deleteIfExist = false, string oldPath = null!, CancellationToken cancellationToken = default);
        Task RemoveImage(string oldPath);
    }
}
