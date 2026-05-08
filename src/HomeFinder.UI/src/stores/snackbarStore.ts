import { defineStore } from 'pinia';
import { ref } from 'vue';

/**
 * アプリ全体で共有するスナックバー（トースト）通知ストア
 * apiClient.ts などの非コンポーネントコードからもトーストを表示できるようにする
 */
export const useSnackbarStore = defineStore('snackbar', () => {
  const message = ref('');
  const isError = ref(false);
  const isVisible = ref(false);
  let hideTimer: ReturnType<typeof setTimeout> | null = null;

  /**
   * トースト通知を表示する
   * @param text 表示するメッセージ
   * @param error エラーメッセージの場合 true（赤色で表示）
   * @param durationMs 自動消去までのミリ秒（デフォルト: 4000ms = SC-003 要件を満たす）
   */
  function show(text: string, error: boolean = false, durationMs: number = 4000) {
    message.value = text;
    isError.value = error;
    isVisible.value = true;

    if (hideTimer !== null) {
      clearTimeout(hideTimer);
    }
    hideTimer = setTimeout(() => {
      isVisible.value = false;
    }, durationMs);
  }

  function hide() {
    isVisible.value = false;
  }

  return { message, isError, isVisible, show, hide };
});
