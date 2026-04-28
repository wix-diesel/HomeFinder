export type ItemRegistrationFormState = {
  name: string;
  quantity: number | null;
  categoryId: string;
  manufacturer: string;
  priceInput: string;
  note: string;
  barcode: string;
  description: string;
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
    isSubmitting: false,
    fieldErrors: {},
    submitError: null,
  };
}
