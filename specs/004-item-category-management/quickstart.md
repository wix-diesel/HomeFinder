# Quickstart: 物品カテゴリー管理

この手順は、現在の実装（US1-US3）をローカルで起動・検証するための最短手順です。

## 前提

- .NET 10 SDK
- Node.js 18+

## 起動

### 1. バックエンド

```bash
cd src/backend
dotnet restore
dotnet run
```

### 2. フロントエンド

```bash
cd src/frontend
npm install
npm run dev
```

## 手動確認シナリオ

### 1. 設定画面から遷移

1. `/settings` を開く
2. `カテゴリー管理` 項目をクリック
3. `/categories` へ遷移することを確認

### 2. 一覧表示（US1）

1. `/categories` を開く
2. ローディング -> 一覧または空状態へ遷移
3. 一覧はカテゴリー名の昇順

### 3. 追加（US2）

1. `カテゴリーを追加` を押す
2. 名称・アイコン・カラーを選択
3. 保存後に一覧へ追加される

### 4. 編集（US3）

1. 非予約カテゴリの `編集` を押す
2. 既存値が初期表示される
3. 保存後に一覧が更新される

### 5. 削除（US3）

1. 非予約カテゴリの `削除` を押す
2. 確認 UI で `削除する`
3. 一覧から対象カテゴリが消える

### 6. 予約カテゴリ保護（US3）

1. 予約カテゴリ `未分類` は編集/削除不可
2. API では 403 (`RESERVED_CATEGORY_PROTECTED`) を返す

## テスト実行

### フロントエンド

```bash
cd src/frontend
npm run test:run -- tests/unit/pages/SettingsPageCategoryNavigation.spec.ts tests/unit/pages/CategoryManagementPage.spec.ts tests/unit/components/CategoryDialogCreate.spec.ts tests/unit/pages/CategoryCreateErrorState.spec.ts tests/unit/components/CategoryDialogEdit.spec.ts tests/unit/pages/CategoryDeleteFlow.spec.ts
```

### バックエンド（契約）

```bash
cd src
dotnet test tests/contract/contract.csproj --filter "FullyQualifiedName~CategoriesApiContractTests"
dotnet test tests/contract/contract.csproj --filter "FullyQualifiedName~CategoriesCreateContractTests"
dotnet test tests/contract/contract.csproj --filter "FullyQualifiedName~CategoriesUpdateContractTests"
dotnet test tests/contract/contract.csproj --filter "FullyQualifiedName~CategoriesDeleteContractTests"
```

### バックエンド（統合）

```bash
cd src
dotnet test tests/integration/integration.csproj --filter "FullyQualifiedName~CategoriesListIntegrationTests"
dotnet test tests/integration/integration.csproj --filter "FullyQualifiedName~CategoriesCreateConflictIntegrationTests"
dotnet test tests/integration/integration.csproj --filter "FullyQualifiedName~CategoryDeleteReassignIntegrationTests"
dotnet test tests/integration/integration.csproj --filter "FullyQualifiedName~ReservedCategoryProtectionIntegrationTests"
```

## 注意

- `dotnet test --filter` は正規表現の括弧や `|` を使わず、個別指定で実行する
- 既知警告 `NU1903`（依存脆弱性）は現状の機能検証には影響しない
