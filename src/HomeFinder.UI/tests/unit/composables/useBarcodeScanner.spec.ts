import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useBarcodeScanner } from '../../../src/composables/useBarcodeScanner';

const {
  decodeFromVideoElementMock,
  BrowserMultiFormatReaderMock,
} = vi.hoisted(() => {
  const decodeMock = vi.fn();
  class BrowserMultiFormatReaderFake {
    possibleFormats: unknown[] = [];
    decodeFromVideoElement = decodeMock;
  }

  return {
    decodeFromVideoElementMock: decodeMock,
    BrowserMultiFormatReaderMock: vi.fn(BrowserMultiFormatReaderFake),
  };
});

vi.mock('@zxing/browser', () => ({
  BarcodeFormat: {
    EAN_13: 'EAN_13',
    EAN_8: 'EAN_8',
  },
  BrowserMultiFormatReader: BrowserMultiFormatReaderMock,
}));

describe('useBarcodeScanner', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    decodeFromVideoElementMock.mockReset();
  });

  it('直前検索から 500ms 未満はクールダウン状態になる', async () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-01-01T00:00:00Z'));

    const scanner = useBarcodeScanner(500);

    const result = await scanner.executeLatestLookup(async () => 'ok');
    expect(result).toBe('ok');
    expect(scanner.isCooldown.value).toBe(true);

    vi.advanceTimersByTime(600);
    expect(scanner.isCooldown.value).toBe(false);

    vi.useRealTimers();
  });

  it('新しい検索実行時に前回検索をキャンセルする', async () => {
    const scanner = useBarcodeScanner(500);
    let firstCanceled = false;

    const first = scanner.executeLatestLookup(
      async (signal) =>
        await new Promise<string>((_resolve, reject) => {
          signal.addEventListener('abort', () => {
            firstCanceled = true;
            reject(new DOMException('Aborted', 'AbortError'));
          });
        }),
    );

    const second = scanner.executeLatestLookup(async () => 'latest');

    await expect(first).rejects.toBeInstanceOf(DOMException);
    await expect(second).resolves.toBe('latest');
    expect(firstCanceled).toBe(true);
  });

  it('カメラ起動後に検出成功で onDetected を呼び出す', async () => {
    vi.useFakeTimers();

    const tracks = [{ stop: vi.fn() }];
    const stream = {
      getTracks: () => tracks,
    } as unknown as MediaStream;

    Object.defineProperty(globalThis.navigator, 'mediaDevices', {
      value: {
        getUserMedia: vi.fn().mockResolvedValue(stream),
      },
      configurable: true,
    });

    class FakeDetector {
      async detect() {
        return [{ rawValue: '4901234567890' }];
      }
    }

    Object.defineProperty(window, 'BarcodeDetector', {
      value: FakeDetector,
      configurable: true,
    });

    const scanner = useBarcodeScanner(500);
    const video = document.createElement('video');
    video.play = vi.fn().mockResolvedValue(undefined);
    const onDetected = vi.fn();
    const onError = vi.fn();

    await scanner.startCamera(video, { onDetected, onError });
    await vi.advanceTimersByTimeAsync(300);

    expect(onDetected).toHaveBeenCalledWith('4901234567890');
    expect(onError).not.toHaveBeenCalled();
    expect(tracks[0].stop).toHaveBeenCalled();

    vi.useRealTimers();
  });

  it('BarcodeDetector 未対応時は ZXing フォールバックで検出できる', async () => {
    vi.useFakeTimers();

    const tracks = [{ stop: vi.fn() }];
    const stream = { getTracks: () => tracks } as unknown as MediaStream;

    Object.defineProperty(globalThis.navigator, 'mediaDevices', {
      value: { getUserMedia: vi.fn().mockResolvedValue(stream) },
      configurable: true,
    });

    // BarcodeDetector を未定義にする
    Object.defineProperty(window, 'BarcodeDetector', {
      value: undefined,
      configurable: true,
    });

    decodeFromVideoElementMock.mockImplementation(async (_video, callback) => {
      callback(
        {
          getText: () => '4901234567890',
        },
        undefined,
        { stop: vi.fn() },
      );
      return { stop: vi.fn() };
    });

    const scanner = useBarcodeScanner(500);
    const video = document.createElement('video');
    video.play = vi.fn().mockResolvedValue(undefined);
    const onDetected = vi.fn();
    const onError = vi.fn();

    await scanner.startCamera(video, { onDetected, onError });

    expect(onDetected).toHaveBeenCalledWith('4901234567890');
    expect(onError).not.toHaveBeenCalled();
    expect(scanner.isScanning.value).toBe(false);
    expect(tracks[0].stop).toHaveBeenCalled();

    vi.useRealTimers();
  });

  it('detect() 例外時にループを停止してカメラを解放する', async () => {
    vi.useFakeTimers();

    const tracks = [{ stop: vi.fn() }];
    const stream = { getTracks: () => tracks } as unknown as MediaStream;

    Object.defineProperty(globalThis.navigator, 'mediaDevices', {
      value: { getUserMedia: vi.fn().mockResolvedValue(stream) },
      configurable: true,
    });

    class ThrowingDetector {
      async detect() {
        throw new Error('検出エラー');
      }
    }

    Object.defineProperty(window, 'BarcodeDetector', {
      value: ThrowingDetector,
      configurable: true,
    });

    const scanner = useBarcodeScanner(500);
    const video = document.createElement('video');
    video.play = vi.fn().mockResolvedValue(undefined);
    const onDetected = vi.fn();
    const onError = vi.fn();

    await scanner.startCamera(video, { onDetected, onError });
    await vi.advanceTimersByTimeAsync(200);

    expect(onError).toHaveBeenCalledWith('バーコードの読み取りに失敗しました。');
    // ループが停止されカメラが解放されていること（onError は 1 回だけ呼ばれる）
    expect(onError).toHaveBeenCalledTimes(1);
    expect(scanner.isScanning.value).toBe(false);
    expect(tracks[0].stop).toHaveBeenCalled();

    vi.useRealTimers();
  });
});
