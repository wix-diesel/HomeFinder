import { test, expect, apiUrl, fullAccessHeaders } from './fixtures';

test('ログイン済み状態を復元し、リロード後もプロフィールを取得できる', async ({ page }) => {
  await page.goto('/user-settings');
  await expect(page.getByTestId('email-input')).toHaveValue('user@example.test');
  await page.reload();
  await expect(page.getByTestId('email-input')).toHaveValue('user@example.test');
});

test('サインアウト後は保護された画面に戻れない', async ({ page }) => {
  await page.goto('/items');
  await page.getByRole('button', { name: 'サインアウト', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  await page.goto('/user-settings');
  await expect(page).toHaveURL(/\/login\?returnUrl=/);
  await page.reload();
  await expect(page.getByRole('button', { name: 'Microsoftでサインイン' })).toBeVisible();
});

test.describe('未ログイン', () => {
  test.use({ persona: 'anonymous' });

  test('ログイン画面に誘導され、Entra を経由せず元の画面に戻る', async ({ page }) => {
    await page.goto('/user-settings');
    await expect(page).toHaveURL(/\/login\?returnUrl=/);
    await page.getByRole('button', { name: 'Microsoftでサインイン' }).click();
    await expect(page).toHaveURL(/\/user-settings$/);
    await expect(page.getByTestId('email-input')).toHaveValue('user@example.test');
  });
});

test('トークンなし・未定義・不正形式は API で 401 になる', async ({ request }) => {
  const cases: Record<string, string>[] = [{}, { Authorization: 'Bearer unknown' }, { Authorization: 'Basic e2e-user' }];
  for (const headers of cases) {
    const response = await request.get(`${apiUrl}/api/items`, { headers });
    expect(response.status()).toBe(401);
  }
});

test.describe('閲覧専用ユーザー', () => {
  test.use({ persona: 'reader' });

  test('一覧は取得でき、登録・更新・削除は 403 になる', async ({ page, request }) => {
    const headers = { Authorization: 'Bearer e2e-reader' };
    expect((await request.get(`${apiUrl}/api/items`, { headers })).status()).toBe(200);
    const created = await request.post(`${apiUrl}/api/items`, {
      headers: fullAccessHeaders,
      data: { name: '閲覧対象', quantity: 1 },
    });
    expect(created.status()).toBe(201);
    const { id } = await created.json();
    expect((await request.post(`${apiUrl}/api/items`, { headers, data: { name: '登録禁止', quantity: 1 } })).status()).toBe(403);
    expect((await request.put(`${apiUrl}/api/items/${id}`, { headers, data: { name: '更新禁止', quantity: 1 } })).status()).toBe(403);
    expect((await request.delete(`${apiUrl}/api/items/${id}`, { headers })).status()).toBe(403);
    await page.goto('/items');
    await expect(page.getByRole('heading', { name: '閲覧対象', exact: true })).toBeVisible();
  });
});

test.describe('ロールなしユーザー', () => {
  test.use({ persona: 'no-roles' });

  test('プロフィールでは認証済み、物品 API では権限不足になる', async ({ page, request }) => {
    const headers = { Authorization: 'Bearer e2e-no-roles' };
    expect((await request.get(`${apiUrl}/api/users/me/profile`, { headers })).status()).toBe(200);
    expect((await request.get(`${apiUrl}/api/items`, { headers })).status()).toBe(403);
    await page.goto('/items');
    await expect(page.getByText('アクセス権がありません', { exact: true })).toBeVisible();
    await expect(page).toHaveURL(/\/items$/);
  });
});

test('テスト用の初期化も未ログイン・閲覧専用ユーザーでは拒否される', async ({ request }) => {
  expect((await request.post(`${apiUrl}/__e2e/reset`)).status()).toBe(401);
  expect((await request.post(`${apiUrl}/__e2e/reset`, { headers: { Authorization: 'Bearer e2e-reader' } })).status()).toBe(403);
});
