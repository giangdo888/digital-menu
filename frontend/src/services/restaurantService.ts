import api from "@/lib/api";
import { RestaurantPublic, MenuResponse, Restaurant, CreateRestaurantRequest } from "@/types";

export const restaurantService = {
    getPublicList: () =>
        api.get<RestaurantPublic[]>("/restaurants/public"),

    getBySlug: (slug: string) =>
        api.get<RestaurantPublic>(`/restaurants/public/${slug}`),

    getMenuBySlug: (slug: string) =>
        api.get<MenuResponse>(`/restaurants/public/${slug}/menu`),

    //owner, admin
    getMyRestaurants: () =>
        api.get<Restaurant[]>("/restaurants/mine"),

    create: (data: CreateRestaurantRequest) =>
        api.post<Restaurant>("/restaurants", data),

    //owner can only see own restaurant, admin can see any
    getById: (id: number) =>
        api.get<Restaurant>(`/restaurants/${id}`),

    //owner can only update own restaurant, admin can update any
    update: (id: number, data: CreateRestaurantRequest) =>
        api.put<Restaurant>(`/restaurants/${id}`, data),

    active: (id: number) =>
        api.put<Restaurant>(`/restaurants/${id}/active`),

    deactive: (id: number) =>
        api.put<Restaurant>(`/restaurants/${id}/deactive`),

    //admin only
    getAll: () =>
        api.get<Restaurant[]>("/restaurants"),
}