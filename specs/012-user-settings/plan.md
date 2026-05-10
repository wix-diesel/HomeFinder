# 実装計画: ユーザー設定画面

**ブランチ**: `012-user-settings` | **日付**: 2026-05-10 | **仕様**: [spec.md](./spec.md)
**入力**: `/specs/012-user-settings/spec.md` の機能仕様

## 概要

ユーザーの表示名・アイコンを確認/更新できるユーザー設定画面を実装する。バックエンドはオニオンアーキテクチャを維持しつつ `UserProfile` エンティティと本人専用 API（`GET /api/users/me/profile`、`POST /api/users/me/profile/avatar`、`PUT /api/users/me/profile`）を追加する。フロントエンドは `design/user_settings.html` 準拠の新規ページを追加し、ヘッダー右上アイコンと設定画面プロフィール領域から遷移可能にする。保存時は成功トースト、失敗時は失敗トースト + フィールドエラー表示を行い、ページ遷移なしで更新結果を反映する。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x（frontend）, C# / .NET 10（backend）  
**主要依存関係**: Vue 3 + Vue Router + Pinia（frontend）, ASP.NET Core Web API + EF Core + DotNext.Result（backend）  
**ストレージ**: SQL Server（`UserProfiles` テーブルを新規追加）  
**テスト**: Vitest + Vue Test Utils（frontend）, xUnit 契約テスト（backend）  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application（frontend + backend）  
**性能目標**: 画面表示から保存完了まで 2 秒以内（SC-002）、保存反映 1 秒以内（SC-003）  
**制約**: メールアドレスは読み取り専用、表示名 1〜30 文字（絵文字可）、アイコン形式 PNG/JPG・2MB 上限、デザインは `design/user_settings.html` 準拠  
**規模/スコープ**: 新規ページ 1、API エンドポイント 3、新規エンティティ 1、新規契約 1

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### Phase 0 前チェック

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | プロフィール更新は API 経由で実施し、フロントは表示/入力に限定 |
| II. UTC 内部・JST 表示 | ✅ 合格 | `CreatedAtUtc` / `UpdatedAtUtc` を UTC で保持 |
| III. 入力値検証の二重防御 | ✅ 合格 | UI バリデーション + API/DB 制約（LEN/NOT NULL/UNIQUE）を併用 |
| IV. テスト駆動開発 | ✅ 合格 | 契約テスト/画面テストを tasks で先行定義可能 |
| V. 成功基準の測定 | ✅ 合格 | SC-001〜SC-005 を検証可能な設計 |
| VI. ドキュメント・コード同期 | ✅ 合格 | contracts と実装を同時更新する前提 |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | Core/Application/Infrastructure/Api の責務分離で追加 |
| VIII. ドキュメント言語 | ✅ 合格 | 本機能ドキュメントは日本語で統一 |

### Phase 1 後の再評価

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | `GET/POST/PUT /api/users/me/profile*` 契約を定義済み |
| II. UTC 内部・JST 表示 | ✅ 合格 | データモデルで UTC フィールドを明示 |
| III. 入力値検証の二重防御 | ✅ 合格 | `DisplayName` 制約を UI/API/DB で一致定義 |
| IV. テスト駆動開発 | ✅ 合格 | quickstart に契約テスト/単体テスト実行手順を記載 |
| V. 成功基準の測定 | ✅ 合格 | quickstart 検証手順で SC 計測観点を担保 |
| VI. ドキュメント・コード同期 | ✅ 合格 | research/data-model/contracts/quickstart を作成済み |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | サービスは `Result<T>`、リポジトリ IF/実装分離を維持 |
| VIII. ドキュメント言語 | ✅ 合格 | すべて日本語 |

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/012-user-settings/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── user-profile-api.md
└── tasks.md                           # Phase 2 で作成
```

### ソースコード変更対象

```text
src/
├── HomeFinder.Core/
│   └── Entities/
│       └── UserProfile.cs                         # 新規: ユーザープロフィールエンティティ
├── HomeFinder.Application/
│   ├── Contracts/
│   │   ├── UserProfileDto.cs                      # 新規
│   │   └── UpdateUserProfileRequest.cs            # 新規
│   ├── Repositories/
│   │   └── IUserProfileRepository.cs              # 新規
│   └── Services/
│       ├── IUserProfileService.cs                 # 新規
│       └── UserProfileService.cs                  # 新規（Result<T> 返却）
├── HomeFinder.Infrastructure/
│   ├── Data/
│   │   ├── ItemDbContext.cs                       # 変更: DbSet + Entity 設定追加
│   │   └── Migrations/                            # 変更: UserProfiles テーブル追加マイグレーション
│   └── Repositories/
│       └── UserProfileRepository.cs               # 新規
├── HomeFinder.Api/
│   ├── Controllers/
│   │   └── UserProfilesController.cs              # 新規: /api/users/me/profile, /api/users/me/profile/avatar
│   └── Program.cs                                 # 変更: DI 登録
├── HomeFinder.UI/
│   ├── public/images/
│   │   └── user-avatar-default.svg                # 新規: デフォルトアイコン
│   └── src/
│       ├── pages/
│       │   └── UserSettingsPage.vue               # 新規: design/user_settings.html 準拠
│       ├── services/
│       │   └── userProfileService.ts              # 新規: プロフィール API クライアント
│       ├── stores/
│       │   └── userProfileStore.ts                # 新規: 画面間同期用状態
│       ├── router/
│       │   └── index.ts                           # 変更: /user-settings ルート追加
│       ├── layouts/
│       │   └── AppLayout.vue                      # 変更: 右上アイコンの遷移接続
│       └── pages/
│           └── SettingsPage.vue                   # 変更: プロフィール領域クリック遷移
└── tests/
    └── contract/
    └── UserProfileApiContractTests.cs         # 新規: GET/POST/PUT 契約テスト
```

**構成方針**: 既存の Web アプリ + オニオンアーキテクチャ構成を維持し、ユーザープロフィール機能を垂直スライスで追加する。既存 Items 画像ドメインには手を入れず、プロフィール専用モデルを分離する。

## 複雑性トラッキング

憲章違反なし。追加の複雑性許容事項なし。

