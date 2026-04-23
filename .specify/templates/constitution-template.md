## 目的（Purpose）

本ドキュメントは、本プロジェクトにおける仕様（spec）の記述ルールを定義する。
すべての仕様は本ルールに従って作成・更新されなければならない。

---

## 必須項目（Required Fields）

各specには必ず以下の項目を含めること：

* name（仕様名）
* description（概要説明）
* endpoints（API一覧）

---

## 記述ルール（Writing Rules）

* 簡潔かつ明確に記述すること
* 現在形で記述すること
* 曖昧な表現（例：「適切に」「いい感じに」）は禁止
* 実装の詳細（技術・ライブラリ・DB構造）は記述しない

---
## Architecture Constraints

- BackendはREST APIとして設計する
- Backendはオニオンアーキテクチャを採用する。
- BackendのDomainService、ApplicationService層ではResult型を採用する。
- APIはOpenAPIに変換可能であること
- Backend、FrontendともにLinuxコンテナで動作可能にできるようにする。

---
## Language Constraints

- BackendはC#(.NET 10)を前提とする。
- Frontendはtypescripitを前提とし、Vue.jsをフレームワークとして使用する。
- データベース: SQL Server
- 型定義は静的型付けを前提とする

---

## API設計ルール（API Design Rules）

* RESTfulな設計に従うこと
* リソース名は複数形（例：/users, /movies）を使用する
* APIのバージョン情報(例：/v1/, /v2/)を必ず設定する
* HTTPメソッドは意味に応じて正しく使用する

  * GET：取得
  * POST：作成
  * PUT / PATCH：更新
  * DELETE：削除
* ステータスコードは標準に従うこと（200, 201, 400, 404, 500 など）

---

## エンドポイント定義ルール

各endpointは以下の項目を必ず含めること：

* method（HTTPメソッド）
* path（URLパス）
* description（処理内容の説明）

---

## 命名規則（Naming Conventions）

* ファイル名はkebab-case（例：user-api.yaml）
* フィールド名はcamelCase
* IDフィールドは "id" で統一する

---

## 禁止事項（Anti-Patterns）

以下の内容をspecに含めてはならない：

* データベース設計（テーブル定義など）
* UI仕様
* フレームワークやライブラリへの依存記述（例：EF Core, Blazorなど）
* ビジネスロジックの詳細実装

---

## データ設計ルール（任意）

* 不要なnullableは避ける
* 必須項目は明示する
* 日付はISO8601形式を使用する

---

## 運用ルール（Optional）

* specの変更は必ずレビューを行う
* 本ドキュメントに違反するspecは受け入れない
