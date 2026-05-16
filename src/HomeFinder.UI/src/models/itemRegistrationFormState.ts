export type ItemRegistrationFormState = {
  name: string;
  quantity: number | null;
  categoryId: string;
  manufacturer: string;
  priceInput: string;
  note: string;
  barcode: string;
  description: string;
  barcodeLookupStatus?: 'idle' | 'searching' | 'success' | 'error' | 'cooldown';
  barcodeLookupMessage?: string | null;
  barcodeLookupRecommendation?: string | null;
  barcodeLookupWarning?: string | null;
  /** 選択済みの画像ファイル（作成時に一時保持） */
  imageFile?: File | null;
  /** 既存画像の ImageId（編集時に使用） */
  imageId?: string | null;
  isSubmitting: boolean;
  fieldErrors: Record<string, string>;
  submitError: string | null;
};

export function createInitialItemRegistrationFormState(): ItemRegistrationFormState {
  return {
    name: '',
    quantity: null,
    categoryId: '',
    manufacturer: '',
    priceInput: '',
    note: '',
    barcode: '',
    description: '',
    barcodeLookupStatus: 'idle',
    barcodeLookupMessage: null,
    barcodeLookupRecommendation: null,
    barcodeLookupWarning: null,
    imageFile: null,
    imageId: null,
    isSubmitting: false,
    fieldErrors: {},
    submitError: null,
  };
}
