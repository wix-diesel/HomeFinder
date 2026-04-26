// 設定画面の項目アクション種別（本機能では表示のみ）
export type SettingsItemActionType = 'display_only';

// 設定画面の個別項目表示モデル
export interface SettingsItemViewModel {
  itemId: string;
  labelJa: string;
  descriptionJa: string | null;
  iconName: string;
  actionType: SettingsItemActionType;
  isInteractive: boolean;
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
