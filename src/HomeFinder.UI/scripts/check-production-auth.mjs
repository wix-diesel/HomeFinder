import { readdir, readFile } from 'node:fs/promises';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

async function checkDirectory(directory) {
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) {
      await checkDirectory(path);
    } else if (/\.(js|html)$/.test(entry.name)) {
      const content = await readFile(path, 'utf8');
      for (const marker of ['homefinder.e2e.token', 'e2e-user', '/__e2e/']) {
        if (content.includes(marker)) throw new Error(`本番ビルドに E2E 認証が含まれています: ${path}`);
      }
    }
  }
}

await checkDirectory(fileURLToPath(new URL('../dist', import.meta.url)));
console.log('本番ビルドに E2E 認証が含まれないことを確認しました。');
