import api from "@/lib/api";
import { MealLog, CreateMealLogRequest } from "@/types";

export const mealLogService = {
    create: (data: CreateMealLogRequest) =>
        api.post<MealLog>("/meal-logs", data),

    getMyLogs: () =>
        api.get<MealLog[]>("/meal-logs"),

    delete: (id: number) =>
        api.delete(`/meal-logs/${id}`),

    getSummary: (startDate: string, endDate: string) =>
        api.get<any[]>(`/meal-logs/summary?startDate=${startDate}&endDate=${endDate}`),
};
