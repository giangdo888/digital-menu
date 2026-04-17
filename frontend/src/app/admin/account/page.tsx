"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { useRouter } from "next/navigation";
import ProtectedRoute from "@/components/layout/ProtectedRoute";
import toast from "react-hot-toast";
import { userService } from "@/services/userService";

export default function AdminAccountPage() {
    const { user, logout, updateUserInfo } = useAuth();
    const router = useRouter();

    const [isEditing, setIsEditing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [editForm, setEditForm] = useState({
        firstName: "",
        lastName: "",
        email: ""
    });

    useEffect(() => {
        if (user) {
            setEditForm({
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email
            });
        }
    }, [user]);

    const handleLogout = async () => {
        await logout();
        toast.success("Logged out successfully");
        router.push("/login");
    };

    const handleEdit = () => {
        setIsEditing(true);
    };

    const handleCancel = () => {
        if (user) {
            setEditForm({
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email
            });
        }
        setIsEditing(false);
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            const res = await userService.updateAccount(editForm);
            updateUserInfo({
                firstName: res.data.firstName,
                lastName: res.data.lastName,
                email: res.data.email
            });
            setIsEditing(false);
            toast.success("Account updated successfully! ✅");
        } catch (error: any) {
            toast.error(error.response?.data?.error || "Failed to update account");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <ProtectedRoute allowedRoles={["restaurant_admin", "system_admin"]}>
            <div className="max-w-2xl mx-auto px-4 py-8">
                <h1 className="text-2xl font-bold mb-6">My Account</h1>

                {/* Personal Info */}
                <div className="bg-bg-card rounded-xl p-5 mb-6">
                    <h2 className="font-semibold mb-4 text-text-secondary text-sm uppercase tracking-wide">Personal Info</h2>

                    {isEditing ? (
                        <form onSubmit={handleSave} className="space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="text-xs text-text-secondary block mb-1">First Name</label>
                                    <input
                                        type="text"
                                        value={editForm.firstName}
                                        onChange={(e) => setEditForm(prev => ({ ...prev, firstName: e.target.value }))}
                                        required
                                        className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-2 text-sm text-text-primary focus:outline-none focus:border-accent"
                                    />
                                </div>
                                <div>
                                    <label className="text-xs text-text-secondary block mb-1">Last Name</label>
                                    <input
                                        type="text"
                                        value={editForm.lastName}
                                        onChange={(e) => setEditForm(prev => ({ ...prev, lastName: e.target.value }))}
                                        required
                                        className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-2 text-sm text-text-primary focus:outline-none focus:border-accent"
                                    />
                                </div>
                            </div>
                            <div>
                                <label className="text-xs text-text-secondary block mb-1">Email</label>
                                <input
                                    type="email"
                                    value={editForm.email}
                                    onChange={(e) => setEditForm(prev => ({ ...prev, email: e.target.value }))}
                                    required
                                    className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-2 text-sm text-text-primary focus:outline-none focus:border-accent"
                                />
                            </div>
                            <div className="flex gap-3 pt-2">
                                <button
                                    type="submit"
                                    disabled={isLoading}
                                    className="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-2 rounded-lg text-sm disabled:opacity-50"
                                >
                                    {isLoading ? "Saving..." : "Save Changes"}
                                </button>
                                <button
                                    type="button"
                                    onClick={handleCancel}
                                    disabled={isLoading}
                                    className="flex-1 bg-bg-elevated hover:bg-bg-card border border-bg-elevated text-text-primary font-semibold py-2 rounded-lg text-sm"
                                >
                                    Cancel
                                </button>
                            </div>
                        </form>
                    ) : (
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
                                <span className="bg-accent/20 text-accent text-xs font-medium px-2.5 py-1 rounded-full uppercase">
                                    {user?.role?.replace('_', ' ')}
                                </span>
                            </div>
                        </div>
                    )}
                </div>

                {/* Action Buttons */}
                {!isEditing && (
                    <div className="grid grid-cols-2 gap-5">
                        <button
                            onClick={handleEdit}
                            className="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-xl"
                        >
                            Edit Profile
                        </button>
                        <button
                            onClick={handleLogout}
                            className="w-full bg-danger hover:bg-red-400 text-white font-semibold py-3 rounded-xl"
                        >
                            Logout
                        </button>
                    </div>
                )}
            </div>
        </ProtectedRoute>
    );
}
