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
    const caloriePct = Math.round((totals.calories / calorieTarget) * 100);
    const calColor = caloriePct <= 80 ? "#10B981" : caloriePct <= 100 ? "#F59E0B" : "#EF4444";

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
                                <circle cx="60" cy="60" r="50" fill="none" stroke={calColor} strokeWidth="10"
                                    strokeDasharray={`${Math.min(caloriePct, 100) * 3.14} 314`}
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
                        ].map((macro) => {
                            const pct = Math.round((macro.current / macro.target) * 100);
                            let colorCls = "bg-success";
                            if (pct > 80 && pct <= 100) colorCls = "bg-warning";
                            else if (pct > 100) colorCls = "bg-danger";

                            return (
                                <div key={macro.label} className="mb-3">
                                    <div className="flex justify-between text-xs text-text-secondary mb-1">
                                        <span>{macro.label}</span>
                                        <span className={pct > 100 ? "text-danger font-medium" : ""}>
                                            {Math.round(macro.current)}g / {Math.round(macro.target)}g
                                        </span>
                                    </div>
                                    <div className="w-full bg-bg-elevated rounded-full h-2">
                                        <div className={`${colorCls} h-2 rounded-full transition-all`}
                                            style={{ width: `${Math.min(pct, 100)}%` }} />
                                    </div>
                                </div>
                            );
                        })}
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
                        {meals.map((meal) => {
                            let dotColor = "bg-success";
                            if (profile) {
                                const pCal = (Number(meal.calories) / profile.dailyCaloriesTarget) * 100;
                                const pPro = (Number(meal.proteinG) / profile.dailyProteinG) * 100;
                                const pCarb = (Number(meal.carbsG) / profile.dailyCarbsG) * 100;
                                const pFat = (Number(meal.fatG) / profile.dailyFatG) * 100;
                                const maxP = Math.max(pCal, pPro, pCarb, pFat);
                                if (maxP > 80 && maxP <= 100) dotColor = "bg-warning";
                                else if (maxP > 100) dotColor = "bg-danger";
                            }
                            return (
                                <div key={meal.id} className="bg-bg-card rounded-xl p-4 flex items-center justify-between">
                                    <div className="flex items-center gap-3">
                                        <div className={`w-3 h-3 rounded-full ${dotColor} flex-shrink-0 shadow-sm`} />
                                        <div>
                                            <p className="font-medium">{meal.dishName}</p>
                                            <p className="text-sm text-text-secondary">
                                                {new Date(meal.createdAt.endsWith('Z') ? meal.createdAt : meal.createdAt + 'Z').toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-4">
                                        <span className="text-white font-semibold">{meal.calories} cal</span>
                                        <button onClick={() => handleDelete(meal.id)} className="text-danger justify-end hover:text-red-500 transition-colors p-1" title="Remove meal">
                                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="w-5 h-5">
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                                            </svg>
                                        </button>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            </div>
        </div>
    );
}
