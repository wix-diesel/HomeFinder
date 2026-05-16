# データモデル: バーコード商品情報自動入力

## 概要

本機能は主に frontend のフォーム状態遷移を扱う。永続化モデルの追加は行わない。

## エンティティ

### 1. BarcodeLookupRequest

バーコード起点の検索要求を表す。

| フィールド | 型 | 必須 | 説明 |
|---|---|---|---|
| source | `camera` \| `manual` | Yes | 入力元（カメラまたは手入力） |
| jan | string | Yes | JANコード（8桁または13桁数字） |
| requestedAt | string (ISO-8601) | Yes | リクエスト開始時刻（内部トラッキング用） |

Validation:
- `jan` は `^\d{8}$` または `^\d{13}$`
- 無効形式の場合は API 呼び出しを行わない

### 2. ProductLookupResult

`GET /api/products/{jan}` の正規化結果。

| フィールド | 型 | 必須 | 説明 |
|---|---|---|---|
| name | string \| null | Yes | 商品名（null は欠損） |
| manufacturer | string \| null | No | メーカー名 |
| price | number \| null | No | 価格 |
| lookupStatus | `success` \| `not_found` \| `timeout` \| `failed` | Yes | 検索結果状態 |
| errorCode | string \| null | No | API エラーコード |

Validation:
- `lookupStatus = success` でも `name` が null の場合は保存不可扱い
- `price` は 0 以上のみ有効

### 3. ItemFormSnapshot

検索前後のフォーム比較に使う状態。

| フィールド | 型 | 必須 | 説明 |
|---|---|---|---|
| name | string | Yes | 物品名称 |
| manufacturer | string | No | メーカー |
| priceInput | string | No | 価格入力文字列 |
| barcode | string | No | バーコード |

### 4. FieldMergeDecision

既存値と取得値の競合に対する項目単位の採用結果。

| フィールド | 型 | 必須 | 説明 |
|---|---|---|---|
| field | `name` \| `manufacturer` \| `priceInput` | Yes | 対象項目 |
| currentValue | string | No | 既存値 |
| fetchedValue | string | No | 取得値 |
| selectedSource | `current` \| `fetched` | Yes | 採用元 |

Validation:
- `fetchedValue` が空の場合は `selectedSource = current` が既定

## 状態遷移

### BarcodeLookupSession

`idle` → `scanning` → `searching` → (`merge_selection` \| `completed` \| `error`)

補足:
- `searching` 中に新規入力が来た場合は現行検索をキャンセルし、最後の入力で新しい `searching` を開始
- `completed` 後は 500ms クールダウン中 `cooldown` とみなし、新規検索を受け付けない
- 3秒タイムアウト時は `error(timeout)` へ遷移し、手動再試行のみ可能

## 保存判定ルール

- 商品名が欠損している場合: 保存不可
- 商品名が存在し、価格またはメーカーが欠損している場合: 警告表示の上で保存可
- 通常入力（検索未使用）: 既存バリデーションに従う
