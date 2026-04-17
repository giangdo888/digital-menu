import api from "@/lib/api";
import { Dish, AFCDItem } from "@/types";

export const dishService = {
    getByCategory: (categoryId: number) =>
        api.get<Dish[]>(`/dishes?categoryId=${categoryId}`),

    getById: (id: number) =>
        api.get<Dish>(`/dishes/${id}`),

    create: (data: { categoryId: number; name: string; price: number; displayOrder: number }) =>
        api.post<Dish>("/dishes", data),

    update: (id: number, data: { name?: string; price?: number; displayOrder?: number; imageUrl?: string }) =>
        api.put<Dish>(`/dishes/${id}`, data),

    getIngredients: (dishId: number) =>
        api.get(`/dishes/${dishId}/ingredients`),

    updateIngredients: (dishId: number, ingredients: { afcdItemId: number; amountInGrams: number }[]) =>
        api.put(`/dishes/${dishId}/ingredients`, { ingredients }),

    delete: (id: number) =>
        api.delete(`/dishes/${id}`),

    // AFCD search
    searchAFCD: (query: string) =>
        api.get<AFCDItem[]>(`/afcd-items?search=${query}`),
};
