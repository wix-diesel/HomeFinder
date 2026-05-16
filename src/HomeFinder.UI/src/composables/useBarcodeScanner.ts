import { computed, onBeforeUnmount, ref } from 'vue';

type LookupHandler<T> = (signal: AbortSignal) => Promise<T>;

type BarcodeDetectorLike = {
  detect: (source: HTMLVideoElement) => Promise<Array<{ rawValue?: string }>>;
};

type BarcodeDetectorCtor = new (options?: { formats?: string[] }) => BarcodeDetectorLike;

type CameraStartOptions = {
  onDetected: (value: string) => void;
  onError: (message: string) => void;
};

export function useBarcodeScanner(cooldownMs = 500) {
  const isScanning = ref(false);
  const isSearching = ref(false);
  const cooldownUntil = ref<number>(0);
  const videoElement = ref<HTMLVideoElement | null>(null);

  let stream: MediaStream | null = null;
  let activeLookupController: AbortController | null = null;
  let detectTimerId: number | null = null;
  let cooldownTimerId: number | null = null;

  const isCooldown = computed(() => {
    return cooldownUntil.value > Date.now();
  });

  const remainingCooldownMs = computed(() => {
    if (!isCooldown.value) return 0;
    return Math.max(0, cooldownUntil.value - Date.now());
  });

  function releaseStream() {
    if (stream) {
      stream.getTracks().forEach((track) => track.stop());
      stream = null;
    }

    if (videoElement.value) {
      videoElement.value.srcObject = null;
    }
  }

  function stopScanLoop() {
    if (detectTimerId != null) {
      window.clearTimeout(detectTimerId);
      detectTimerId = null;
    }
  }

  function stopCamera() {
    stopScanLoop();
    releaseStream();
    isScanning.value = false;
  }

  async function startCamera(target: HTMLVideoElement, options: CameraStartOptions) {
    videoElement.value = target;

    if (!navigator.mediaDevices?.getUserMedia) {
      options.onError('このブラウザではカメラを利用できません。');
      return;
    }

    try {
      stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      });
      target.srcObject = stream;
      await target.play();
      isScanning.value = true;
    } catch {
      options.onError('カメラの利用が許可されていません。');
      stopCamera();
      return;
    }

    const detectorCtor = (window as unknown as { BarcodeDetector?: BarcodeDetectorCtor }).BarcodeDetector;
    if (!detectorCtor) {
      options.onError('このブラウザはバーコード読み取りに対応していません。');
      return;
    }

    const detector = new detectorCtor({ formats: ['ean_13', 'ean_8'] });

    const detect = async () => {
      if (!isScanning.value || !videoElement.value) return;

      try {
        const barcodes = await detector.detect(videoElement.value);
        const firstValue = barcodes.find((item) => typeof item.rawValue === 'string')?.rawValue?.trim();
        if (firstValue) {
          options.onDetected(firstValue);
          stopCamera();
          return;
        }
      } catch {
        options.onError('バーコードの読み取りに失敗しました。');
      }

      detectTimerId = window.setTimeout(detect, 250);
    };

    detectTimerId = window.setTimeout(detect, 100);
  }

  async function executeLatestLookup<T>(handler: LookupHandler<T>): Promise<T | null> {
    if (isCooldown.value) {
      return null;
    }

    activeLookupController?.abort();

    const controller = new AbortController();
    activeLookupController = controller;
    isSearching.value = true;

    try {
      const result = await handler(controller.signal);
      if (activeLookupController !== controller) {
        return null;
      }
      return result;
    } finally {
      if (activeLookupController === controller) {
        activeLookupController = null;
      }
      cooldownUntil.value = Date.now() + cooldownMs;
      if (cooldownTimerId != null) {
        window.clearTimeout(cooldownTimerId);
      }
      cooldownTimerId = window.setTimeout(() => {
        cooldownUntil.value = 0;
      }, cooldownMs);
      isSearching.value = false;
    }
  }

  onBeforeUnmount(() => {
    activeLookupController?.abort();
    if (cooldownTimerId != null) {
      window.clearTimeout(cooldownTimerId);
      cooldownTimerId = null;
    }
    stopCamera();
  });

  return {
    isScanning,
    isSearching,
    isCooldown,
    remainingCooldownMs,
    startCamera,
    stopCamera,
    executeLatestLookup,
  };
}
