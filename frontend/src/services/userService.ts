import api from "@/lib/api";
import { UserProfile, CreateProfileRequest, WeightHistory, LogWeightRequest } from "@/types";

export const userService = {
    getProfile: () =>
        api.get<UserProfile>("/users/me/profile"),

    createProfile: (request: CreateProfileRequest) =>
        api.post<UserProfile>("/users/me/profile", request),

    updateProfile: (request: Partial<CreateProfileRequest>) =>
        api.put<UserProfile>("/users/me/profile", request),

    getWeightHistory: (limit: number = 30) =>
        api.get<WeightHistory[]>(`/users/me/weight?limit=${limit}`),

    logWeight: (request: LogWeightRequest) =>
        api.post("/users/me/weight", request),
}