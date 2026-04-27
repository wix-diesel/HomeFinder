// 設定画面の項目アクション種別（本機能では表示のみ）
export type SettingsItemActionType = 'display_only' | 'navigation';

// 設定画面の個別項目表示モデル
export interface SettingsItemViewModel {
  itemId: string;
  labelJa: string;
  descriptionJa: string | null;
  iconName: string;
  actionType: SettingsItemActionType;
  isInteractive: boolean;
  navigationRoute?: string; // actionType が 'navigation' の場合、遷移先ルート
}

// 設定画面のセクション表示モデル
export interface SettingsSectionViewModel {
  sectionId: string;
  headingJa: string;
  items: SettingsItemViewModel[];
}

// 設定画面全体の表示モデル
export interface SettingsPageViewModel {
  titleJa: string;
  sections: SettingsSectionViewModel[];
  footerNoteJa: string | null;
}

/**
 * カテゴリー管理導線の表示モデルを生成
 */
export function createCategoryManagementItem(): SettingsItemViewModel {
  return {
    itemId: 'category_management',
    labelJa: 'カテゴリー管理',
    descriptionJa: '物品のカテゴリーを追加・編集・削除します',
    iconName: 'category',
    actionType: 'navigation',
    isInteractive: true,
    navigationRoute: '/categories',
  };
}
