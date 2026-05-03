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

export const detailStateMessages: Record<'not_found' | 'fetch_failure' | 'delete_failure' | 'update_not_found' | 'submitting', ScreenState> = {
  submitting: {
    stateType: 'submitting',
    titleJa: uiText.detail.loadingMessage,
    descriptionJa: '',
    isBusy: true,
  },
  not_found: {
    stateType: 'empty',
    titleJa: uiText.detail.notFoundMessage,
    descriptionJa: '',
    primaryActionLabelJa: uiText.detail.backToList,
  },
  fetch_failure: {
    stateType: 'failure',
    titleJa: uiText.detail.fetchErrorMessage,
    descriptionJa: '',
    primaryActionLabelJa: uiText.detail.backToList,
  },
  delete_failure: {
    stateType: 'failure',
    titleJa: uiText.detail.deleteFailMessage,
    descriptionJa: '',
  },
  update_not_found: {
    stateType: 'failure',
    titleJa: uiText.detail.updateNotFoundMessage,
    descriptionJa: '',
    primaryActionLabelJa: uiText.detail.backToList,
  },
};
