export type ScreenStateType = 'empty' | 'validation_error' | 'submitting' | 'failure';

export type ScreenState = {
  stateType: ScreenStateType;
  titleJa: string;
  descriptionJa: string;
  primaryActionLabelJa?: string;
  secondaryActionLabelJa?: string;
  isBusy?: boolean;
};
