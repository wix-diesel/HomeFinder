export interface Picture {
  id: number
  url: string
  description: string
  uploadedAt: string
}

export interface Category {
  id: number
  name: string
  description: string
}

export interface Item {
  id: number
  name: string
  description: string
  janCode: string
  price: number
  picture: Picture | null
  category: Category | null
  categoryId: number
  createdAt: string
  updatedAt: string
}

export interface ItemFormData {
  name: string
  description: string
  janCode: string
  price: number
  categoryId: number
  image: File | null
}
