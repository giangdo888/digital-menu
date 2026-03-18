"use client";

import { useAuth } from "@/context/AuthContext";
import { useRouter } from "next/navigation";
import ProtectedRoute from "@/components/layout/ProtectedRoute";
import toast from "react-hot-toast";

export default function AdminAccountPage() {
    const { user, logout } = useAuth();
    const router = useRouter();

    const handleLogout = async () => {
        await logout();
        toast.success("Logged out successfully");
        router.push("/login");
    };

    return (
        <ProtectedRoute allowedRoles={["restaurant_admin", "system_admin"]}>
            <div className="max-w-2xl mx-auto px-4 py-8">
                <h1 className="text-2xl font-bold mb-6">My Account</h1>

                {/* Personal Info */}
                <div className="bg-bg-card rounded-xl p-5 mb-6">
                    <h2 className="font-semibold mb-4 text-text-secondary text-sm uppercase tracking-wide">Personal Info</h2>
                    <div className="space-y-3">
                        <div className="flex justify-between items-center">
                            <span className="text-text-secondary">Name</span>
                            <span className="font-medium">{user?.firstName} {user?.lastName}</span>
                        </div>
                        <div className="border-t border-bg-elevated" />
                        <div className="flex justify-between items-center">
                            <span className="text-text-secondary">Email</span>
                            <span className="font-medium">{user?.email}</span>
                        </div>
                        <div className="border-t border-bg-elevated" />
                        <div className="flex justify-between items-center">
                            <span className="text-text-secondary">Role</span>
                            <span className="bg-accent/20 text-accent text-xs font-medium px-2.5 py-1 rounded-full">
                                Restaurant Admin
                            </span>
                        </div>
                    </div>
                </div>

                {/* Logout */}
                <button
                    onClick={handleLogout}
                    className="w-full bg-danger hover:bg-red-400 text-white font-semibold py-3 rounded-xl"
                >
                    Logout
                </button>
            </div>
        </ProtectedRoute>
    );
}
