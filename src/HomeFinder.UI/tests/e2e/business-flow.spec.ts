import { test, expect, apiUrl, fullAccessHeaders } from './fixtures';

test('物品を画面から登録・検索・編集・削除できる', async ({ page, request }) => {
  await page.goto('/items/new');
  await page.locator('input[name="name"]').fill('E2E 卓上ライト');
  await page.locator('input[name="quantity"]').fill('2');
  const createResponse = page.waitForResponse(response => response.url() === `${apiUrl}/api/items` && response.request().method() === 'POST');
  await page.getByRole('button', { name: '登録する', exact: true }).click();
  const created = await createResponse;
  expect(created.status()).toBe(201);
  const { id } = await created.json();
  await expect(page).toHaveURL(/\/items\?created=1$/);
  const searchResponse = page.waitForResponse(response => {
    const url = new URL(response.url());
    return url.pathname === '/api/items' && url.searchParams.get('keyword') === '卓上ライト';
  });
  await page.getByPlaceholder('名称で検索').fill('卓上ライト');
  expect((await searchResponse).status()).toBe(200);
  await page.getByRole('heading', { name: 'E2E 卓上ライト', exact: true }).click();
  await expect(page).toHaveURL(`/items/${id}`);
  await page.getByRole('button', { name: '編集', exact: true }).click();
  await page.locator('input[name="name"]').fill('E2E 更新済ライト');
  await page.getByRole('button', { name: '更新する', exact: true }).click();
  await expect(page).toHaveURL(/\/items\?updated=1$/);
  await page.getByRole('heading', { name: 'E2E 更新済ライト', exact: true }).click();
  await page.getByRole('button', { name: 'メニュー', exact: true }).click();
  await page.getByRole('button', { name: '削除', exact: true }).click();
  await expect(page.getByRole('heading', { name: '物品を削除しますか？' })).toBeVisible();
  await page.locator('.dialog-box').getByRole('button', { name: '削除', exact: true }).click();
  await expect(page).toHaveURL(/\/items$/);
  expect((await request.get(`${apiUrl}/api/items/${id}`, { headers: fullAccessHeaders })).status()).toBe(404);
});

test('画像を画面からアップロードし、Azurite から取得できる', async ({ page, request }) => {
  await page.goto('/items/new');
  await page.locator('input[name="name"]').fill('E2E 画像付き物品');
  await page.locator('input[name="quantity"]').fill('1');
  const image = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=', 'base64');
  await page.getByLabel('画像ファイルを選択', { exact: true }).setInputFiles({ name: 'test.png', mimeType: 'image/png', buffer: image });
  const createResponse = page.waitForResponse(response => response.url() === `${apiUrl}/api/items` && response.request().method() === 'POST');
  const uploadResponse = page.waitForResponse(response => response.url().endsWith('/image') && response.request().method() === 'POST');
  await page.getByRole('button', { name: '登録する', exact: true }).click();
  const { id } = await (await createResponse).json();
  expect((await uploadResponse).status()).toBe(200);
  await expect(page).toHaveURL(/\/items\?created=1$/);
  const retrieved = await request.get(`${apiUrl}/api/items/${id}/image`, { headers: fullAccessHeaders });
  expect(retrieved.status()).toBe(200);
  expect(retrieved.headers()['content-type']).toContain('image/');
  expect((await retrieved.body()).length).toBeGreaterThan(0);
  expect(retrieved.headers()['etag']).toBeTruthy();
});

test('プロフィールの変更が DB に保存され、別ユーザーと分離される', async ({ page, request }) => {
  await page.goto('/user-settings');
  await expect(page.getByTestId('email-input')).toHaveValue('user@example.test');
  await page.getByTestId('display-name-input').fill('E2E 表示名');
  const saveResponse = page.waitForResponse(response => response.url().endsWith('/api/users/me/profile') && response.request().method() === 'PUT');
  await page.getByTestId('save-button').click();
  expect((await saveResponse).status()).toBe(200);
  await page.reload();
  await expect(page.getByTestId('display-name-input')).toHaveValue('E2E 表示名');
  const other = await request.get(`${apiUrl}/api/users/me/profile`, { headers: { Authorization: 'Bearer e2e-reader' } });
  expect(other.status()).toBe(200);
  expect((await other.json()).displayName).not.toBe('E2E 表示名');
});
