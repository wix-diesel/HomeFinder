# 共通コンポーネント利用ガイド

- `StatePanel`: 空状態・検証エラー・送信中・失敗の4状態表示に利用する。
- `AppPrimaryButton`: 主要アクション用。`loading`時に連打防止される。
- `FormField`: ラベル・入力・ヘルプ・エラーを一体表示する。
- `StockStatusBadge`: 数量に応じた在庫状態を表示する。
- `PageSectionHeader`: 画面セクションの見出しと補助説明を表示する。
- `ViewModeToggle`: カード/テーブルの表示切替に利用する。

## 再利用ルール

1. 新規ページでは、状態表示を `StatePanel` から開始する。
2. 入力UIは `FormField` と `AppPrimaryButton` を優先利用する。
3. 表示切替が必要な場合は `ViewModeToggle` を利用する。
4. 独自実装が必要な場合は、まず `common` 内拡張を検討する。
