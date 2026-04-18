"use client";
import { useState, useEffect } from "react";
import { UserProfile, CreateProfileRequest } from "@/types";
import { userService } from "@/services/userService";
import toast from "react-hot-toast";
import { useAuth } from "@/context/AuthContext";
import { useRouter } from "next/navigation";

export default function ProfilePage() {
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isEditing, setIsEditing] = useState(false);
    const [form, setForm] = useState<CreateProfileRequest>({
        gender: "male",
        dateOfBirth: "2000-01-01",
        heightCm: 170,
        currentWeightKg: 70,
        weeklyWeightGoal: 0,
        activityLevel: "sedentary",
    });

    const activityLevels = {
        sedentary: "Sedentary",
        lightly_active: "Light",
        moderately_active: "Moderate",
        very_active: "Active",
        extra_active: "Extra",
    };

    const { user, logout, updateUserInfo } = useAuth();
    const router = useRouter();

    const handleLogout = async () => {
        await logout();
        toast.success("Logged out successfully");
        router.push("/login");
    };

    const handleEdit = () => {
        if (profile) {
            setForm({
                gender: profile.gender,
                dateOfBirth: profile.dateOfBirth,
                heightCm: profile.heightCm,
                currentWeightKg: profile.currentWeightKg,
                weeklyWeightGoal: profile.weeklyWeightGoal,
                activityLevel: profile.activityLevel,
            });
        }
        setIsEditing(true);
    };

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const response = await userService.getProfile();
                setProfile(response.data);
            } catch (error) {
                setIsEditing(true);
            } finally {
                setIsLoading(false);
            }
        };

        fetchProfile();
    }, []);

    // Generic form update handler
    const updateField = (field: keyof CreateProfileRequest, value: string | number) => {
        setForm((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const response = profile
                ? await userService.updateProfile(form)
                : await userService.createProfile(form);
            setProfile(response.data);
            updateUserInfo({ hasProfile: true });
            setIsEditing(false);
            toast.success("Profile saved! ✅");
        } catch {
            toast.error("Failed to save profile");
        }
    }

    const handleClose = () => {
        setIsEditing(false);
    }

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    // ── Display Profile ──
    if (profile && !isEditing) {
        return (
            <div className="max-w-2xl mx-auto px-4 py-8">
                <h1 className="text-2xl font-bold">My Profile</h1>
                <p className="text-text-secondary mb-6">{user?.firstName} {user?.lastName}</p>
                {/* Calculated Stats */}
                <div className="grid grid-cols-3 gap-3 mb-6">
                    {[
                        { label: "BMI", value: profile.bmi.toFixed(1), sub: profile.bmiCategory },
                        {
                            label: "Activity",
                            value: activityLevels[profile.activityLevel as keyof typeof activityLevels] || profile.activityLevel,
                            sub: "Level"
                        },
                        { label: "Goal", value: profile.dietaryGoal, sub: `${profile.weeklyWeightGoal > 0 ? "+" : ""}${profile.weeklyWeightGoal} kg/week` },
                    ].map((stat) => {
                        const valStr = stat.value.toString();
                        const fontSize = valStr.length > 12 ? "text-[10px]" : valStr.length > 8 ? "text-sm" : "text-lg";
                        
                        return (
                            <div key={stat.label} className="bg-bg-card rounded-xl p-3 text-center flex flex-col justify-center min-w-0">
                                <p className={`font-bold text-accent leading-tight ${fontSize} transition-all duration-200`} title={valStr}>
                                    {stat.value}
                                </p>
                                <p className="text-[10px] text-text-secondary uppercase mt-1 truncate">{stat.label}</p>
                                <p className="text-[9px] text-text-secondary line-clamp-1">{stat.sub}</p>
                            </div>
                        );
                    })}
                </div>
                {/* Daily Targets */}
                <div className="bg-bg-card rounded-xl p-5 mb-6">
                    <h2 className="font-semibold mb-3">Daily Nutrition Targets</h2>
                    <div className="grid grid-cols-4 gap-3">
                        {[
                            { label: "Calories", value: Math.round(profile.dailyCaloriesTarget), unit: "cal" },
                            { label: "Protein", value: Math.round(profile.dailyProteinG), unit: "g" },
                            { label: "Carbs", value: Math.round(profile.dailyCarbsG), unit: "g" },
                            { label: "Fat", value: Math.round(profile.dailyFatG), unit: "g" },
                        ].map((n) => (
                            <div key={n.label} className="text-center">
                                <p className="text-lg font-bold">{n.value}<span className="text-xs text-text-secondary ml-0.5">{n.unit}</span></p>
                                <p className="text-xs text-text-secondary">{n.label}</p>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Action butttons */}
                <div className="grid grid-cols-2 gap-5">
                    <button onClick={handleEdit} className="bg-accent hover:bg-accent-hover text-white px-6 py-2 rounded-lg">
                        Edit Profile
                    </button>
                    <button onClick={handleLogout} className="bg-danger hover:bg-red-400 text-white px-6 py-2 rounded-lg">
                        Logout
                    </button>
                </div>
            </div>
        );
    }
    // ── Setup / Edit Form ──
    return (
        <div className="max-w-lg mx-auto px-4 py-8">
            <h1 className="text-2xl font-bold mb-6">{profile ? "Edit Profile" : "Set Up Your Profile"}</h1>
            <p className="text-text-secondary mb-6">
                We need these details to calculate your BMI, daily calorie target, and personalised nutrition feedback.
            </p>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="text-sm text-text-secondary block mb-1">Gender</label>
                        <select value={form.gender} onChange={(e) => updateField("gender", e.target.value)}
                            className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent">
                            <option value="male">Male</option>
                            <option value="female">Female</option>
                        </select>
                    </div>
                    <div>
                        <label className="text-sm text-text-secondary block mb-1">Date of Birth</label>
                        <input type="date" value={form.dateOfBirth} onChange={(e) => updateField("dateOfBirth", e.target.value)}
                            className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                    </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="text-sm text-text-secondary block mb-1">Height (cm)</label>
                        <input type="number" value={form.heightCm} onChange={(e) => updateField("heightCm", +e.target.value)}
                            className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                    </div>
                    <div>
                        <label className="text-sm text-text-secondary block mb-1">Current Weight (kg)</label>
                        <input type="number" value={form.currentWeightKg} onChange={(e) => updateField("currentWeightKg", +e.target.value)}
                            className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                    </div>
                </div>
                <div>
                    <label className="text-sm text-text-secondary block mb-1">Weekly Weight Goal (kg/week)</label>
                    <select value={form.weeklyWeightGoal} onChange={(e) => updateField("weeklyWeightGoal", +e.target.value)}
                        className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent">
                        <option value="-1">lose 1kg/week</option>
                        <option value="-0.75">lose 0.75kg/week</option>
                        <option value="-0.5">lose 0.5kg/week</option>
                        <option value="-0.25">lose 0.25kg/week</option>
                        <option value="0">maintain weight</option>
                        <option value="0.25">gain 0.25kg/week</option>
                        <option value="0.5">gain 0.5kg/week</option>
                        <option value="0.75">gain 0.75kg/week</option>
                        <option value="1">gain 1kg/week</option>
                    </select>
                </div>
                <div>
                    <label className="text-sm text-text-secondary block mb-1">Activity Level</label>
                    <select value={form.activityLevel} onChange={(e) => updateField("activityLevel", e.target.value)}
                        className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent">
                        <option value="sedentary">Sedentary (Little exercise)</option>
                        <option value="lightly_active">Lightly Active (1-3 days/week)</option>
                        <option value="moderately_active">Moderately Active (3-5 days/week)</option>
                        <option value="very_active">Very Active (6-7 days/week)</option>
                        <option value="extra_active">Extra Active (Athlete / Physical job)</option>
                    </select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                    <button type="submit" className="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-xl">
                        Save Profile
                    </button>
                    <button type="button" onClick={handleClose} className="w-full bg-danger hover:bg-red-400 text-white font-semibold py-3 rounded-xl">
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}
