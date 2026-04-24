import type { CreateItemRequest } from '../models/createItemRequest';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';

export function toCreateItemRequest(formState: ItemRegistrationFormState): CreateItemRequest {
  return {
    name: formState.name.trim(),
    quantity: Number(formState.quantity ?? 0),
  };
}
