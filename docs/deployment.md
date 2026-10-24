# 自動デプロイ

## 実行の流れ

1. HomeFinder の `main` に push / PR をマージ。
2. `CI` がバックエンド・フロントエンドのビルドとテスト、両コンテナのビルドを実行。
3. 全ジョブ成功後、`Publish & Deploy` が `publish-ghcr.yml` を再利用し、API / UI を GHCR に公開。
4. 両イメージの公開成功後、`homefinder-image-published` イベントを HomeFinderIaC に送信。
5. HomeFinderIaC の `Deploy HomeFinder (prod)` が、受信した digest のイメージを Azure Container Apps に反映。

PR の CI ではイメージ公開・デプロイを実行しません。CI が失敗・キャンセルされた場合も公開・通知を実行しません。新しい push が届いた場合は、同じブランチで進行中の古い CI / 公開処理をキャンセルします。すでに IaC に送信済みのデプロイは IaC 側で直列実行されます。

GHCR には `latest` と `sha-<40文字のコミットSHA>` タグを公開します。自動デプロイには `ghcr.io/wix-diesel/homefinder-api@sha256:...` / `homefinder-ui@sha256:...` を渡すため、公開後にタグが更新されてもデプロイ対象は変わりません。

## 初回設定

1. GitHub で Fine-grained personal access token を作成。
   - Resource owner: `wix-diesel`
   - Repository access: **Only select repositories** → `HomeFinderIaC`
   - Repository permissions: **Contents: Read and write** (Metadata の read は自動付与)
   - 有効期限を設定し、期限前にトークンを更新。
2. **HomeFinder リポジトリ** の Settings → Secrets and variables → Actions → New repository secret に登録。
   - Name: `IAC_DISPATCH_TOKEN`
   - Secret: 作成したトークン
3. HomeFinderIaC の既存 Azure OIDC / `prod` Environment / Azure Secrets を設定済みにする。詳細は [IaC セットアップ手順](https://github.com/wix-diesel/HomeFinderIaC/blob/main/docs/setup.md) を参照。
4. **IaC 側の変更を先に main に反映し、その後 HomeFinder 側の変更を main に反映**。

GitHub の標準 `GITHUB_TOKEN` は HomeFinder リポジトリに限定されるため、他リポジトリへの通知には上記トークンを使用します。`repository_dispatch` に必要な権限は [GitHub 公式ドキュメント](https://docs.github.com/en/rest/repos/repos#create-a-repository-dispatch-event) の Contents: write です。Actions: write は不要です。トークンは Repository Secret に保存し、このドキュメントやコードには記載しません。

IaC の `prod` Environment に required reviewers が設定されている場合、デプロイは承認待ちになります。完全自動で反映する場合は、その承認ルールを解除してください。GHCR の両パッケージは既存構成どおり public 公開が必要です。

## 結果確認と再実行

- HomeFinder の `CI` が全チェック成功後に `Publish & Deploy` を実行することを確認。
- `Trigger IaC Deployment` の Summary で API / UI の digest と IaC の Actions リンクを確認。
- **Azure への反映結果は HomeFinderIaC の `Deploy HomeFinder (prod)` で確認**。HomeFinder 側の成功は、デプロイ要求の送信完了を示します。
- `IAC_DISPATCH_TOKEN` が未設定・期限切れ・権限不足の場合は通知ジョブが失敗します。Secret を修正後、HomeFinder の失敗ジョブを再実行します。
- 片方のイメージ公開に失敗した場合は通知しません。失敗ジョブを再実行し、両方の公開成功を確認します。
- Azure のデプロイに失敗した場合は IaC 側のログを確認し、原因を解消して再実行します。

以前のバージョンに戻す場合は、以前の成功した Summary にある **API / UI 両方の digest** を使い、IaC の `Deploy HomeFinder (prod)` を手動実行してください。`apiImage` / `uiImage` にそれぞれの参照を指定します。入力を省略した手動実行や IaC のインフラ変更による実行は、従来どおり `latest` を使用します。
