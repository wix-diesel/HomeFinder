import { apiClient } from './apiClient';


/**
 * バックエンドから返ってきた相対パスを API エンドポイントに変換
 * デフォルトアイコンパスはそのまま（フロントエンドの public フォルダ）
 * ユーザーアップロード画像は GET /api/users/me/profile/avatar エンドポイントを使用
 */
function normalizeAvatarPath(path: string): string {
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }
  if (path.startsWith('/images/user-avatar-default.svg')) {
    return path; // フロントエンドのデフォルトアイコン
  }
  if (path.startsWith('/images/users/')) {
    // バックエンドで保存されたユーザーアイコンは API エンドポイント経由でアクセス
    return '/api/users/me/profile/avatar';
  }
  return path;
}

export interface UserProfile {
  entraObjectId: string;
  email: string;
  displayName: string;
  avatarImagePath: string;
}

export interface UpdateUserProfilePayload {
  displayName: string;
}

export class UserProfileServiceError extends Error
{
  readonly code: string;
  readonly details: Record<string, string>;

  constructor(
    message: string,
    code: string = 'UNKNOWN_ERROR',
    details: Record<string, string> = {},
  ) {
    super(message);
    this.code = code;
    this.details = details;
  }
}

async function parseError(response: Response): Promise<UserProfileServiceError> {
  try {
    const body = await response.json() as {
      code?: string;
      message?: string;
      details?: Array<{ field?: string; reason?: string }>;
    };

    const details: Record<string, string> = {};
    for (const detail of body.details ?? []) {
      const field = detail.field ?? '';
      const reason = detail.reason ?? '';
      if (field && reason) {
        details[field] = reason;
      }
    }

    return new UserProfileServiceError(
      body.message ?? 'プロフィール処理に失敗しました。',
      body.code ?? 'UNKNOWN_ERROR',
      details,
    );
  } catch {
    return new UserProfileServiceError('プロフィール処理に失敗しました。');
  }
}

export async function getMyProfile(): Promise<UserProfile> {
  const response = await apiClient.apiFetch('/api/users/me/profile');
  if (!response.ok) {
    throw await parseError(response);
  }

  const profile = await response.json() as { entraObjectId: string; email: string; displayName: string };
  return {
    ...profile,
    // バックエンドは avatarImagePath を返さないため、フロント側で常に API の avatar エンドポイントを参照する
    avatarImagePath: normalizeAvatarPath('/api/users/me/profile/avatar'),
  };
}

export async function uploadMyAvatar(file: File): Promise<void> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.apiFetch('/api/users/me/profile/avatar', {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw await parseError(response);
  }
  // 成功時は何も返さない（204 No Content）
}

export async function updateMyProfile(payload: UpdateUserProfilePayload): Promise<UserProfile> {
  const response = await apiClient.apiFetch('/api/users/me/profile', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  const profile = await response.json() as { entraObjectId: string; email: string; displayName: string };
  return {
    ...profile,
    avatarImagePath: normalizeAvatarPath('/api/users/me/profile/avatar'),
  };
}
