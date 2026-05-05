import { ref } from 'vue';

/**
 * 画像操作用の通知（スナックバー＋詳細エラーモーダル）コンポーザブル
 * 成功：緑色スナックバー、3秒表示
 * エラー：赤色スナックバー、3秒表示
 * 詳細エラー：モーダルダイアログで title + 詳細テキストを表示
 */
export function useImageNotification() {
  // スナックバー用
  const message = ref('');
  const isVisible = ref(false);
  const isError = ref(false);
  let timer: ReturnType<typeof setTimeout> | null = null;

  // 詳細エラーモーダル用
  const isModalVisible = ref(false);
  const modalTitle = ref('');
  const modalDetail = ref('');

  function showSuccess(text: string) {
    _show(text, false);
  }

  function showError(text: string) {
    _show(text, true);
  }

  /**
   * 詳細エラーをモーダルダイアログで表示する。
   * 同時にスナックバーでも簡易通知を行う。
   */
  function showDetailedError(title: string, detail: string) {
    modalTitle.value = title;
    modalDetail.value = detail;
    isModalVisible.value = true;
    // スナックバーにも短いタイトルを表示する
    _show(title, true);
  }

  function dismissModal() {
    isModalVisible.value = false;
    modalTitle.value = '';
    modalDetail.value = '';
  }

  function _show(text: string, error: boolean, durationMs = 3000) {
    message.value = text;
    isError.value = error;
    isVisible.value = true;

    if (timer !== null) clearTimeout(timer);
    timer = setTimeout(() => {
      isVisible.value = false;
    }, durationMs);
  }

  function dismiss() {
    isVisible.value = false;
    if (timer !== null) clearTimeout(timer);
  }

  return {
    message, isVisible, isError,
    isModalVisible, modalTitle, modalDetail,
    showSuccess, showError, showDetailedError,
    dismiss, dismissModal,
  };
}
