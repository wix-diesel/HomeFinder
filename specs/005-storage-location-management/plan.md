# Implementation Plan: 部屋・棚管理

**Branch**: `005-item-category-management` | **Date**: 2026-04-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-storage-location-management/spec.md`

## Summary

部屋・棚は既存アイテム管理システムの保管場所マスタ。複数利用者で共有される部屋（上位単位）と棚（下位単位）を一覧・追加・編集・削除する管理画面として、既存デザイン資産を活用しながら Backend API と Frontend UI を統合実装する。

## Technical Context

**Language/Version**: .NET 10 with ASP.NET Core 10 (backend), Vue 3 + TypeScript (frontend)
**Primary Dependencies**: Entity Framework Core, LINQ, async/await pattern (backend); Vue router, Composition API, axios (frontend)
**Storage**: SQL Server (existing ItemDbContext)
**Testing**: xUnit (backend unit/integration), Contract tests (API validation), Vitest (frontend)
**Target Platform**: Web application (desktop browser)
**Project Type**: Feature enhancement to existing web service
**Design Baseline (Must Reproduce)**:
- List page: `design/storage_list.html`
- Room create/edit dialog: `design/storage_register_room.html`
- Shelf create/edit dialog: `design/storage_register_shelf.html`
**Design Reproduction Policy**:
- Above HTML files are visual source of truth for layout, spacing, typography, icon usage, color tokens, and component states.
- Implementation must preserve top navigation, sidebar, main content composition, dialog structure, and mobile bottom navigation behavior defined in baseline HTML.
- Differences are allowed only for data binding, accessibility attributes, and framework integration details.
**Backend Architecture**: Onion Architecture with Dependency Inversion
  - **Service Layer**: Implements business logic, returns `Result<T>` type for explicit error handling
  - **Repository Layer**: Data access abstraction with dependency injection
  - **Presentation Layer** (Controllers): Maps Result responses to HTTP status codes
**Performance Goals**: 
- API response time: <500ms (p95) for list operations
- UI interaction response: <100ms for dialog open/save
- Database query: <300ms for list with filters
**Constraints**: 
- 既存の Entity Framework Core スキーマに統合
- 既存の認証基盤（Present/Future）に依存しない設計
- 複数利用者の同時操作での競合制御は対象外（楽観的排他制御、最新版勝ち）
**Scale/Scope**: 
- Estimated 2-5 API endpoints (List, Create, Update, Delete per resource)
- ~8-12 Frontend UI components (List view, Dialog for Room, Dialog for Shelf)
- ~150-250 lines of backend service logic
- ~300-500 lines of frontend component code

## Constitution Check

| 原則 | 項目 | 状態 | 確認項目 |
|------|------|------|----------|
| I | API-First アーキテクチャ | ✅ PASS | Backend のビジネスロジック実装、Frontend は UI と呼び出しのみ |
| II | UTC/JST 日時処理 | ✅ PASS | 削除フラグ・タイムスタンプは Backend で UTC 管理、Frontend で JST 表示（なし：本機能は日時未使用） |
| III | 入力値検証の二重防御 | ✅ PASS | API レイヤーで長さ・必須チェック、DB レイヤーで UNIQUE・NOT NULL 制約 |
| IV | テスト駆動開発 | ✅ PASS | Phase 2 (tasks.md) で受け入れテストとコマンド実装を並行定義 |
| V | 成功基準の測定 | ✅ PASS | SC-001 ～ SC-005 対応検証タスクを Phase 2 に明記 |
| VI | ドキュメント・コード同期 | ✅ PASS | API 契約 (contracts/), spec, tasks を統一管理 |

**ゲート判定**: ✅ PASS（すべての MUST 原則に適合）

## Project Structure

### Documentation (this feature)

```text
specs/005-storage-location-management/
├── spec.md                          # Feature specification
├── plan.md                          # This file
├── research.md                      # Phase 0 output (↓)
├── data-model.md                    # Phase 1 output (↓)
├── quickstart.md                    # Phase 1 output (↓)
├── contracts/                       # Phase 1 output (↓)
│   ├── storage-locations-api.md     # Room/Shelf API contract
│   ├── storage-dialog-ui.md         # Dialog UI components contract
│   └── storage-list-ui.md           # List UI components contract
├── tasks.md                         # Phase 2 output (created by /speckit.tasks)
└── checklists/
    └── requirements.md
```

### Source Code

```text
# Backend - ASP.NET Core
src/backend/
├── Controllers/
│   ├── RoomsController.cs           # HTTP endpoints for Room CRUD
│   └── ShelvesController.cs         # HTTP endpoints for Shelf CRUD
├── Models/
│   ├── Room.cs                      # Domain entity
│   └── Shelf.cs                     # Domain entity
├── Contracts/
│   ├── CreateRoomRequest.cs         # DTO for room creation
│   ├── UpdateRoomRequest.cs         # DTO for room update
│   ├── CreateShelfRequest.cs        # DTO for shelf creation
│   ├── UpdateShelfRequest.cs        # DTO for shelf update
│   ├── RoomDto.cs                   # Response DTO for room
│   └── ShelfDto.cs                  # Response DTO for shelf
├── Services/
│   ├── RoomService.cs               # Business logic for rooms
│   └── ShelfService.cs              # Business logic for shelves
├── Data/
│   └── ItemDbContext.cs             # EF Core context (update with Room/Shelf DbSets)
└── Repositories/
    ├── RoomRepository.cs            # Data access for rooms
    └── ShelfRepository.cs           # Data access for shelves

# Frontend - Vue.js + TypeScript
src/frontend/src/
├── components/
│   ├── StorageLocationList.vue      # Main list view
│   ├── RoomDialog.vue               # Modal for room add/edit
│   ├── ShelfDialog.vue              # Modal for shelf add/edit
│   └── StorageLocationItem.vue      # List item component
├── pages/
│   └── StorageManagement.vue        # Page wrapper
├── services/
│   ├── roomService.ts               # API calls for rooms
│   └── shelfService.ts              # API calls for shelves
└── stores/
    └── storageStore.ts              # State management (Pinia)

# Testing
src/tests/
├── contract/
│   ├── StorageLocationsApiContract.cs  # Contract tests for API
│   └── StorageDataModelContract.cs     # Contract tests for data model
└── integration/
    ├── RoomServiceTests.cs          # Integration tests
    └── ShelfServiceTests.cs         # Integration tests

src/frontend/tests/
├── StorageLocationList.test.ts      # Component tests
├── RoomDialog.test.ts               # Dialog component tests
└── services.test.ts                 # Service mock tests
```

**選択理由**: 既存プロジェクト構造（Controllers/Models/Services/Repositories パターン）に準拠し、room・shelf は item と並行する entity として実装。Backend は Onion Architecture に従い、Service Layer が Result<T> 型で明示的なエラー処理を実行。これにより、ビジネスロジックが infrastructure concerns から独立し、testability と maintainability が向上する。

## Complexity Tracking

> Constitution Check: すべて PASS のため、違反なし

---

## Phase 0: Research & Clarification

**Output**: `research.md` | **Status**: Will be generated below

### Research Tasks

| Task | Technical Question | Required For |
|------|-------------------|--------------|
| R-001 | 既存 ItemDbContext への Room・Shelf entity 追加方式（新 DbSet vs 既存拡張） | DB schema design |
| R-002 | Item ↔ Room/Shelf の関連付け設計（FK 構造、cascade delete 動作） | Data model design |
| R-003 | 削除フラグ実装パターン（soft delete library vs 手動） | Repository 実装 |
| R-004 | 既存の UI コンポーネント再利用可否（Dialog/List/Form） | Frontend architecture |
| R-005 | API エラーレスポンス形式の既存パターン（統一コード） | Error handling contract |
| R-006 | 単語の重複チェック実装位置（DB UNIQUE vs ビジネスロジック） | Validation strategy |

**→ 詳細は [research.md](research.md) を参照**


---

## Phase 1: Design & Contracts

### 1.1 Data Model

**Output**: [data-model.md](data-model.md) ✅ COMPLETE

- Room entity (id, name, description, isDeleted, timestamps)
- Shelf entity (id, roomId FK, name, description, isDeleted, timestamps)
- Item entity extension (roomId?, shelfId? nullable FKs)
- Relationships: Room 1:* Shelf, Room 1:* Item, Shelf 1:* Item
- Constraints: UNIQUE (with IsDeleted filter), NOT NULL, FK on delete Restrict
- Soft delete logic: All queries filtered by IsDeleted=false

### 1.2 Contracts (API & UI)

**Output**: [contracts/](contracts/) ✅ COMPLETE

- [storage-locations-api.md](contracts/storage-locations-api.md): REST API endpoints
  - GET /api/rooms (list)
  - POST /api/rooms (create)
  - PUT /api/rooms/{id} (update)
  - DELETE /api/rooms/{id} (delete)
  - [Same pattern for /api/rooms/{roomId}/shelves]
  
- [storage-dialog-ui.md](contracts/storage-dialog-ui.md): Dialog component contracts
  - RoomDialog.vue props/events
  - ShelfDialog.vue props/events
  - Validation & error states
  
- [storage-list-ui.md](contracts/storage-list-ui.md): List component contract
  - StorageLocationList.vue structure
  - Room expansion/collapse
  - Add/Edit/Delete interactions

### 1.3 Quick Start Guide

**Output**: [quickstart.md](quickstart.md) ✅ COMPLETE

- Environment setup (Node.js, .NET 10, SQL Server)
- Database migrations
- Running backend (dotnet run)
- Running frontend (npm run dev)
- Running tests
- API usage examples (curl)
- Component usage examples
- Troubleshooting


---

## Implementation Readiness

| Artifact | Status | Link |
|----------|--------|------|
| Specification | ✅ Complete | [spec.md](spec.md) |
| Research | ✅ Complete | [research.md](research.md) |
| Data Model | ✅ Complete | [data-model.md](data-model.md) |
| API Contracts | ✅ Complete | [contracts/storage-locations-api.md](contracts/storage-locations-api.md) |
| UI Contracts | ✅ Complete | [contracts/storage-dialog-ui.md](contracts/storage-dialog-ui.md), [contracts/storage-list-ui.md](contracts/storage-list-ui.md) |
| Quick Start | ✅ Complete | [quickstart.md](quickstart.md) |
| Constitution Check | ✅ PASS | All MUST principles compliant |

---

## Next Steps

### Phase 2: Task Generation

Run the following command to generate implementation tasks:

```bash
/speckit.tasks
```

This will create `tasks.md` with:
- Dependency-ordered task list
- Task descriptions & acceptance criteria
- Effort estimates
- Phase sequencing (backend → frontend → testing)

### Expected Task Categories

1. **Database & Entity Framework**
   - Add Room/Shelf DbSets to ItemDbContext
   - Create EF Core migrations
   - Apply migrations to database

2. **Backend API**
   - Implement RoomService business logic
   - Implement ShelfService business logic
   - Create RoomsController endpoints
   - Create ShelvesController endpoints
   - Add error handling & validation

3. **Frontend Components**
   - Build StorageLocationList.vue
   - Build RoomDialog.vue
   - Build ShelfDialog.vue
   - Implement state management (Pinia store)
   - Wire service calls

4. **Testing**
   - Write xUnit backend tests
   - Write contract tests for API
   - Write Vitest frontend component tests
   - Manual QA & acceptance testing

5. **Integration & Verification**
   - Run contract tests against API
   - Verify UI/API integration
   - Validate success criteria (SC-001 ～ SC-005)
   - Documentation update

---

## Architecture Decision Records (ADR)

### ADR-001: Soft Delete Implementation
- **Decision**: Use IsDeleted bool column in Room & Shelf tables
- **Rationale**: Aligns with existing Item soft-delete pattern, maintains audit trail
- **Implementation**: All queries filtered by `WHERE IsDeleted = 0`

### ADR-002: Duplicate Detection Scope
- **Decision**: Only check active (IsDeleted=false) records for duplicates
- **Rationale**: Allows re-registration of deleted room/shelf names
- **Implementation**: DB UNIQUE constraint with `WHERE IsDeleted = 0` filter + business logic check

### ADR-003: Room-Shelf Hierarchy
- **Decision**: Shelves are owned by Rooms (1:* cascade delete)
- **Rationale**: Shelf has no meaning without Room; deletion cascade is natural
- **Implementation**: Shelf.RoomId FK with ON DELETE CASCADE

### ADR-004: Item References (Optional)
- **Decision**: Item.RoomId and Item.ShelfId are nullable
- **Rationale**: Existing items may not have storage location assigned; supports gradual migration
- **Implementation**: Item → Room/Shelf FK with ON DELETE RESTRICT

### ADR-005: Onion Architecture (Backend)
- **Decision**: Backend implementation follows Onion Architecture (Dependency Inversion)
- **Rationale**: 
  - Core business logic isolated from infrastructure concerns
  - Testability: Service layer can be tested independently of Controller/Repository
  - Maintainability: Clear layer separation (Domain Models → Services → Repositories → DbContext)
  - Alignment with constitution principle (API-First: business logic in Backend)
- **Layer Structure**:
  - **Core Layer**: Domain entities (Room, Shelf, Item models)
  - **Service Layer**: Business logic (RoomService, ShelfService) - implements Result-based responses
  - **Repository Layer**: Data access abstraction (RoomRepository, ShelfRepository)
  - **Infrastructure Layer**: Entity Framework Core (DbContext, Migrations)
  - **Presentation Layer**: Controllers (RoomsController, ShelvesController) - handles HTTP concerns
- **Implementation**: Services depend on Repository interfaces (IRepository pattern); Repositories implement DbContext access

### ADR-006: Result Pattern for Service Layer
- **Decision**: Service methods return `Result<T>` type instead of throwing exceptions for business rule violations
- **Rationale**:
  - Explicit error handling: Caller explicitly handles Success/Failure cases
  - Prevents exception-based control flow: Business rule violations (duplicate name, item attachment) are not exceptions
  - Type-safe: Result<T> encapsulates success value or failure details (code, message)
  - Cleaner API contracts: Controllers can map Result to HTTP status codes (200, 400, 404, 409)
- **Implementation**:
  - Define `Result<T>` and `Error` types in `Common/Results/` namespace
  - Service methods: `Task<Result<RoomDto>> CreateRoomAsync(CreateRoomRequest request)`
  - Error codes: VALIDATION_ERROR (400), DUPLICATE_NAME (409), NOT_FOUND (404), CONFLICT (409)
  - Controller maps: `result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error)`

### ADR-007: Design Fidelity to Baseline HTML
- **Decision**: Frontend UI must reproduce the baseline design HTML for list and dialog screens.
- **Rationale**:
  - Prevent drift between approved design artifacts and implementation output
  - Reduce review ambiguity by using explicit source HTML mapping
  - Maintain consistent information architecture and visual language across pages
- **Implementation**:
  - `StorageManagement.vue` / `StorageLocationList.vue` are implemented against `design/storage_list.html`
  - `RoomDialog.vue` is implemented against `design/storage_register_room.html`
  - `ShelfDialog.vue` is implemented against `design/storage_register_shelf.html`
  - Add visual parity checklist and screenshot-based verification in test/polish tasks

---

## Completion Checklist

Before proceeding to implementation, verify:

- [ ] Spec is finalized with all clarifications
- [ ] Research questions are resolved
- [ ] Data model is approved by backend team
- [ ] API contract aligns with existing patterns
- [ ] UI contract follows design guidelines
- [ ] Constitution compliance verified
- [ ] Database migration strategy confirmed
- [ ] Team is familiar with quickstart guide

---


