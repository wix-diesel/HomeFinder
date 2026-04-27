# 実装計画: 物品カテゴリー管理

**ブランチ**: `004-item-category-management` | **日付**: 2026-04-26 | **仕様**: `/specs/004-item-category-management/spec.md`
**入力**: `/specs/004-item-category-management/spec.md` の機能仕様

## 概要

設定画面から遷移できるカテゴリー管理ページを新設し、`design/category_list.html` と `design/category_dialog.html` を踏襲した UI でカテゴリー一覧・追加・編集・削除を提供する。実装は既存 `src/frontend` + `src/backend` 構成を維持し、フロントエンドでは設定画面導線・カテゴリーページ・ダイアログ・API クライアントを追加、バックエンドではカテゴリー CRUD API、永続化モデル、重複名防止、削除時の「未分類」再割り当てを実装する。未分類カテゴリは常設の予約カテゴリとして保持し、名称変更・削除不可とする。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3, Vue Router 4, Vite 8, ASP.NET Core Web API, Entity Framework Core, SQL Server  
**ストレージ**: SQL Server (カテゴリーテーブル追加、既存アイテムとの関連更新)  
**テスト**: Vitest + Vue Test Utils (frontend), xUnit 契約/統合テスト (backend)  
**対象プラットフォーム**: モダンブラウザ (PC/モバイル) + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application (`src/frontend` + `src/backend`)  
**性能目標**: カテゴリー一覧初期表示は通常利用で体感遅延なし、一覧/保存/削除後の画面更新は 1 秒以内を目標とする  
**制約**: `design/category_list.html` / `design/category_dialog.html` の構成準拠、設定画面からの導線必須、カテゴリー名は正規化後一意、アイコン・カラーは定義済み候補のみ、削除時は参照アイテムを常設の「未分類」へ自動付け替え、カテゴリー時刻は内部/API は UTC・UI 表示のみ JST、自動同期は対象外  
**規模/スコープ**: 新規ページ 1、設定画面改修 1、カテゴリーダイアログ 1、フロントエンド API サービス追加、バックエンド CRUD API 一式、DB マイグレーション 1、契約/単体/統合テスト一式

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

- Principle I (API-First): 合格  
  カテゴリーの重複判定、予約カテゴリ制約、削除時再割り当てはすべてバックエンドに実装し、フロントエンドは表示と入力補助に限定する。
  API 契約はカテゴリー専用の `contracts/categories-api.md` で維持する。
- Principle II (UTC 内部・JST 表示): 合格  
  カテゴリー作成/更新時刻は UTC で保存・返却し、UI に表示する場合のみフロントエンドで JST に変換する。
- Principle III (入力値検証の二重防御): 合格  
  API で名称・候補値・予約カテゴリ制約を検証し、DB でも正規化名一意制約と必須制約を実装する。
- Principle IV (テスト駆動開発): 合格  
  ユーザーストーリーごとにフロントエンド画面テスト、API 契約テスト、削除時再割り当ての統合テストを定義する。
- Principle V (成功基準の測定): 合格  
  遷移成功率、重複拒否、削除時再割り当て、昇順表示維持を計測対象として検証できる。
- Principle VI (ドキュメント・コード同期): 合格  
  本 plan に加えて category API 契約、データモデル、quickstart、テスト更新を同時に実施する。

Phase 0 前判定: 合格  
Phase 1 後判定: 合格

## 実装フェーズと品質ゲート

### Phase 1: 契約とデータ境界の確定

- 目的: カテゴリー API とデータモデルの責務を定義し、既存アイテムとの関連更新方針を確定する
- 完了条件: カテゴリー CRUD 契約、予約カテゴリ規則、削除時再割り当て方針が設計文書で一致している
- 品質ゲート:
  - カテゴリー一覧/追加/編集/削除 API 契約が `contracts/categories-api.md` に定義されている
  - 「未分類」カテゴリの制約とエラー契約が明文化されている
  - 既存 UI/ロジック再利用の監査結果が `research.md` に記録されている

### Phase 2: バックエンド基盤実装

- 目的: カテゴリー永続化、バリデーション、再割り当てロジックを持つ API 基盤を追加する
- 完了条件: `src/backend` にカテゴリーモデル、Repository、Service、Controller、Migration が追加される
- 品質ゲート:
  - 正規化名の重複が API と DB の両方で拒否される
  - 削除時に参照アイテムが必ず「未分類」へ再割り当てされる
  - 予約カテゴリの名称変更・削除が拒否される
  - カテゴリーの `createdAt` / `updatedAt` が UTC で永続化・返却される

### Phase 3: フロントエンド遷移導線と一覧画面

- 目的: 設定画面からカテゴリー管理ページへ遷移し、昇順一覧を表示できるようにする
- 完了条件: 設定画面のデータ管理項目からカテゴリー管理ページに遷移し、カテゴリ一覧が名称昇順で表示される
- 品質ゲート:
  - 設定画面のカテゴリー項目がインタラクティブになり、目的地が明確である
  - 空状態・読み込み中・取得失敗時の表示がある
  - 一覧カード UI が `design/category_list.html` の情報構成に沿う

### Phase 4: 追加・編集ダイアログ

- 目的: `design/category_dialog.html` を再現した追加/編集ダイアログを実装する
- 完了条件: 名称入力、定義済みアイコン選択、定義済みカラー選択、保存/キャンセルが機能する
- 品質ゲート:
  - 編集時に既存値が初期表示される
  - 重複名・不正候補値・通信失敗がダイアログ内で明示される
  - 保存成功後に一覧が昇順のまま更新される

### Phase 5: 削除フローと整合性維持

- 目的: 削除確認と未分類再割り当てを伴う安全な削除フローを提供する
- 完了条件: 通常カテゴリのみ削除可能で、削除後も参照アイテムが未分類へ安全に移動する
- 品質ゲート:
  - 予約カテゴリは削除 UI から除外または非活性化される
  - 削除成功後に一覧と保存データの整合が取れている
  - 別操作競合や対象消失時もクラッシュしない
  - 更新時に重複名変更が拒否される

### Phase 6: 横断検証

- 目的: 受け入れ条件・成功基準・憲章準拠を最終確認する
- 品質ゲート:
  - SC-001〜SC-009 に対応する検証結果を記録できる
  - frontend/backend のテストが通過する
  - 仕様、契約、実装、テストの用語が一致している
  - SC-009 を満たすため、一覧・作成・更新・削除の全 API エンドポイントで UTC (`Z`) 形式応答を検証している

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/004-item-category-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── categories-api.md
│   ├── settings-category-navigation-ui.md
│   └── category-dialog-ui.md
└── tasks.md
```

### ソースコード (リポジトリルート)

```text
src/
├── HomeFinder.Core/          # エンティティ・ドメイン例外（外部依存なし）
│   ├── Entities/
│   └── Errors/
├── HomeFinder.Application/   # サービス・リポジトリIF・DTO（Core のみに依存）
│   ├── Contracts/
│   ├── Repositories/
│   └── Services/             # Result<T> 返り値を使用
├── HomeFinder.Infrastructure/ # DbContext・リポジトリ実装（Application に依存）
│   ├── Data/
│   │   └── Migrations/
│   └── Repositories/
├── HomeFinder.Api/           # コントローラー・起動設定（Application + Infrastructure に依存）
│   ├── Controllers/
│   ├── Errors/
│   └── Program.cs
├── tests/
│   ├── contract/
│   └── integration/
└── frontend/
    ├── src/
    │   ├── components/
    │   ├── constants/
    │   ├── models/
    │   ├── pages/
    │   ├── router/
    │   ├── services/
    │   └── utils/
    └── tests/
        └── unit/
```

**構成方針**: バックエンドはオニオンアーキテクチャ（Core / Application / Infrastructure / Api）に分割し、Application 層のサービスは `Result<T>` 返り値を使用する。フロントエンドは `src/frontend/src/pages` にカテゴリ管理ページ、`src/frontend/src/components` にダイアログ/一覧カード、`src/frontend/src/services` に category API クライアントを追加し、`src/frontend/src/router` と `SettingsPage.vue` を最小差分で改修する。

## 複雑性トラッキング

憲章違反はないため、追加正当化は不要。

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|-----------|------------|-------------------------------------|
| なし | N/A | N/A |
