import type { CreateItemRequest } from '../models/createItemRequest';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';

export function toCreateItemRequest(formState: ItemRegistrationFormState): CreateItemRequest {
  const payload: CreateItemRequest = {
    name: formState.name.trim(),
    quantity: Number(formState.quantity ?? 0),
  };

  if (formState.manufacturer.trim()) {
    payload.manufacturer = formState.manufacturer.trim();
  }

  if (formState.description.trim()) {
    payload.description = formState.description.trim();
  }

  if (formState.note.trim()) {
    payload.note = formState.note.trim();
  }

  if (formState.barcode.trim()) {
    payload.barcode = formState.barcode.trim();
  }

  const priceValue = Number(formState.priceInput);
  if (formState.priceInput.trim() && !Number.isNaN(priceValue)) {
    payload.price = priceValue;
  }

  if (formState.categoryId.trim()) {
    payload.categoryId = formState.categoryId.trim();
  }

  return payload;
}
