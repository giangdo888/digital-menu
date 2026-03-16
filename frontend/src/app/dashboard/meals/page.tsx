"use client";

import { useState, useEffect } from "react";
import { MealLog, UserProfile } from "@/types";
import { mealLogService } from "@/services/mealLogService";
import { userService } from "@/services/userService";
import toast from "react-hot-toast";

export default function MealsPage() {
    const [meals, setMeals] = useState<MealLog[]>([]);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [mealsRes, profileRes] = await Promise.all([
                    mealLogService.getMyLogs(),
                    userService.getProfile(),
                ]);
                setMeals(mealsRes.data);
                setProfile(profileRes.data);
            } catch {
                console.error("Failed to load data");
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, []);

    // ── Derived totals (recalculates automatically when meals changes) ──
    const totals = meals.reduce(
        (acc, meal) => ({
            calories: acc.calories + parseFloat(meal.calories),
            protein: acc.protein + parseFloat(meal.proteinG),
            carbs: acc.carbs + parseFloat(meal.carbsG),
            fat: acc.fat + parseFloat(meal.fatG),
        }),
        { calories: 0, protein: 0, carbs: 0, fat: 0 }
    );

    const handleDelete = async (id: string) => {
        try {
            await mealLogService.delete(parseInt(id));
            setMeals((prev) => prev.filter((m) => m.id !== id));
            toast.success("Meal removed");
        } catch {
            toast.error("Failed to remove meal");
        }
    };

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    const calorieTarget = profile?.dailyCaloriesTarget || 2000;
    const caloriePercent = Math.min((totals.calories / calorieTarget) * 100, 100);

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <h1 className="text-2xl font-bold mb-6">Today&apos;s Meals</h1>

            <div className="md:flex md:gap-8">
                {/* ── Left: Progress Ring + Macros ── */}
                <div className="md:w-1/3 mb-6 md:mb-0">
                    {/* Calorie Circle (simplified with CSS) */}
                    <div className="bg-bg-card rounded-xl p-6 text-center">
                        <div className="relative w-32 h-32 mx-auto mb-4">
                            <svg className="w-full h-full -rotate-90" viewBox="0 0 120 120">
                                <circle cx="60" cy="60" r="50" fill="none" stroke="#252836" strokeWidth="10" />
                                <circle cx="60" cy="60" r="50" fill="none" stroke="#10B981" strokeWidth="10"
                                    strokeDasharray={`${caloriePercent * 3.14} 314`}
                                    strokeLinecap="round" />
                            </svg>
                            <div className="absolute inset-0 flex flex-col items-center justify-center">
                                <span className="text-xl font-bold">{Math.round(totals.calories)}</span>
                                <span className="text-xs text-text-secondary">/ {Math.round(calorieTarget)} cal</span>
                            </div>
                        </div>

                        {/* Macro Bars */}
                        {[
                            { label: "Protein", current: totals.protein, target: profile?.dailyProteinG || 120 },
                            { label: "Carbs", current: totals.carbs, target: profile?.dailyCarbsG || 250 },
                            { label: "Fat", current: totals.fat, target: profile?.dailyFatG || 65 },
                        ].map((macro) => (
                            <div key={macro.label} className="mb-3">
                                <div className="flex justify-between text-xs text-text-secondary mb-1">
                                    <span>{macro.label}</span>
                                    <span>{Math.round(macro.current)}g / {Math.round(macro.target)}g</span>
                                </div>
                                <div className="w-full bg-bg-elevated rounded-full h-2">
                                    <div className="bg-accent h-2 rounded-full transition-all"
                                        style={{ width: `${Math.min((macro.current / macro.target) * 100, 100)}%` }} />
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* ── Right: Meal List ── */}
                <div className="md:flex-1">
                    <div className="space-y-3">
                        {meals.length === 0 && (
                            <p className="text-text-secondary text-center py-8">
                                No meals logged today. Browse a restaurant menu and tap &quot;Log This Meal&quot;!
                            </p>
                        )}
                        {meals.map((meal) => (
                            <div key={meal.id} className="bg-bg-card rounded-xl p-4 flex items-center justify-between">
                                <div>
                                    <p className="font-medium">{meal.dishName}</p>
                                    <p className="text-sm text-text-secondary">
                                        {new Date(meal.createdAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                                    </p>
                                </div>
                                <div className="flex items-center gap-4">
                                    <span className="text-accent font-semibold">{meal.calories} cal</span>
                                    <button onClick={() => handleDelete(meal.id)} className="text-danger text-sm hover:underline">
                                        Remove
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}
