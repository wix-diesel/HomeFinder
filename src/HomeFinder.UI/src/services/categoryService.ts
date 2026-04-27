// フロントエンド: カテゴリー API サービス（雛形）

import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../models/category';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

/**
 * カテゴリー API サービス
 *
 * バックエンド /api/categories との通信を担当
 */
export class CategoryService {
  private readonly baseUrl = '/api/categories';

  /**
   * カテゴリー一覧を取得
   */
  async getCategories(includeReserved: boolean = true): Promise<Category[]> {
    try {
      const url = new URL(`${API_BASE_URL}${this.baseUrl}`);
      url.searchParams.append('includeReserved', String(includeReserved));

      const response = await fetch(url.toString(), {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch categories: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('CategoryService.getCategories error:', error);
      throw error;
    }
  }

  /**
   * カテゴリー詳細を取得
   */
  async getCategory(id: string): Promise<Category> {
    try {
      const response = await fetch(`${API_BASE_URL}${this.baseUrl}/${id}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch category: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('CategoryService.getCategory error:', error);
      throw error;
    }
  }

  /**
   * カテゴリーを作成
   */
  async createCategory(request: CreateCategoryRequest): Promise<Category> {
    try {
      const response = await fetch(`${API_BASE_URL}${this.baseUrl}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const error = await this.parseErrorResponse(response);
        throw error;
      }

      return await response.json();
    } catch (error) {
      console.error('CategoryService.createCategory error:', error);
      throw this.normalizeUnknownError(error);
    }
  }

  /**
   * カテゴリーを更新
   */
  async updateCategory(
    id: string,
    request: UpdateCategoryRequest
  ): Promise<Category> {
    try {
      const response = await fetch(`${API_BASE_URL}${this.baseUrl}/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const error = await this.parseErrorResponse(response);
        throw error;
      }

      return await response.json();
    } catch (error) {
      console.error('CategoryService.updateCategory error:', error);
      throw error;
    }
  }

  /**
   * カテゴリーを削除
   */
  async deleteCategory(id: string): Promise<void> {
    try {
      const response = await fetch(`${API_BASE_URL}${this.baseUrl}/${id}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const error = await this.parseErrorResponse(response);
        throw error;
      }
    } catch (error) {
      console.error('CategoryService.deleteCategory error:', error);
      throw error;
    }
  }

  /**
   * エラーレスポンスを解析
   */
  private async parseErrorResponse(response: Response): Promise<Error> {
    try {
      const errorData = await response.json();
      const message = this.getErrorMessage(errorData.code, errorData.message);
      const error = new Error(message);
      (error as any).code = errorData.code;
      (error as any).details = errorData.details;
      return error;
    } catch {
      return new Error(`API Error: ${response.status}`);
    }
  }

  /**
   * エラーコードを日本語メッセージに変換
   */
  private getErrorMessage(code: string, fallback: string): string {
    const messages: Record<string, string> = {
      VALIDATION_ERROR: '入力内容に誤りがあります。',
      CATEGORY_NOT_FOUND: '指定されたカテゴリーが見つかりません。',
      CATEGORY_NAME_DUPLICATE: '同一名称のカテゴリーが既に存在します。',
      RESERVED_CATEGORY_PROTECTED:
        'このカテゴリーは編集・削除できません。',
    };
    return messages[code] || fallback || 'エラーが発生しました。';
  }

  /**
   * 非 API エラー（通信断など）を画面表示向けに正規化
   */
  private normalizeUnknownError(error: unknown): Error {
    if (error instanceof Error && (error as any).code) {
      return error;
    }

    return new Error('通信エラー: もう一度試してください');
  }
}

/**
 * シングルトンインスタンス
 */
export const categoryService = new CategoryService();
