import axios from 'axios'
import type { Item, Category } from '../types'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5221',
})

export const itemsApi = {
  getAll(): Promise<Item[]> {
    return api.get<Item[]>('/Items').then((r) => r.data)
  },
  getById(id: number): Promise<Item> {
    return api.get<Item>(`/Items/${id}`).then((r) => r.data)
  },
  create(name: string, description: string, janCode: string, price: number, categoryId: number, image: File | null): Promise<Item> {
    const form = new FormData()
    form.append('name', name)
    form.append('description', description)
    form.append('janCode', janCode)
    form.append('price', String(price))
    form.append('categoryId', String(categoryId))
    if (image) form.append('image', image)
    return api.post<Item>('/Items', form).then((r) => r.data)
  },
  update(id: number, name: string, description: string, janCode: string, price: number, categoryId: number): Promise<Item> {
    return api.put<Item>(`/Items/${id}`, { name, description, janCode, price, categoryId }).then((r) => r.data)
  },
  delete(id: number): Promise<void> {
    return api.delete(`/Items/${id}`).then(() => undefined)
  },
}

export const categoriesApi = {
  getAll(): Promise<Category[]> {
    return api.get<Category[]>('/api/categories').then((r) => r.data)
  },
}

export const picturesApi = {
  getUrl(id: number): string {
    return `${api.defaults.baseURL}/api/pictures/${id}`
  },
}
