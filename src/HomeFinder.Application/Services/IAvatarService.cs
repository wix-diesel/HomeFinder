using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HomeFinder.Application.Contracts;
using DotNext;

namespace HomeFinder.Application.Services;

public interface IAvatarService
{
    // ユーザーのアバターをアップロードする
    Task<Result<bool>> UploadAvatarAsync(string entraId, Stream imageStream, string fileName, long fileSizeBytes, CancellationToken cancellationToken = default);

    // ユーザーのアバターを取得する
    Task<Result<AvatarDto>> GetAvatarByEntraIdAsync(string entraId, CancellationToken cancellationToken = default);

    // ユーザーのアバターを削除する
    Task<Result<bool>> DeleteAvatarByEntraIdAsync(string entraId, CancellationToken cancellationToken = default);
}