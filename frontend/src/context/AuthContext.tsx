"use client"

import { createContext, useContext, useState, useEffect, ReactNode } from "react";
import { AuthResponse } from "@/types";
import api from "@/lib/api";

interface AuthContextType {
    user: AuthResponse | null; //null = not logged in
    isLoading: boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    register: (firstName: string, lastName: string, email: string, password: string, role: "customer" | "restaurant_admin") => Promise<void>;
    isAuthenticated: boolean;
    isRestaurantAdmin: boolean;
    isCustomer: boolean;
    isSystemAdmin: boolean;
}

//create context with default value
const AuthContext = createContext<AuthContextType | null>(null);

//create provider component
export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<AuthResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);


    //check if user is logged in on component mount
    useEffect(() => {
        const token = localStorage.getItem("token");
        const savedUser = localStorage.getItem("user");

        if (token && savedUser) {
            try {
                setUser(JSON.parse(savedUser));
            } catch (error) {
                //data corrupted, cant be parsed, remove it
                localStorage.removeItem("token");
                localStorage.removeItem("user");
                setUser(null);
            }
        }
        setIsLoading(false);
    }, []);

    const login = async (email: string, password: string) => {
        const response = await api.post<AuthResponse>("/auth/login", {
            email,
            password,
        });
        const data = response.data;

        localStorage.setItem("token", data.token);
        localStorage.setItem("refreshToken", data.refreshToken);
        localStorage.setItem("user", JSON.stringify(data));
        setUser(data);
    };

    const logout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("user");
        setUser(null);
    };

    const register = async (
        firstName: string,
        lastName: string,
        email: string,
        password: string,
        role: "customer" | "restaurant_admin"
    ) => {
        const response = await api.post<AuthResponse>("/auth/register", {
            firstName,
            lastName,
            email,
            password,
            role,
        });
        const data = response.data;

        localStorage.setItem("token", data.token);
        localStorage.setItem("refreshToken", data.refreshToken);
        localStorage.setItem("user", JSON.stringify(data));
        setUser(data);
    }

    //convenience booleans to show/hide elements
    const isAuthenticated = !!user;
    const isRestaurantAdmin = user?.role === "restaurant_admin";
    const isCustomer = user?.role === "customer";
    const isSystemAdmin = user?.role === "system_admin";

    return (
        <AuthContext.Provider value={{
            user,
            isLoading,
            login,
            logout,
            register,
            isAuthenticated,
            isRestaurantAdmin,
            isCustomer,
            isSystemAdmin,
        }}
        >
            {children}
        </AuthContext.Provider>
    );
}

//custom hook Auth
export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
}
