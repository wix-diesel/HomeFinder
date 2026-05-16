<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { AppPrimaryButton, FormField, StatePanel } from './common';
import ImageUploader from './ImageUploader.vue';
import BarcodeScannerDialog from './BarcodeScannerDialog.vue';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';
import type { Category } from '../models/category';
import { categoryService } from '../services/categoryService';
import { getLookupMessage, getLookupRecommendation, lookupProductByJan, ProductLookupError } from '../services/productLookupService';
import { isValidJan, normalizeJan } from '../utils/jan';
import { useBarcodeScanner } from '../composables/useBarcodeScanner';
import { uiText } from '../constants/uiText';

const emit = defineEmits<{
  submit: [payload: ItemRegistrationFormState];
  retry: [];
}>();

const props = withDefaults(
  defineProps<{
    submitError?: string;
    isSubmitting?: boolean;
    initialValues?: Partial<ItemRegistrationFormState>;
    submitLabelJa?: string;
    submitErrorTitleJa?: string;
    /** 編集モードのアイテム ID。指定時はドロップゾーンが画像アップローダーになる */
    itemId?: string;
  }>(),
  {
    submitError: '',
    isSubmitting: false,
    initialValues: undefined,
    submitLabelJa: undefined,
    submitErrorTitleJa: undefined,
    itemId: undefined,
  },
);

const categories = ref<Category[]>([]);
const scannerOpen = ref(false);
const lookupErrorMessage = ref('');
const lookupRecommendation = ref('');
const lookupWarning = ref('');

const mergeCandidates = ref<Partial<Record<'name' | 'manufacturer' | 'priceInput', string>> | null>(null);
const mergeSelection = reactive<Record<'name' | 'manufacturer' | 'priceInput', 'current' | 'fetched'>>({
  name: 'fetched',
  manufacturer: 'fetched',
  priceInput: 'fetched',
});

const {
  isCooldown,
  remainingCooldownMs,
  executeLatestLookup,
} = useBarcodeScanner(500);

const formState = reactive<ItemRegistrationFormState>({
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
});

// initialValues が渡された（または変更された）タイミングでフォームに反映する
watch(
  () => props.initialValues,
  (newValues) => {
    if (newValues) {
      Object.assign(formState, newValues);
    }
  },
  { immediate: true },
);

onMounted(async () => {
  try {
    categories.value = await categoryService.getCategories(true);
  } catch {
    // カテゴリー取得失敗時はフォームを利用可能な状態に保つ
  }
});

const hasValidationError = computed(() => Object.values(formState.fieldErrors).some((message) => Boolean(message)));
const hasMergeChoices = computed(() => Boolean(mergeCandidates.value));
const hasLookupWarning = computed(() => lookupWarning.value.length > 0);
const cooldownText = computed(() => {
  if (!isCooldown.value) {
    return '';
  }
  return `連続検索を抑止中です。${Math.ceil(remainingCooldownMs.value)}ms 後に再検索できます。`;
});

function resetLookupMessages() {
  lookupErrorMessage.value = '';
  lookupRecommendation.value = '';
  lookupWarning.value = '';
}

function toFetchedValues(result: { name: string | null; manufacturer: string | null; price: number | null }) {
  return {
    name: result.name?.trim() ?? '',
    manufacturer: result.manufacturer?.trim() ?? '',
    priceInput: result.price == null ? '' : String(result.price),
  } satisfies Partial<Record<'name' | 'manufacturer' | 'priceInput', string>>;
}

function applyFetchedValues(fetched: Partial<Record<'name' | 'manufacturer' | 'priceInput', string>>) {
  if (fetched.name) {
    formState.name = fetched.name;
  }
  if (fetched.manufacturer) {
    formState.manufacturer = fetched.manufacturer;
  }
  if (fetched.priceInput) {
    formState.priceInput = fetched.priceInput;
  }
}

function getConflictedFields(fetched: Partial<Record<'name' | 'manufacturer' | 'priceInput', string>>) {
  const conflicts: Array<'name' | 'manufacturer' | 'priceInput'> = [];

  if (fetched.name && formState.name.trim() && formState.name.trim() !== fetched.name) {
    conflicts.push('name');
  }
  if (fetched.manufacturer && formState.manufacturer.trim() && formState.manufacturer.trim() !== fetched.manufacturer) {
    conflicts.push('manufacturer');
  }
  if (fetched.priceInput && formState.priceInput.trim() && formState.priceInput.trim() !== fetched.priceInput) {
    conflicts.push('priceInput');
  }

  return conflicts;
}

function setupWarnings(result: { name: string | null; manufacturer: string | null; price: number | null }) {
  if (!result.name) {
    formState.fieldErrors.name = '商品名が取得できなかったため保存できません。';
  }

  if (result.name && (result.manufacturer == null || result.price == null)) {
    lookupWarning.value = '価格またはメーカーが未取得です。警告を確認した上で保存できます。';
  }
}

async function lookupByJan(rawJan: string) {
  const jan = normalizeJan(rawJan);
  formState.barcode = jan;
  formState.fieldErrors.barcode = '';
  resetLookupMessages();
  mergeCandidates.value = null;

  if (!isValidJan(jan)) {
    formState.fieldErrors.barcode = 'JANコードは8桁または13桁の数字で入力してください。';
    return;
  }

  if (isCooldown.value) {
    lookupRecommendation.value = cooldownText.value;
    return;
  }

  formState.barcodeLookupStatus = 'searching';

  try {
    const result = await executeLatestLookup((signal) => lookupProductByJan(jan, { signal, timeoutMs: 3000 }));
    if (!result) {
      return;
    }

    const fetched = toFetchedValues(result);
    const conflicts = getConflictedFields(fetched);

    if (conflicts.length > 0) {
      mergeCandidates.value = fetched;
      conflicts.forEach((field) => {
        mergeSelection[field] = 'fetched';
      });
    } else {
      applyFetchedValues(fetched);
    }

    setupWarnings(result);
    formState.barcodeLookupStatus = 'success';
  } catch (error) {
    formState.barcodeLookupStatus = 'error';
    if (error instanceof ProductLookupError) {
      lookupErrorMessage.value = getLookupMessage(error.code);
      lookupRecommendation.value = getLookupRecommendation(error.code);
      return;
    }

    lookupErrorMessage.value = '商品情報の取得に失敗しました。';
    lookupRecommendation.value = '再試行または手動入力で続行してください。';
  }
}

function applyMergeSelection() {
  if (!mergeCandidates.value) {
    return;
  }

  const fetched = mergeCandidates.value;
  const fields: Array<'name' | 'manufacturer' | 'priceInput'> = ['name', 'manufacturer', 'priceInput'];

  for (const field of fields) {
    const fetchedValue = fetched[field];
    if (!fetchedValue) {
      continue;
    }

    if (mergeSelection[field] === 'fetched') {
      if (field === 'name') {
        formState.name = fetchedValue;
      }
      if (field === 'manufacturer') {
        formState.manufacturer = fetchedValue;
      }
      if (field === 'priceInput') {
        formState.priceInput = fetchedValue;
      }
    }
  }

  mergeCandidates.value = null;
}

function openScannerDialog() {
  scannerOpen.value = true;
}

function closeScannerDialog() {
  scannerOpen.value = false;
}

async function onScannerDetected(jan: string) {
  scannerOpen.value = false;
  await lookupByJan(jan);
}

async function onBarcodeEnter() {
  await lookupByJan(formState.barcode);
}

async function retryLookup() {
  await lookupByJan(formState.barcode);
}

function validate(): boolean {
  formState.fieldErrors = {};

  if (!formState.name.trim()) {
    formState.fieldErrors.name = uiText.errors.nameRequired;
  }

  if (formState.quantity === null || !Number.isInteger(formState.quantity) || formState.quantity < 1) {
    formState.fieldErrors.quantity = uiText.errors.quantityInvalid;
  }

  if (formState.priceInput.trim().length > 0) {
    const priceValue = Number(formState.priceInput);
    if (Number.isNaN(priceValue) || priceValue < 0) {
      formState.fieldErrors.priceInput = uiText.errors.priceInvalid;
    }
  }

  if (formState.barcode.trim() && !isValidJan(formState.barcode)) {
    formState.fieldErrors.barcode = 'JANコードは8桁または13桁の数字で入力してください。';
  }

  if (formState.barcodeLookupStatus === 'success' && !formState.name.trim()) {
    formState.fieldErrors.name = '商品名が未取得のため保存できません。';
  }

  return !hasValidationError.value;
}

function onSubmit(): void {
  formState.submitError = null;
  if (!validate()) {
    return;
  }

  emit('submit', { ...formState, isSubmitting: props.isSubmitting });
}

function onRetry(): void {
  formState.submitError = null;
  emit('retry');
}
</script>

<template>
  <form class="item-form" @submit.prevent="onSubmit">
    <div class="form-grid">
      <div class="main-panel">
        <StatePanel
          v-if="hasValidationError"
          state-type="validation_error"
          :title-ja="uiText.errors.validationSummary"
          description-ja="入力項目を確認してください。"
        />

        <StatePanel
          v-if="props.submitError"
          state-type="failure"
          :title-ja="props.submitErrorTitleJa ?? uiText.errors.submitFailed"
          :description-ja="props.submitError"
          :primary-action-label-ja="uiText.create.retry"
          @primary-action="onRetry"
        />

        <StatePanel
          v-if="lookupErrorMessage"
          state-type="failure"
          :title-ja="uiText.create.barcode.lookupErrorTitle"
          :description-ja="lookupErrorMessage"
          :primary-action-label-ja="uiText.create.barcode.retryLookup"
          @primary-action="retryLookup"
        />

        <StatePanel
          v-if="lookupRecommendation"
          state-type="validation_error"
          title-ja="推奨アクション"
          :description-ja="lookupRecommendation"
        />

        <StatePanel
          v-if="hasLookupWarning"
          state-type="validation_error"
          :title-ja="uiText.create.barcode.lookupWarningTitle"
          :description-ja="lookupWarning"
        />

        <p v-if="cooldownText" class="cooldown-note">{{ cooldownText }}</p>

        <div class="field-two">
          <FormField
            name="name"
            :label-ja="uiText.create.fields.name.label"
            :helper-text-ja="uiText.create.fields.name.helper"
            :placeholder-ja="uiText.create.fields.name.placeholder"
            :error-text-ja="formState.fieldErrors.name"
            :model-value="formState.name"
            required
            @update:model-value="(value) => (formState.name = String(value ?? ''))"
          />

          <FormField
            name="manufacturer"
            :label-ja="uiText.create.fields.manufacturer.label"
            :helper-text-ja="uiText.create.fields.manufacturer.helper"
            :placeholder-ja="uiText.create.fields.manufacturer.placeholder"
            :error-text-ja="formState.fieldErrors.manufacturer"
            :model-value="formState.manufacturer"
            @update:model-value="(value) => (formState.manufacturer = String(value ?? ''))"
          />
        </div>

        <div class="field-two">
          <label class="form-field">
            <span class="form-field__label">{{ uiText.create.fields.category.label }}</span>
            <select
              name="categoryId"
              :value="formState.categoryId"
              @change="(event) => (formState.categoryId = (event.target as HTMLSelectElement).value)"
            >
              <option value="">{{ uiText.create.fields.category.placeholder }}</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
            <small class="helper">{{ uiText.create.fields.category.helper }}</small>
          </label>

          <label class="form-field">
            <span class="form-field__label">{{ uiText.create.fields.barcode.label }}</span>
            <div class="barcode-input-wrap">
              <input
                name="barcode"
                type="text"
                :value="formState.barcode"
                :placeholder="uiText.create.fields.barcode.placeholder"
                @input="(event) => (formState.barcode = String((event.target as HTMLInputElement).value ?? ''))"
                @keydown.enter.prevent="onBarcodeEnter"
              >
              <button
                type="button"
                class="camera-btn"
                :aria-label="uiText.create.barcode.openCamera"
                :title="uiText.create.barcode.openCamera"
                @click="openScannerDialog"
              >
                <span class="material-symbols-outlined" aria-hidden="true">photo_camera</span>
              </button>
            </div>
            <small class="helper">{{ uiText.create.fields.barcode.helper }}</small>
            <small v-if="formState.fieldErrors.barcode" class="error">{{ formState.fieldErrors.barcode }}</small>
          </label>
        </div>

        <div class="field-two">
          <FormField
            name="quantity"
            type="number"
            :label-ja="uiText.create.fields.quantity.label"
            :helper-text-ja="uiText.create.fields.quantity.helper"
            :placeholder-ja="uiText.create.fields.quantity.placeholder"
            :error-text-ja="formState.fieldErrors.quantity"
            :model-value="formState.quantity"
            required
            @update:model-value="(value) => (formState.quantity = value === null ? null : Number(value))"
          />

          <FormField
            name="priceInput"
            :label-ja="uiText.create.fields.priceInput.label"
            :helper-text-ja="uiText.create.fields.priceInput.helper"
            :placeholder-ja="uiText.create.fields.priceInput.placeholder"
            :error-text-ja="formState.fieldErrors.priceInput"
            :model-value="formState.priceInput"
            @update:model-value="(value) => (formState.priceInput = String(value ?? ''))"
          />
        </div>

        <section v-if="hasMergeChoices" class="merge-panel">
          <h4>取得値の反映方法を選択</h4>
          <p>既存入力と取得結果が異なる項目があります。項目ごとに採用する値を選択してください。</p>
          <div v-if="mergeCandidates?.name && formState.name.trim() && formState.name.trim() !== mergeCandidates.name" class="merge-row">
            <span class="merge-label">商品名</span>
            <label><input v-model="mergeSelection.name" type="radio" value="current"> {{ uiText.create.barcode.keepCurrent }}: {{ formState.name }}</label>
            <label><input v-model="mergeSelection.name" type="radio" value="fetched"> {{ uiText.create.barcode.useFetched }}: {{ mergeCandidates.name }}</label>
          </div>
          <div v-if="mergeCandidates?.manufacturer && formState.manufacturer.trim() && formState.manufacturer.trim() !== mergeCandidates.manufacturer" class="merge-row">
            <span class="merge-label">メーカー</span>
            <label><input v-model="mergeSelection.manufacturer" type="radio" value="current"> {{ uiText.create.barcode.keepCurrent }}: {{ formState.manufacturer }}</label>
            <label><input v-model="mergeSelection.manufacturer" type="radio" value="fetched"> {{ uiText.create.barcode.useFetched }}: {{ mergeCandidates.manufacturer }}</label>
          </div>
          <div v-if="mergeCandidates?.priceInput && formState.priceInput.trim() && formState.priceInput.trim() !== mergeCandidates.priceInput" class="merge-row">
            <span class="merge-label">価格</span>
            <label><input v-model="mergeSelection.priceInput" type="radio" value="current"> {{ uiText.create.barcode.keepCurrent }}: {{ formState.priceInput }}</label>
            <label><input v-model="mergeSelection.priceInput" type="radio" value="fetched"> {{ uiText.create.barcode.useFetched }}: {{ mergeCandidates.priceInput }}</label>
          </div>
          <button type="button" class="secondary-btn" @click="applyMergeSelection">{{ uiText.create.barcode.applyDiff }}</button>
        </section>

        <FormField
          name="description"
          type="textarea"
          :label-ja="uiText.create.fields.description.label"
          :helper-text-ja="uiText.create.fields.description.helper"
          :placeholder-ja="uiText.create.fields.description.placeholder"
          :model-value="formState.description"
          @update:model-value="(value) => (formState.description = String(value ?? ''))"
        />

        <FormField
          name="note"
          type="textarea"
          :label-ja="uiText.create.fields.note.label"
          :helper-text-ja="uiText.create.fields.note.helper"
          :placeholder-ja="uiText.create.fields.note.placeholder"
          :model-value="formState.note"
          @update:model-value="(value) => (formState.note = String(value ?? ''))"
        />

        <!-- 画像アップロード（編集モード: 即時アップロード / 新規作成: ファイル選択して作成時アップロード） -->
        <ImageUploader
          :item-id="props.itemId"
          @file-selected="(file: File) => (formState.imageFile = file)"
          @uploaded="(imageId: string) => (formState.imageId = imageId)"
        />
        <!-- 新規登録モードではドロップゾーンはクリックでファイル選択し、create 時に送信される -->
      </div>

      <aside class="summary-panel">
        <h3>登録サマリー</h3>
        <dl>
          <div>
            <dt>状態</dt>
            <dd>下書き</dd>
          </div>
          <div>
            <dt>保存先</dt>
            <dd>倉庫 A-12</dd>
          </div>
          <div>
            <dt>公開範囲</dt>
            <dd>社内のみ</dd>
          </div>
        </dl>
      </aside>
    </div>

    <div class="action-bar">
      <button type="button" class="secondary-btn">下書き保存</button>
      <AppPrimaryButton :label-ja="props.submitLabelJa ?? uiText.create.submit" type="submit" :loading="props.isSubmitting" />
    </div>

    <BarcodeScannerDialog
      :open="scannerOpen"
      :title-ja="uiText.create.barcode.scannerTitle"
      @close="closeScannerDialog"
      @detected="onScannerDetected"
      @error="(payload) => (lookupErrorMessage = payload.message)"
    />
  </form>
</template>

<style scoped>
.item-form {
  display: grid;
  gap: 16px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

.main-panel {
  display: grid;
  gap: 12px;
  border: 1px solid #d7dfeb;
  border-radius: 14px;
  padding: 14px;
  background: #fff;
}

.field-two {
  display: grid;
  gap: 10px;
  grid-template-columns: 1fr;
  align-items: start;
}

.form-field {
  display: grid;
  gap: 6px;
}

.form-field__label {
  font-weight: 700;
}

select {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 10px;
  font-size: 0.95rem;
  background: #fff;
}

.helper {
  color: #475569;
  font-size: 0.85rem;
}

.error {
  color: #b91c1c;
  font-size: 0.85rem;
}

.cooldown-note {
  margin: 0;
  color: #334155;
  font-size: 0.85rem;
}

.barcode-input-wrap {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 8px;
}

.barcode-input-wrap input {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 10px;
  font-size: 0.95rem;
}

.camera-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  width: 42px;
  min-width: 42px;
  height: 42px;
  padding: 0;
  background: #fff;
  color: #334155;
  cursor: pointer;
}

.camera-btn .material-symbols-outlined {
  width: 20px;
  height: 20px;
  font-size: 20px;
}

.merge-panel {
  border: 1px solid #d7dfeb;
  border-radius: 10px;
  padding: 12px;
  display: grid;
  gap: 10px;
  background: #f8fbff;
}

.merge-panel h4,
.merge-panel p {
  margin: 0;
}

.merge-row {
  display: grid;
  gap: 6px;
}

.merge-label {
  font-weight: 700;
}

.dropzone {
  border: 2px dashed #bfcede;
  border-radius: 12px;
  padding: 20px;
  background: #f8fbff;
  text-align: center;
}

.dropzone-title {
  margin: 0;
  color: #334155;
  font-weight: 700;
}

.dropzone-sub {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 0.85rem;
}

.summary-panel {
  border: 1px solid #d7dfeb;
  border-radius: 14px;
  padding: 14px;
  background: #fff;
}

.summary-panel h3 {
  margin: 0 0 8px;
}

.summary-panel dl {
  margin: 0;
  display: grid;
  gap: 8px;
}

.summary-panel div {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 6px;
}

.summary-panel dt,
.summary-panel dd {
  margin: 0;
}

.summary-panel dt {
  color: #64748b;
}

.summary-panel dd {
  font-weight: 700;
}

.action-bar {
  position: sticky;
  bottom: 70px;
  z-index: 5;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  border: 1px solid #d7dfeb;
  background: rgba(255, 255, 255, 0.96);
  border-radius: 12px;
  padding: 10px;
  backdrop-filter: blur(4px);
}

.secondary-btn {
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 10px 14px;
  background: #fff;
  color: #475569;
  font-weight: 700;
}

@media (min-width: 1024px) {
  .form-grid {
    grid-template-columns: minmax(0, 2fr) minmax(240px, 1fr);
    align-items: start;
  }

  .field-two {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
