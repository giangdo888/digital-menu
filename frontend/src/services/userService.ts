import api from "@/lib/api";
import { UserProfile, CreateProfileRequest, WeightHistory, LogWeightRequest, UserResponse, UpdateUserRequest } from "@/types";

export const userService = {
    getProfile: () =>
        api.get<UserProfile>("/users/me/profile"),

    createProfile: (request: CreateProfileRequest) =>
        api.post<UserProfile>("/users/me/profile", request),

    updateProfile: (request: Partial<CreateProfileRequest>) =>
        api.put<UserProfile>("/users/me/profile", request),

    updateAccount: (request: UpdateUserRequest) =>
        api.put<UserResponse>("/users/me", request),

    getWeightHistory: (limit: number = 30, startDate?: string, endDate?: string) => {
        let url = `/users/me/weight?limit=${limit}`;
        if (startDate) url += `&startDate=${startDate}`;
        if (endDate) url += `&endDate=${endDate}`;
        return api.get<WeightHistory[]>(url);
    },

    logWeight: (request: LogWeightRequest) =>
        api.post("/users/me/weight", request),
}