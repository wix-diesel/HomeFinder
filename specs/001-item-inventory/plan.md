# Implementation Plan: 個人用物品管理

**Branch**: `001-item-inventory` | **Date**: 2026-04-24 | **Spec**: `/specs/001-item-inventory/spec.md`
**Input**: Feature specification from `/specs/001-item-inventory/spec.md`

## Summary

ログイン不要の個人向け在庫管理として、物品の一覧表示・詳細表示・新規登録を提供する。実装は Web API バックエンドを前提に、フロントエンド (Vue 3 + Vite) とバックエンド (ASP.NET Core) を分離し、SQL Server で永続化する。物品名称の一意制約と数量の正整数制約を API レイヤーとデータベースの双方で担保する。

## Technical Context

**Language/Version**: TypeScript 5.x (frontend), C# / .NET 10 (backend)  
**Primary Dependencies**: Vue 3, Vite, ASP.NET Core Web API, Entity Framework Core, SQL Server provider  
**Storage**: SQL Server (サーバー側永続化)  
**Testing**: Vitest (frontend unit), xUnit (backend unit/integration), API 契約確認テスト  
**Target Platform**: 最新モダンブラウザ + Linux/Windows 上の Web API ホスト  
**Project Type**: Web application (frontend + backend)  
**Performance Goals**: 一覧/詳細 API の p95 応答時間 300ms 以下 (ローカル開発時の目標)、登録成功率 95%以上  
**Constraints**: ログインなし、名称一意、数量は 1 以上の整数、日時はローカル表示形式  
**Scale/Scope**: 単一ユーザー向け、初期想定 0-500 件程度の物品データ

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- 憲章ファイル `.specify/memory/constitution.md` はプレースホルダのため、強制可能な原則・制約は未定義。
- 本計画では以下を暫定ゲートとして適用:
  - 仕様要求 (FR-001 〜 FR-009) を満たすこと
  - バリデーション規則を API 契約とデータモデルの双方に反映すること
  - ドキュメント成果物 (research/data-model/contracts/quickstart) を更新すること
- 判定 (Phase 0 前): PASS
- 判定 (Phase 1 後): PASS

## Project Structure

### Documentation (this feature)

```text
specs/001-item-inventory/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Models/
│   ├── Data/
│   ├── Contracts/
│   ├── Services/
│   └── Api/
├── tests/
│   ├── unit/
│   └── integration/
└── contracts/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/
    └── unit/
```

**Structure Decision**: Web application 構成を採用。現時点では仕様成果物の更新を優先し、実装フェーズで `frontend/` と `backend/` を追加する。

## Complexity Tracking

現時点で憲章違反に該当する項目はないため、追加の正当化は不要。

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
