# Data Model: アイテム保管場所表示

**日付**: 2026-05-29  
**対象機能**: 016-item-storage-location

## 目的

アイテムに紐づく部屋・棚情報の保存と表示に必要なデータ構造、制約、状態遷移を定義する。

## 主要エンティティ

### Item（既存拡張）

- 概要: 管理対象アイテム。保管場所として部屋・棚を任意で保持する。

属性:
- id: 一意識別子
- name: アイテム名
- roomId: 部屋ID（nullable）
- shelfId: 棚ID（nullable）
- 既存監査項目: createdAt, updatedAt など

制約:
- roomId は null 許容
- shelfId は null 許容
- shelfId が非 null の場合、対応する roomId も非 null 必須

### Room（既存参照）

- 概要: 保管場所の上位単位。

属性:
- id: 一意識別子
- name: 部屋名
- isDeleted: 論理削除フラグ

### Shelf（既存参照）

- 概要: 保管場所の下位単位。必ず部屋に所属する。

属性:
- id: 一意識別子
- roomId: 所属部屋ID
- name: 棚名
- isDeleted: 論理削除フラグ

制約:
- shelf.roomId は必須
- 同一 shelf は常に一つの room に所属

## リレーション

- Room 1 : N Shelf
- Item N : 0..1 Room
- Item N : 0..1 Shelf

整合ルール:
- item.shelfId が設定される場合、item.roomId は必須
- item.shelfId が設定される場合、item.roomId は shelf.roomId と一致必須

## 表示モデル（Read Model）

編集/詳細表示向けに以下の投影を持つ:

- roomDisplayName: string
- shelfDisplayName: string

表示ルール:
- roomId/shelfId が null の場合は「未設定」
- 参照先が論理削除済みの場合は「削除済み（元の名称）」

## 状態遷移

### 保管場所設定状態

1. 未設定
- roomId = null, shelfId = null

2. 部屋のみ設定
- roomId != null, shelfId = null

3. 部屋+棚設定
- roomId != null, shelfId != null

禁止状態:
- roomId = null, shelfId != null

### 遷移ルール

- 未設定 -> 部屋のみ設定: 許可
- 部屋のみ設定 -> 部屋+棚設定: 許可
- 部屋+棚設定 -> 部屋変更: 許可（棚は自動クリア）
- 任意状態 -> 未設定: 許可

## バリデーション

API レイヤー:
- shelfId 指定時に roomId 未指定なら 400
- shelfId が roomId に属さないなら 400

DB レイヤー:
- Item の roomId/shelfId は FK 制約で参照整合性を確保
- 既存の論理削除ポリシーに従い、削除済み参照の読み取り表示は許容

## テスト観点

1. roomId/shelfId とも null で更新成功
2. roomId 指定・shelfId null で更新成功
3. roomId/shelfId の整合組み合わせで更新成功
4. shelfId 単独指定で更新失敗（400）
5. room 不一致 shelfId 指定で更新失敗（400）
6. 削除済み room/shelf 参照の表示が「削除済み（元の名称）」
