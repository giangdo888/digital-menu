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
        bmiGoal: 22,
        activityLevel: "sedentary",
    });

    const activityLevels = {
        sedentary: "Sedentary",
        lightly_active: "Light",
        moderately_active: "Moderate",
        very_active: "Active",
        extra_active: "Extra",
    };

    const { logout } = useAuth();
    const router = useRouter();

    const handleLogout = async () => {
        await logout();
        toast.success("Logged out successfully");
        router.push("/login");
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
            setIsEditing(false);
            toast.success("Profile saved! ✅");
        } catch {
            toast.error("Failed to save profile");
        }
    }

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    // ── Display Profile ──
    if (profile && !isEditing) {
        return (
            <div className="max-w-2xl mx-auto px-4 py-8">
                <h1 className="text-2xl font-bold mb-6">My Profile</h1>
                {/* Calculated Stats */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
                    {[
                        { label: "BMI", value: profile.bmi.toFixed(1), sub: profile.bmiCategory },
                        { 
                            label: "Activity", 
                            value: activityLevels[profile.activityLevel as keyof typeof activityLevels] || profile.activityLevel,
                            sub: "Level" 
                        },
                        { label: "TDEE", value: `${Math.round(profile.tdee)} cal`, sub: "Daily expenditure" },
                        { label: "Goal", value: profile.dietaryGoal, sub: `${profile.weightGoal} kg` },
                    ].map((stat) => (
                        <div key={stat.label} className="bg-bg-card rounded-xl p-4 text-center">
                            <p className="text-2xl font-bold text-accent">{stat.value}</p>
                            <p className="text-xs text-text-secondary mt-1">{stat.label}</p>
                            <p className="text-xs text-text-secondary">{stat.sub}</p>
                        </div>
                    ))}
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
                    <button onClick={() => setIsEditing(true)} className="bg-accent hover:bg-accent-hover text-white px-6 py-2 rounded-lg">
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
                    <label className="text-sm text-text-secondary block mb-1">Target BMI</label>
                    <input type="number" step="0.1" value={form.bmiGoal} onChange={(e) => updateField("bmiGoal", +e.target.value)}
                        className="w-full bg-bg-card border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                    <p className="text-xs text-text-secondary mt-1">Normal BMI range: 18.5 – 24.9</p>
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
                <button type="submit" className="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-xl">
                    Save Profile
                </button>
            </form>
        </div>
    );
}
