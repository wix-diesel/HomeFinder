export type ItemRegistrationFormState = {
  name: string;
  quantity: number | null;
  category: string;
  priceInput: string;
  note: string;
  referenceCode?: string;
  description?: string;
  isSubmitting: boolean;
  fieldErrors: Record<string, string>;
  submitError: string | null;
};

export function createInitialItemRegistrationFormState(): ItemRegistrationFormState {
  return {
    name: '',
    quantity: null,
    category: '',
    priceInput: '',
    note: '',
    referenceCode: '',
    description: '',
    isSubmitting: false,
    fieldErrors: {},
    submitError: null,
  };
}
