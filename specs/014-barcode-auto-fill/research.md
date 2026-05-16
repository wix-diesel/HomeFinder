# 調査メモ: バーコード商品情報自動入力

## 目的

アイテム作成/編集フォームで、カメラ読み取りまたは手入力JANを使って商品情報を自動入力するための実装方針を確定する。

## Decision 1: カメラ読み取り方式

- Decision: ブラウザ標準 API（`navigator.mediaDevices.getUserMedia` と `BarcodeDetector`）を第一選択とする
- Rationale: 仕様制約でブラウザカメラ API 利用が明示されており、依存追加を抑えて既存 Vue 構成に統合しやすい
- Alternatives considered:
  - `zxing-js` など外部ライブラリ導入: 対応ブラウザが広がる利点はあるが、依存増加と運用負荷が増える
  - ネイティブアプリ/外部スキャナ依存: 現行 Web UI スコープ外

## Decision 2: 商品情報取得インターフェース

- Decision: 既存 API `GET /api/products/{jan}` をそのまま利用する
- Rationale: すでに `docs/jan-search-api.md` で契約が定義済みで、商品名・メーカー・価格の要件を満たす
- Alternatives considered:
  - 新規 endpoint 追加: 重複責務となり API-First 原則に反する
  - フロントから外部 API を直接呼ぶ: 認証情報漏えいリスクがあり不適切

## Decision 3: タイムアウト・再試行

- Decision: API 呼び出しは 3 秒で失敗表示し、自動再試行しない（手動再試行のみ）
- Rationale: Clarifications で確定済み。失敗時の挙動を予測可能にし、UI 待機時間を抑える
- Alternatives considered:
  - 自動再試行 1 回: 一時障害には有効だが、失敗原因が不明瞭になり操作感が不安定
  - タイムアウトなし: ユーザー体験が悪化

## Decision 4: 連続操作時の整合

- Decision: 実行中検索はキャンセルし、最後の入力のみ処理。加えて同時実行 1 件 + 完了後 500ms クールダウン
- Rationale: 古い検索結果の反映を防ぎ、フォーム値の一貫性を担保できる
- Alternatives considered:
  - キュー実行: 入力順保証はできるが古い結果が遅れて反映される
  - 並列実行: レースコンディションが増える

## Decision 5: 欠損データと保存可否

- Decision: 商品名は必須。商品名欠損時は保存不可、価格/メーカー欠損時は警告表示の上で保存可
- Rationale: 必須データ品質を維持しつつ、業務継続性を確保
- Alternatives considered:
  - 欠損があれば常に保存不可: 実運用の入力停止が増える
  - 常に保存可: データ品質低下リスク

## 実装上の注意

- JAN 形式は UI 側で先行検証（8桁/13桁数字）し、無効値は API 呼び出し前に弾く
- カメラ権限拒否時は手入力導線を維持し、エラー種別をユーザーに明示する
- 既存入力がある項目は差分表示して項目単位で採用値を選択可能にする
