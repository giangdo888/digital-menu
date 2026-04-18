"use client"

import { useAuth } from "@/context/AuthContext";
import { useRouter, usePathname } from "next/navigation";
import { useEffect } from "react";

export default function ProfileGuard({ children }: { children: React.ReactNode }) {
    const { isAuthenticated, isCustomer, hasProfile, isLoading } = useAuth();
    const router = useRouter();
    const pathname = usePathname();

    useEffect(() => {
        // Only guard for customers who are logged in
        if (!isLoading && isAuthenticated && isCustomer) {
            // If they don't have a profile and are NOT already on the profile page
            if (!hasProfile && pathname !== "/dashboard/profile") {
                router.replace("/dashboard/profile");
            }
        }
    }, [isAuthenticated, isCustomer, hasProfile, isLoading, pathname, router]);

    // Show nothing while checking (minimal flash)
    if (isLoading) return null;

    // If they are a customer without a profile and NOT on the profile page, hide content while redirecting
    if (isAuthenticated && isCustomer && !hasProfile && pathname !== "/dashboard/profile") {
        return null;
    }

    return <>{children}</>;
}
