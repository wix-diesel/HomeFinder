# リサーチ: アイテム変更履歴

**機能**: 008-item-change-history | **日付**: 2026-05-06

## 調査項目

### R-001: EF Core でのトランザクション管理

**Decision**: `DbContext.SaveChangesAsync()` を1回呼び出すことで、アイテム更新と履歴記録を同一トランザクションにまとめる。

**Rationale**: EF Core では `SaveChanges` が呼ばれるまで変更はメモリ上に追跡されるため、`Item` の更新と `ItemHistory` の追加を同じ DbContext インスタンスに積んで一度 `SaveChangesAsync()` を呼べば自動的に同一トランザクションとなる。

**Alternatives considered**:
- `BeginTransactionAsync()` を明示的に使う → 不要な複雑性。EF Core の変更追跡で十分。
- ベストエフォート（履歴記録失敗でもアイテム更新は成功）→ SC-001 の「100% 記録」要件に反するため却下。

---

### R-002: 変更種別の設計

**Decision**: `ItemHistoryChangeType` 列挙型を Core 層に定義し、`Created` / `QuantityIncreased` / `QuantityDecreased` / `PriceUpdated` / `NameUpdated` / `CategoryUpdated` の6種別とする。

**Rationale**: 仕様の制約セクションで変更種別ごとの色指定が異なるため、UI 側でクラスを切り替えるには明確な種別情報が必要。文字列ではなく enum を使うことでタイプセーフかつコンパクト。

**Alternatives considered**:
- 文字列フィールド（"CREATED", "QUANTITY_UPDATED" 等）→ タイポリスクがあり enum が優れる。
- 汎用 `Updated` 種別のみ → 数量増減の色分け要件を満たせないため却下。

---

### R-003: ItemHistory の変更内容説明文の生成

**Decision**: 説明文はサーバーサイドで生成して `Description` フィールドに保存する。フォーマットは「{フィールド}が{値}{単位}に{変化}」（例:「数量が5個に増加」「名称が"掃除機"に変更」）。

**Rationale**: 仕様 Q3 の回答「変更後の値のみ記載」と spec の制約例「数量が5個に増加」に合致。サーバーサイドで生成することでフロントエンドの表示ロジックをシンプルに保てる。

**Alternatives considered**:
- フロントエンドで動的生成 → API-First 原則に反し、フロントエンドにビジネスロジックが漏れる。
- 変更前後の値を別フィールドで保存してフロントで組み立て → 設計が複雑になり、仕様要件を超える。

---

### R-004: 履歴取得 API の設計

**Decision**: `GET /api/items/{itemId}/history?limit=5` として最大5件を返す。既存の `ItemsController` にメソッドを追加する。

**Rationale**: RESTful なリソース階層設計に合致（Items の子リソースとして History）。limit パラメータで将来の件数拡張にも対応可能。

**Alternatives considered**:
- 専用コントローラー `HistoryController` → 規模が小さいため ItemsController への追加で十分。
- アイテム詳細レスポンス（`ItemDto`）に履歴を含める → DTO が肥大化し、履歴不要な呼び出しでも余分なデータを返す。

---

### R-005: フロントエンドでの JST 変換

**Decision**: API から受け取った ISO 8601 UTC 文字列を、フロントエンドで `Intl.DateTimeFormat` を使って JST（Asia/Tokyo）に変換して表示する。

**Rationale**: 憲章原則 II「UTC 内部・JST 表示」に準拠。既存コードベースでも同様の変換パターンが使われている。

**Alternatives considered**:
- `date-fns` / `dayjs` 等のライブラリ → 既存依存関係に含まれていない場合は追加コストがかかるため、標準 `Intl` API で十分。
- サーバーサイドで JST に変換して返す → 憲章違反。UTC で返すことが原則。

---

### R-006: 既存 recent-activity-card UI の活用

**Decision**: 既存の `.recent-activity-card`、`.recent-item`、`.recent-item.positive`、`.recent-item.neutral` クラスをそのまま流用し、新規に `.recent-item.created`（青系）と `.recent-item.other-update`（黄系）を追加定義する。

**Rationale**: 仕様の制約セクションに「現在の recent-activity-card クラスを利用する」と明記されており、既存スタイルの再利用でコストを最小化できる。

**Alternatives considered**:
- 専用コンポーネントとして新規作成 → 既存 UI と乖離するリスクがあり、仕様にも反する。
- 種別ごとにインラインスタイルで色指定 → クラス名で管理するほうがメンテナンス性が高い。

## まとめ

| 調査項目 | 決定事項 |
|----------|----------|
| R-001 トランザクション | `SaveChangesAsync()` 1回呼び出しで同一トランザクション |
| R-002 変更種別 | `ItemHistoryChangeType` enum（6種別） |
| R-003 説明文生成 | サーバーサイドで生成・保存 |
| R-004 履歴 API | `GET /api/items/{itemId}/history?limit=5` |
| R-005 JST 変換 | フロントエンドで `Intl.DateTimeFormat` を使用 |
| R-006 既存 UI 活用 | 既存クラス流用 + `.created`（青系）/`.other-update`（黄系）を追加定義 |
