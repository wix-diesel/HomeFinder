# Implementation Tasks: 部屋・棚管理

**Feature**: 005-storage-location-management  
**Branch**: `005-item-category-management`  
**Date**: 2026-04-27  
**Status**: Ready for Implementation

**Reference**: [spec.md](spec.md) | [plan.md](plan.md) | [data-model.md](data-model.md) | [contracts/](contracts/)

---

## Overview

以下の 3 つのユーザーストーリーを、依存順に実装します。

| Story | Title | Priority | Phase | MVP Scope |
|-------|-------|----------|-------|-----------|
| US1 | 部屋と棚の一覧確認 | P1 | 3 | ✅ MVP 必須 |
| US2 | 部屋と棚の追加・編集 | P2 | 4 | ✅ MVP 必須 |
| US3 | 部屋と棚の削除 | P3 | 5 | ⚠️ MVP 後追加可 |

---

## Phase 1: Setup

### 目的
プロジェクト初期化・基盤構築

### 独立テスト基準
プロジェクト構造が確立され、ビルド・テスト実行が可能な状態

---

- [X] T001 [P] プロジェクト構造を確認・作成 (`src/backend`, `src/frontend`, `src/tests` 下のディレクトリ)
- [X] T002 [P] Backend NuGet パッケージを確認・追加 (EntityFrameworkCore, xUnit など)
- [X] T003 [P] Frontend npm 依存パッケージを確認・追加 (Vue 3, axios, Pinia など)
- [X] T004 Backend `.gitignore` に `bin/`, `obj/`, `.vs/` を追加
- [X] T005 Frontend `.gitignore` に `node_modules/`, `dist/` を追加

---

## Phase 2: Foundational (Database & Entity Framework)

### 目的
Database スキーマ・EF Core entity・Repository パターンを確立（全 US が依存）

### 独立テスト基準
- Room・Shelf entity が ItemDbContext に DbSet として登録
- Migration 適用後、Room・Shelf テーブルが DB に作成
- 既存 Item テーブルに roomId・shelfId FK 列が追加

---

- [X] T006 Backend: Room entity を作成 (`src/backend/Models/Room.cs`) - 属性: Id, Name, Description, IsDeleted, CreatedAt, UpdatedAt, Shelves 

- [X] T007 Backend: Shelf entity を作成 (`src/backend/Models/Shelf.cs`) - 属性: Id, RoomId (FK), Name, Description, IsDeleted, CreatedAt, UpdatedAt

- [X] T008 Backend: Item entity を更新 (`src/backend/Models/Item.cs`) - 属性追加: RoomId?, ShelfId?, Room navigation, Shelf navigation

- [X] T009 Backend: ItemDbContext を更新 (`src/backend/Data/ItemDbContext.cs`) - DbSet<Room>, DbSet<Shelf> 追加、OnModelCreating で制約・FK 設定

- [X] T010 Backend: 初期 Migration を作成・適用 (`dotnet ef migrations add AddRoomShelfEntities` → `dotnet ef database update`)

- [X] T011 Backend: RoomRepository を実装 (`src/backend/Repositories/RoomRepository.cs`) - メソッド: ListActiveRoomsWithShelvesAsync, GetRoomByIdAsync, CreateRoomAsync, UpdateRoomAsync, SoftDeleteRoomAsync, CheckDuplicateNameAsync

- [X] T012 Backend: ShelfRepository を実装 (`src/backend/Repositories/ShelfRepository.cs`) - メソッド: ListActiveShelvesAsync, GetShelfByIdAsync, CreateShelfAsync, UpdateShelfAsync, SoftDeleteShelfAsync, CheckDuplicateNameAsync, CheckItemAttachmentAsync

- [X] T013 Backend: RoomService を実装 (`src/backend/Services/RoomService.cs`) - メソッド: ListRoomsWithShelvesAsync, CreateRoomAsync, UpdateRoomAsync, DeleteRoomAsync (with item check), 重複チェック ロジック

- [X] T014 Backend: ShelfService を実装 (`src/backend/Services/ShelfService.cs`) - メソッド: ListShelvesAsync, CreateShelfAsync, UpdateShelfAsync, DeleteShelfAsync (with item check), 重複チェック ロジック

- [X] T015 Backend: Error handling・validation utilities を作成 (`src/backend/Common/Errors/ValidationException.cs`, `ConflictException.cs`)

---

## Phase 3: User Story 1 - 部屋と棚の一覧確認 (P1)

### 目的
登録済みの部屋・棚を一覧表示し、管理画面として最小価値を実現

### 依存
Phase 2 完了（Repository・Service 実装済み）

### 独立テスト基準
- API: GET /api/rooms が部屋・棚の一覧を JSON で返す
- Frontend: StorageLocationList が一覧を表示、Room 展開で棚一覧表示
- 空状態メッセージが表示される

---

### Backend Tasks

- [X] T016 [P] [US1] RoomsController を作成 (`src/backend/Controllers/RoomsController.cs`) - メソッド: GET /api/rooms (list all) - レスポンス: { rooms: [] with shelves nested }

- [X] T017 [P] [US1] API レスポンス DTO を作成 (`src/backend/Contracts/RoomDto.cs`, `ShelfDto.cs`) - 属性: id, name, description, createdAt, updatedAt, shelves (nested)

- [X] T018 [US1] Contract test を作成 (`src/tests/contract/StorageLocationsApiContract.cs`) - テスト: GET /api/rooms returns 200 with correct schema, empty list when no rooms exist

- [X] T019 [P] [US1] Backend: List API のエラーハンドリング (500 on DB error)

### Frontend Tasks

- [X] T020 [P] [US1] roomService を実装 (`src/frontend/src/services/roomService.ts`) - メソッド: listRooms() - API 呼び出し、エラー処理

- [X] T021 [P] [US1] shelfService を実装 (`src/frontend/src/services/shelfService.ts`) - メソッド: listShelves(roomId), utility functions

- [X] T022 [P] [US1] storageStore を実装 (`src/frontend/src/stores/storageStore.ts`) - State: rooms, isLoading, error - Actions: fetchRooms(), setError(), clearError()

- [X] T023 [P] [US1] StorageLocationList.vue を実装 (`src/frontend/src/components/StorageLocationList.vue`) - Props: rooms, isLoading, error - Events: room-add, room-edit, room-delete, shelf-add, shelf-edit, shelf-delete - 機能: Room 展開/折畳み, 空状態メッセージ

- [X] T024 [P] [US1] StorageManagement.vue を実装 (`src/frontend/src/pages/StorageManagement.vue`) - StorageLocationList をマウント、storageStore から rooms 取得、画面読み込み時に fetchRooms()

- [ ] T024a [US1] デザイン再現: `design/storage_list.html` を基準に一覧ページのレイアウト・タイポグラフィ・ナビゲーション構成を再現（Header/Sidebar/Main/Mobile Nav）

- [ ] T024b [US1] デザイン一致検証: `design/storage_list.html` と実装画面の目視差分チェック（余白/色/フォント/アイコン/状態）

- [X] T025 [US1] Frontend コンポーネント test を作成 (`src/frontend/tests/StorageLocationList.test.ts`) - テスト: List rendering, room expansion, empty state display

- [ ] T026 [US1] E2E 受け入れテスト仕様を定義 (`docs/e2e/US1-list-rooms-shelves.md`) - シナリオ: データ存在時表示、空状態メッセージ

### Validation & Metrics

- [ ] T027 [manual] [US1] SC-001 検証: 利用者 90% が初回 3 分以内に部屋・棚を特定 (manual test)

---

## Phase 4: User Story 2 - 部屋と棚の追加・編集 (P2)

### 目的
部屋・棚の作成・更新を実現し、情報の日常メンテナンスを可能にする

### 依存
Phase 2 完了、Phase 3 完了（List 機能が前提）

### 独立テスト基準
- API: POST /api/rooms (create), PUT /api/rooms/{id} (update) が 201/200 を返す
- Frontend: RoomDialog・ShelfDialog でフォーム入力・保存が可能
- 重複名称でエラー表示 (409)
- 空フィールドでエラー表示 (400)

---

### Backend Tasks

- [X] T028 [P] [US2] Request DTO を作成 (`src/backend/Contracts/CreateRoomRequest.cs`, `UpdateRoomRequest.cs`, etc) - 属性: name (50 char max), description (200 char max, required)

- [X] T029 [P] [US2] RoomsController に POST /api/rooms endpoint を追加 - リクエスト検証、RoomService.CreateRoomAsync() 呼び出し、201 レスポンス

- [X] T030 [P] [US2] RoomsController に PUT /api/rooms/{id} endpoint を追加 - リクエスト検証、RoomService.UpdateRoomAsync() 呼び出し、200 レスポンス、404 handling

- [X] T031 [P] [US2] ShelvesController を作成 (`src/backend/Controllers/ShelvesController.cs`) - メソッド: POST /api/rooms/{roomId}/shelves (create), PUT /api/rooms/{roomId}/shelves/{id} (update)

- [X] T032 [P] [US2] Backend: 重複名称チェック logic を RoomService/ShelfService に実装 - query by name (active records only) → 409 Conflict レスポンス

- [X] T033 [P] [US2] Backend: 入力値検証 - MaxLength, Required attribute チェック → 400 Bad Request

- [X] T034 [US2] Backend: 重複・必須フィールドの Contract test を追加 (`src/tests/contract/StorageLocationsApiContract.cs`)

- [X] T035 [US2] Integration test を作成 (`src/tests/integration/RoomServiceTests.cs`) - テスト: CreateRoom success, duplicate name error, invalid input error

- [X] T036 [US2] Integration test を作成 (`src/tests/integration/ShelfServiceTests.cs`) - テスト: CreateShelf success, duplicate name per room, room not found error

### Frontend Tasks

- [X] T037 [P] [US2] roomService に createRoom(), updateRoom() メソッドを追加 - API POST/PUT 呼び出し、エラー処理

- [X] T038 [P] [US2] shelfService に createShelf(), updateShelf() メソッドを追加

- [X] T039 [P] [US2] RoomDialog.vue を実装 (`src/frontend/src/components/RoomDialog.vue`) - Props: modelValue, mode (create/edit), room, isLoading, error - Events: update:modelValue, save - フィールド: name (text, max 50), description (textarea, max 200)

- [X] T040 [P] [US2] ShelfDialog.vue を実装 (`src/frontend/src/components/ShelfDialog.vue`) - Props: modelValue, mode, shelf, roomId, rooms, isLoading, error - Events: update:modelValue, save - フィールド: room (dropdown), name, description

- [ ] T040a [US2] デザイン再現: `design/storage_register_room.html` を基準に RoomDialog のヘッダー/フォーム/補助情報/フッター構成を再現

- [ ] T040b [US2] デザイン再現: `design/storage_register_shelf.html` を基準に ShelfDialog のヘッダー/フォーム/フッター構成を再現

- [ ] T040c [US2] デザイン一致検証: 2ダイアログを基準HTMLと比較し、スクラム・角丸・余白・ボタン階層・入力状態を確認

- [X] T041 [P] [US2] storageStore に createRoom(), updateRoom(), createShelf(), updateShelf() actions を追加

- [X] T042 [US2] DeleteConfirmDialog.vue を実装 (`src/frontend/src/components/DeleteConfirmDialog.vue`) - Props: title, message - Events: confirm, cancel

- [X] T043 [US2] StorageLocationList に room-add, room-edit, shelf-add, shelf-edit event handlers を追加

- [X] T044 [P] [US2] StorageManagement.vue に dialog state・event handlers を追加

- [X] T045 [US2] Frontend コンポーネント test を作成 (`src/frontend/tests/RoomDialog.test.ts`, `ShelfDialog.test.ts`) - テスト: form rendering, input validation, error display

- [X] T046 [US2] Service mock test を作成 (`src/frontend/tests/services.test.ts`) - テスト: createRoom API call, error handling

- [ ] T047 [US2] E2E 受け入れテスト仕様を定義 (`docs/e2e/US2-add-edit-rooms-shelves.md`) - シナリオ: 正常系作成・編集、重複エラー、必須フィールド空エラー

### Validation & Metrics

- [ ] T048 [manual] [US2] SC-002 検証: 95% 以上の操作が 30 秒以内に完了 (performance test)

- [ ] T049 [US2] SC-003 検証: 重複登録・紐づきあり削除が 100% 拒否 (contract test)

---

## Phase 5: User Story 3 - 部屋と棚の削除 (P3)

### 目的
不要になった部屋・棚を削除し、管理情報を整理できるようにする

### 依存
Phase 2 完了、Phase 4 完了（追加・編集と削除は操作フロー上の連続）

### 独立テスト基準
- API: DELETE /api/rooms/{id} が 204 を返す（アイテム未紐づけ）
- API: DELETE /api/rooms/{id} が 409 を返す（アイテム紐づけあり）
- Frontend: 削除確認 dialog が表示・実行される
- 削除後、一覧から消える

---

### Backend Tasks

- [X] T050 [P] [US3] RoomsController に DELETE /api/rooms/{id} endpoint を追加 - RoomService.DeleteRoomAsync() 呼び出し、item 紐づきチェック → 409 or 204 レスポンス

- [X] T051 [P] [US3] ShelvesController に DELETE /api/rooms/{roomId}/shelves/{id} endpoint を追加

- [X] T052 [P] [US3] RoomService.DeleteRoomAsync() に item attachment チェックを実装 - query Items.Where(i => i.RoomId == roomId && !i.IsDeleted) → throw ConflictException

- [X] T053 [P] [US3] ShelfService.DeleteShelfAsync() に item attachment チェックを実装

- [X] T054 [P] [US3] Backend: 論理削除実装の検証 - Room.IsDeleted = true, UpdatedAt 更新

- [X] T055 [US3] Backend: Item 紐づき削除拒否の Contract test を追加

- [X] T056 [US3] Integration test を作成 (`src/tests/integration/RoomDeletionTests.cs`) - テスト: Delete success (no items), conflict error (items exist), soft delete verification

### Frontend Tasks

- [ ] T057 [P] [US3] roomService に deleteRoom()、shelfService に deleteShelf() メソッドを追加 - API DELETE 呼び出し

- [X] T058 [P] [US3] storageStore に deleteRoom(), deleteShelf() actions を追加・error handling を実装

- [X] T059 [P] [US3] StorageLocationList に room-delete, shelf-delete event handlers を追加

- [X] T060 [US3] StorageManagement.vue に delete イベント処理を追加 - delete API 呼び出し → 成功時に fetchRooms()

- [X] T061 [US3] DeleteConfirmDialog に confirm ボタン click → deleteRoom/deleteShelf() action dispatch を追加

- [X] T062 [US3] Frontend コンポーネント test を作成 (`src/frontend/tests/DeleteConfirm.test.ts`) - テスト: dialog rendering, confirm/cancel actions

- [ ] T063 [US3] E2E 受け入れテスト仕様を定義 (`docs/e2e/US3-delete-rooms-shelves.md`) - シナリオ: 正常系削除、item 紐づき delete 拒否

### Validation & Metrics

- [ ] T064 [US3] SC-003 検証: 紐づきあり削除が 100% 拒否 (integration test)

---

## Phase 6: Polish & Cross-Cutting Concerns

### 目的
エラーハンドリング・ローディング状態・UI UX・ドキュメント・本番対応

### 独立テスト基準
- すべてのエラーシナリオが適切にハンドル（4xx, 5xx）
- ローディング中は UI が disabled
- 通信失敗時は再試行導線が表示
- ドキュメント・Quickstart が最新

---

### Error Handling & Resilience

- [ ] T065 [P] Frontend: API 通信失敗時のエラー表示・再試行 UI - error message 表示, retry button

- [ ] T066 Backend: 500 error logging・監視対応 - Log to console/file, exception details

- [ ] T067 Frontend: 予期しないエラー時の generic メッセージ表示

- [ ] T068 Frontend: Network timeout ハンドリング (axios timeout 設定)

### Loading & Performance

- [ ] T069 [P] Frontend: isLoading state の全操作への適用 - fetch時, post時, delete時 → buttons/inputs disabled

- [ ] T070 Frontend: Loading spinner/progress indicator の UI 追加

- [ ] T071 Backend: List API のパフォーマンス検証 (<500ms p95)

### UI/UX Polish

- [ ] T072 Frontend: Keyboard navigation・accessibility 対応 (ARIA labels, tab order)

- [ ] T073 Frontend: Dialog の ESC key close 対応

- [ ] T074 Frontend: 入力フィールドに remaining character count 表示 (50/50, 200/200)

- [ ] T075 Frontend: Empty state message の UI polish

- [ ] T076 Frontend: Error message の i18n 対応・日本語化完了確認

- [ ] T076a Frontend: ベースラインHTML再現チェックリストを作成し、`design/storage_list.html`/`design/storage_register_room.html`/`design/storage_register_shelf.html` との一致を記録

### Documentation & QA

- [ ] T077 [P] Quickstart.md の実行確認・更新 (npm run dev, dotnet run 成功確認)

- [ ] T078 Backend API documentation を生成 (Swagger/OpenAPI) または README に手動記載

- [ ] T079 Frontend component props・events documentation を JSDoc で記述

- [ ] T080 Manual QA checklist を作成・実行 (spec の all scenarios カバー)

### Success Criteria Validation

- [ ] T081 [manual] SC-001 最終検証: 利用者テスト or デモで 3 分以内に目的達成確認

- [ ] T082 [manual] SC-002 最終検証: 操作 95%以上が 30 秒以内に完了 (performance monitor)

- [ ] T083 [auto] SC-003 最終検証: 重複・紐づき削除が 100% 拒否 (test report)

- [ ] T084 [auto] SC-004 最終検証: 文字数超過が 100% 検出 (test report)

---

## Post-Launch Success Metrics

### 目的

リリース後のビジネス成果を測定（MVP 後）

### 事後測定タスク

- [ ] T085 [post-launch] SC-005 計測: 誤登録問い合わせ減少率 (現行比 30%以上) - リリース後 1 か月以内に baseline との比較を実施

---

### Code Quality & Maintenance

- [ ] T086 Code review: Backend implementation (RoomService, ShelfService logic)

- [ ] T087 Code review: Frontend implementation (components, stores)

- [ ] T088 Unit test coverage: Backend ≥ 80%

- [ ] T089 Component test coverage: Frontend ≥ 70%

- [ ] T090 Integration test: End-to-end flow (Backend + Frontend) validation

---

## Dependencies & Execution Order

### Dependency Graph

```
Phase 1 (Setup)
    ↓
Phase 2 (DB/EF Core Foundation)
    ├→ Phase 3 (US1 List) [US1 MVP]
    │   ↓
    ├→ Phase 4 (US2 Add/Edit) [US2 MVP]
    │   ↓
    └→ Phase 5 (US3 Delete) [US3 Post-MVP]
        ↓
Phase 6 (Polish & QA)
```

### Parallel Execution Opportunities

**Phase 3 内**:
- T016 (Backend GET endpoint) と T020 (Frontend roomService) は並行可
- T023 (StorageLocationList component) と T026 (test) は並行可

**Phase 4 内**:
- Backend POST/PUT と Frontend RoomDialog/ShelfDialog は並行可
- Contract test (T034) と Integration test (T035/T036) は並行可

**Phase 5 内**:
- DELETE endpoint (T050/T051) と Frontend delete logic (T057/T058) は並行可

---

## Task Estimation

| Phase | Task Count | Effort (dev-hours) | Dependencies |
|-------|-----------|-------------------|-------------|
| 1 | 5 | 2 | None |
| 2 | 10 | 16 | Phase 1 |
| 3 | 12 | 20 | Phase 2 |
| 4 | 20 | 32 | Phase 2, 3 |
| 5 | 14 | 20 | Phase 2, 4 |
| 6 | 26 | 24 | All previous phases |
| **Total** | **87** | **114** | - |

---

## MVP Scope Definition

**Minimum Viable Product (Release v1.0)**:
- Phase 1: ✅ Complete
- Phase 2: ✅ Complete
- Phase 3 (US1): ✅ Complete
- Phase 4 (US2): ✅ Complete
- Phase 5 (US3): ⚠️ Optional (post-MVP)
- Phase 6: 部分実装（基本的なエラーハンドリング + documentation）

**MVP Release Criteria**:
- US1・US2 が AC 100% 達成
- Backend unit test coverage ≥ 80%
- Frontend component test coverage ≥ 70%
- Contract test 全 PASS
- Quickstart 実行可能
- 本番環境へのデプロイ準備完了

---

## Next Steps

1. ✅ Specification Complete
2. ✅ Plan Complete
3. ✅ Tasks Complete
4. **→ Ready for Implementation** (Phase 1 から開始)

```bash
# 推奨実行順序
# Phase 1
- [ ] T001 ～ T005

# Phase 2
- [ ] T006 ～ T015

# Phase 3
- [ ] T016 ～ T027

# Phase 4
- [ ] T028 ～ T049

# Phase 5
- [ ] T050 ～ T064

# Phase 6
- [ ] T065 ～ T090
```

