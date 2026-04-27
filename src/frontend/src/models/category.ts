// フロントエンド: カテゴリーモデルと型定義

/**
 * カテゴリーエンティティ（フロントエンド用 DTO）
 */
export interface Category {
  id: string; // UUID
  name: string; // ユーザー表示用名称
  normalizedName: string; // 正規化名（重複判定用）
  icon: string | null; // Material Symbols Outlined アイコン名
  color: string | null; // 16進カラーコード（例: #FF6B6B）
  isReserved: boolean; // 予約カテゴリフラグ
  createdAt: string; // ISO8601 UTC（例: 2026-04-26T12:00:00Z）
  updatedAt: string; // ISO8601 UTC（例: 2026-04-26T12:00:00Z）
}

/**
 * カテゴリー作成リクエスト
 */
export interface CreateCategoryRequest {
  name: string;
  icon: string;
  color: string;
}

/**
 * カテゴリー更新リクエスト
 */
export interface UpdateCategoryRequest {
  name: string;
  icon: string;
  color: string;
}

/**
 * 定義済みアイコン候補
 */
export const CATEGORY_ICONS = [
  { value: 'restaurant', label: '食器' },
  { value: 'book', label: '本' },
  { value: 'home', label: '家' },
  { value: 'directions_car', label: '車' },
  { value: 'shopping_bag', label: '買い物' },
  { value: 'favorite', label: 'お気に入り' },
  { value: 'work', label: '仕事' },
  { value: 'sports_soccer', label: 'スポーツ' },
  { value: 'checkroom', label: '衣類' },
  { value: 'health_and_safety', label: '医療' },
  { value: 'computer', label: 'デジタル' },
  { value: 'landscape', label: '屋外' },
] as const;

/**
 * 定義済みカラー候補
 */
export const CATEGORY_COLORS = [
  { value: '#FF6B6B', label: '赤' },
  { value: '#4ECDC4', label: '青緑' },
  { value: '#45B7D1', label: '水色' },
  { value: '#FFA07A', label: '橙' },
  { value: '#98D8C8', label: '緑' },
  { value: '#F7DC6F', label: '黄' },
  { value: '#BB8FCE', label: '紫' },
  { value: '#85C1E2', label: 'スカイブルー' },
  { value: '#F8B88B', label: '茶' },
  { value: '#A8D8EA', label: 'ライトブルー' },
  { value: '#AA96DA', label: 'ラベンダー' },
  { value: '#FCBAD3', label: 'ピンク' },
] as const;

/**
 * 予約カテゴリ: 未分類
 */
export const UNCLASSIFIED_CATEGORY_ID = '550e8400-e29b-41d4-a716-446655440000';

export const UNCLASSIFIED_CATEGORY: Category = {
  id: UNCLASSIFIED_CATEGORY_ID,
  name: '未分類',
  normalizedName: '未分類',
  icon: null,
  color: null,
  isReserved: true,
  createdAt: '2026-04-01T00:00:00Z',
  updatedAt: '2026-04-01T00:00:00Z',
};

/**
 * カテゴリー一覧の昇順ソート関数
 */
export function sortCategoriesByName(categories: Category[]): Category[] {
  return [...categories].sort((a, b) =>
    a.normalizedName.localeCompare(b.normalizedName, 'ja')
  );
}

/**
 * 指定カテゴリーが編集・削除可能か判定
 */
export function isEditableCategory(category: Category): boolean {
  return !category.isReserved;
}

/**
 * 指定カテゴリーが削除可能か判定
 */
export function isDeletableCategory(category: Category): boolean {
  return !category.isReserved;
}
