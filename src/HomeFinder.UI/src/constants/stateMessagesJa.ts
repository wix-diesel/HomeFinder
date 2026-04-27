import type { ScreenState } from '../models/screenState';
import { uiText } from './uiText';

export const listStateMessages: Record<'empty' | 'validation_error' | 'submitting' | 'failure', ScreenState> = {
  empty: {
    stateType: 'empty',
    titleJa: uiText.list.emptyTitle,
    descriptionJa: uiText.list.emptyDescription,
    primaryActionLabelJa: uiText.list.resetFilter,
  },
  validation_error: {
    stateType: 'validation_error',
    titleJa: uiText.list.validationTitle,
    descriptionJa: uiText.list.validationDescription,
    primaryActionLabelJa: uiText.list.resetFilter,
  },
  submitting: {
    stateType: 'submitting',
    titleJa: uiText.list.loadingTitle,
    descriptionJa: uiText.list.loadingDescription,
    isBusy: true,
  },
  failure: {
    stateType: 'failure',
    titleJa: uiText.list.failureTitle,
    descriptionJa: uiText.list.failureDescription,
    primaryActionLabelJa: uiText.list.reload,
  },
};
