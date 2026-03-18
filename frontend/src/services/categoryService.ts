import api from "@/lib/api";
import { Category, CreateCategoryRequest } from "@/types";

export const categoryService = {
    getByRestaurant: (restaurantId: number) =>
        api.get<Category[]>(`/categories?restaurantId=${restaurantId}`),

    create: (data: CreateCategoryRequest) =>
        api.post<Category>("/categories", data),

    update: (id: number, data: Partial<CreateCategoryRequest>) =>
        api.put<Category>(`/categories/${id}`, data),

    delete: (id: number) =>
        api.delete(`/categories/${id}`),
};
