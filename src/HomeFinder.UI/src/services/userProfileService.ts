import { apiClient } from './apiClient';

export interface UserProfile {
  entraObjectId: string;
  email: string;
  displayName: string;
  avatarImagePath: string;
}

export interface UpdateUserProfilePayload {
  displayName: string;
  avatarImagePath: string;
}

export class UserProfileServiceError extends Error
{
  constructor(
    message: string,
    public readonly code: string = 'UNKNOWN_ERROR',
    public readonly details: Record<string, string> = {},
  ) {
    super(message);
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

  return await response.json() as UserProfile;
}

export async function uploadMyAvatar(file: File): Promise<{ avatarImagePath: string }> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.apiFetch('/api/users/me/profile/avatar', {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return await response.json() as { avatarImagePath: string };
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

  return await response.json() as UserProfile;
}
