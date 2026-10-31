# Entra を使わない E2E テスト

Playwright から UI → 実際の API → SQL Server / Azurite を検証します。
Entra アカウント・アプリ登録・クライアントシークレットは不要です。

## 実行方法

前提: .NET 10 SDK、Node.js 22、pnpm 10.8.1、Docker Compose。

リポジトリのルートで SQL Server と Azurite を起動します。

```sh
docker compose -f docker-compose.e2e.yml -p homefinder-e2e up -d --wait --wait-timeout 180
cd src/HomeFinder.UI
pnpm install --frozen-lockfile
pnpm exec playwright install --with-deps chromium
pnpm run test:e2e
```

Playwright が専用 API ホストと UI を起動し、終了時に停止します。
SQL Server / Azurite の接続設定は `src/tests/e2e-host/appsettings.json` にあります。
このファイルの SQL パスワードと Azurite キーはローカルの使い捨て環境専用です。
通常の `.env` に Entra 設定を追加する必要はありません。

終了後、リポジトリのルートでコンテナを削除します。

```sh
docker compose -f docker-compose.e2e.yml -p homefinder-e2e down -v
```

HTML レポートは UI ディレクトリで `pnpm exec playwright show-report` により確認できます。
GitHub Actions では PR / main の CI で実行し、レポートと失敗時のトレースを保存します。

## 認証と認可

通常の UI は `@auth-provider` を MSAL 実装へ解決します。
E2E 専用の `vite.e2e.config.ts` だけが、テスト実装へ解決します。
`dist-e2e` に出力するため、本番の `dist` と混在しません。
実行時環境変数や `window.__APP_CONFIG__` では認証方式を切り替えられません。

API の通常起動は引き続き Entra JWT 認証のみを登録します。
`src/tests/e2e-host` は共通の API 構成・コントローラー・サービスを使い、認証ハンドラーだけを差し替えます。
既存の `[Authorize]` と認証必須のフォールバックポリシーを維持します。

| persona | トークン | ロール |
| --- | --- | --- |
| `user` | `e2e-user` | Items.Read / Items.Create / Items.Delete / User |
| `reader` | `e2e-reader` | Items.Read |
| `no-roles` | `e2e-no-roles` | なし。認証済みだが物品操作不可 |
| `anonymous` | なし | 未認証 |

新しいテストは `tests/e2e/fixtures.ts` の `test` / `expect` を使ってください。
`test.use({ persona: 'reader' })` でブラウザーのユーザーを指定できます。
テスト用 UI のログインボタンは通常ユーザーとしてログインします。
ユーザー ID とメールはテスト用 API の `/__e2e/session` から復元します。
Entra 宛てのブラウザー通信が発生した場合はテストを失敗させます。

各テストの前に専用 DB / Blob を初期化し、予約カテゴリー「未分類」を再作成します。
物品データはユーザー単位ではないため、ワーカー数は 1 に固定しています。
並列化する場合は、ワーカーごとに DB / Blob / ポートを分離してください。
JAN 商品検索はテスト用サービスへ置換し、`4901234567890` で固定の商品情報を返します。

## 本番との分離

- 通常の API は E2E ホストを参照せず、テストトークンを受け付けません。
- 初期化・セッション・起動確認用の `/__e2e/*` は専用ホストだけに登録します。
- 専用ホストは `E2E` 環境でのみ起動し、`127.0.0.1:5017` で待ち受けます。
- SQL は `127.0.0.1,14337` の `HomeFinderE2E`、Blob はローカル Azurite に限定します。
- Compose が公開する SQL / Blob ポートもループバックに限定します。
- CI で本番 UI の配布物にテスト認証が含まれないことを確認します。
- API の統合テストで、通常ホストがテストトークンを 401 で拒否することを確認します。

このテストは Entra 自体のログイン、署名検証、テナント設定を検証しません。
それらは実際の Entra 環境を使った接続確認で扱います。
