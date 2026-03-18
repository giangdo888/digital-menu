"use  client"

import { useAuth } from "@/context/AuthContext";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles: string[];
}

export default function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
    const { user, isLoading, isAuthenticated } = useAuth();
    const router = useRouter();

    useEffect(() => {
        if (!isLoading && !isAuthenticated) {
            router.push("/login");
        }

        if (!isLoading && user && !allowedRoles.includes(user.role)) {
            router.push("/"); // r edirect if wrong role
        }
    }, [isLoading, isAuthenticated, user, allowedRoles, router]);

    if (!isAuthenticated) {
        return null;
    }

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;
    if (!user || !allowedRoles.includes(user.role)) return null;
    return <>{children}</>;
}
