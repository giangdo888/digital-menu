"use client"

import { useAuth } from "@/context/AuthContext";
import { mealLogService } from "@/services/mealLogService";
import { MenuDish, UserProfile } from "@/types"
import { useEffect, useRef, useState } from "react";
import toast from "react-hot-toast";

interface DishDetailModalProps {
    dish: MenuDish;
    onClose: () => void;
    profile?: UserProfile | null;
    accumulator?: { calories: number; protein: number; carbs: number; fat: number };
    onMealLogged?: () => void;
}

export default function DishDetailModal({ dish, onClose, profile, accumulator, onMealLogged }: DishDetailModalProps) {
    const { isAuthenticated, isCustomer } = useAuth();
    const [consumedAt, setConsumedAt] = useState(new Date().toISOString().split("T")[0]);
    const dateInputRef = useRef<HTMLInputElement | null>(null);

    const logMeal = async (date: string) => {
        try {
            await mealLogService.create({ dishId: dish.id, consumedAt: date });
            toast.success("Meal logged! 🎉");
            onMealLogged?.();
            onClose();
        } catch {
            const { formatApiValidationErrors } = await import("@/lib/apiErrors");
            // @ts-ignore
            const msg = formatApiValidationErrors((arguments[0]) || (new Error("Failed to log meal")));
            toast.error(msg || "Failed to log meal");
        }
    };

    const handleBackdropClick = () => {
        onClose();
    };

    return (
        <>
            {/* Backdrop */}
            <div className="fixed inset-0 bg-stone-900/40 z-[100]" onClick={handleBackdropClick} />
 
            {/* Modal — bottom-sheet on mobile, centered on desktop */}
            <div
                className="fixed z-[101] inset-x-0 bottom-0 md:top-16 md:bottom-0 md:left-0 md:right-0 md:flex md:items-center md:justify-center md:py-8"
                onClick={onClose}
            >
                <div
                    className="bg-bg-card border border-border rounded-t-sm md:rounded-sm max-h-[85vh] overflow-y-auto w-full md:max-w-lg pb-20 md:pb-0 animate-fade-in-up"
                    onClick={e => e.stopPropagation()}
                >
                    {/* Hero Image */}
                    <div className="aspect-video bg-bg-elevated relative">
                        <img
                            src={dish.imageUrl || "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=400&q=80"}
                            alt={dish.name}
                            className="w-full h-full object-cover"
                        />
                        <button
                            onClick={onClose}
                            className="absolute top-3 right-3 bg-stone-900/50 text-white w-8 h-8 rounded-sm cursor-pointer flex items-center justify-center"
                        >
                            ✕
                        </button>
                    </div>

                    <div className="p-5">
                        {/* Title + Price */}
                        <h2 className="text-3xl font-serif font-bold tracking-tight">{dish.name}</h2>
                        <p className="text-accent font-bold text-lg mt-1 tracking-tight">${dish.price.toFixed(2)}</p>
                        {/* Nutrition Grid */}
                        <div className="grid grid-cols-4 gap-2 mt-4">
                            {[
                                { label: "Calories", value: Math.round(dish.calories), unit: "cal" },
                                { label: "Protein", value: Math.round(dish.proteinG), unit: "g" },
                                { label: "Carbs", value: Math.round(dish.carbsG), unit: "g" },
                                { label: "Fat", value: Math.round(dish.fatG), unit: "g" },
                            ].map((item) => (
                                <div key={item.label} className="bg-bg-elevated rounded-sm p-3 text-center">
                                    <p className="text-lg font-bold text-accent">
                                        {item.value}
                                        <span className="text-xs text-text-secondary ml-0.5">{item.unit}</span>
                                    </p>
                                    <p className="text-xs text-text-secondary mt-0.5">{item.label}</p>
                                </div>
                            ))}
                        </div>
                        {/* Traffic Light Bars (only if user has a profile) */}
                        {profile && accumulator && (
                            <div className="mt-4 bg-bg-elevated rounded-sm p-3 space-y-3">
                                <h3 className="text-sm font-semibold text-text-secondary">Projected Daily Totals (if eaten)</h3>
                                {[
                                    { label: "Calories", base: accumulator.calories, added: dish.calories, tgt: profile.dailyCaloriesTarget },
                                    { label: "Protein", base: accumulator.protein, added: dish.proteinG, tgt: profile.dailyProteinG },
                                    { label: "Carbs", base: accumulator.carbs, added: dish.carbsG, tgt: profile.dailyCarbsG },
                                    { label: "Fat", base: accumulator.fat, added: dish.fatG, tgt: profile.dailyFatG },
                                ].map(macro => {
                                    const totalVal = macro.base + macro.added;
                                    const isZero = macro.added === 0;

                                    // Calculate range based on user's weight goal
                                    let minGreen = 90;
                                    let maxGreen = 105;
                                    if (profile.weeklyWeightGoal < 0) { // Lose 
                                        minGreen = 85; maxGreen = 100;
                                    } else if (profile.weeklyWeightGoal > 0) { // Gain
                                        minGreen = 95; maxGreen = 110;
                                    }

                                    const totalOverallPct = (totalVal / macro.tgt) * 100;
                                    const isOverLimit = totalOverallPct > maxGreen;
                                    const isSweetSpot = totalOverallPct >= minGreen && !isOverLimit;

                                    const basePct = Math.min((macro.base / macro.tgt) * 100, 100);
                                    // To calculate the added percentage without overflowing 100%
                                    const totalBarPct = Math.min((totalVal / macro.tgt) * 100, 100);
                                    const addedPct = totalBarPct - basePct;

                                    let addedColor = "bg-success"; // Green progress by default
                                    if (isZero) addedColor = "bg-text-secondary"; // Gray if doesn't contain macro
                                    else if (isOverLimit) addedColor = "bg-danger"; // Red if goes too far

                                    // Color Logic for Status (Percentage vs Added value)
                                    const dangerThreshold = maxGreen + 10;

                                    // + Added value color: Green for everything "Safe" (up to maxGreen)
                                    let addedValueColor = "text-text-primary";
                                    if (isZero) addedValueColor = "text-text-secondary";
                                    else if (totalOverallPct > dangerThreshold) addedValueColor = "text-danger";
                                    else if (totalOverallPct > maxGreen) addedValueColor = "text-warning";
                                    else addedValueColor = "text-success";

                                    // % Total Percentage color: Green ONLY in sweetspot
                                    let pctColor = "text-text-secondary";
                                    if (isZero) pctColor = "text-text-secondary";
                                    else if (totalOverallPct > dangerThreshold) pctColor = "text-danger";
                                    else if (totalOverallPct > maxGreen) pctColor = "text-warning";
                                    else if (isSweetSpot) pctColor = "text-success";
                                    else pctColor = "text-text-secondary";


                                    return (
                                        <div key={macro.label}>
                                            <div className="flex justify-between text-sm mb-1">
                                                <span className="text-text-secondary">{macro.label}</span>
                                                <div className="font-medium flex items-baseline gap-1">
                                                    <span className="text-text-primary">{Math.round(macro.base)}</span>
                                                    <span className="text-text-secondary text-[10px]">+</span>
                                                    <span className={`font-bold ${addedValueColor}`}>{Math.round(macro.added)}</span>
                                                    <span className="text-text-secondary text-xs ml-0.5">/ {Math.round(macro.tgt)}</span>
                                                    <span className={`ml-2 font-bold ${pctColor}`}>
                                                        ({Math.round((totalVal / macro.tgt) * 100)}%)
                                                    </span>
                                                </div>
                                            </div>
                                            <div className="w-full bg-border rounded-full h-2 flex overflow-hidden">
                                                <div className="h-full bg-accent transition-all" style={{ width: `${basePct}%` }} />
                                                <div className={`h-full ${addedColor} transition-all`} style={{ width: `${addedPct}%` }} />
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                        )}
                    </div>


                    {/* Ingredients */}
                    {dish.ingredients.length > 0 && (
                        <div className="p-5 mt-4">
                            <h3 className="text-sm font-semibold text-text-secondary mb-2">Ingredients</h3>
                            <div className="flex flex-wrap gap-1.5">
                                {dish.ingredients.map((ing, i) => (
                                    <span key={i} className="text-xs bg-bg-elevated px-2 py-1 rounded-sm text-text-secondary">
                                        {ing.name} ({ing.amountInGrams}g)
                                    </span>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Action Buttons */}
                    <div className="p-5 mt-6 relative overflow-visible">
                        {isAuthenticated && isCustomer && (
                            <>
                                <div className="flex gap-3 items-center">
                                    <input
                                        ref={dateInputRef}
                                        type="date"
                                        value={consumedAt}
                                        max={new Date().toISOString().split("T")[0]}
                                        onChange={(e) => setConsumedAt(e.currentTarget.value)}
                                        className="min-w-0 w-48 md:w-64 bg-bg-primary border border-border rounded-sm px-3 py-2 text-text-primary focus:outline-none focus:border-accent"
                                    />
                                    <button
                                        onClick={() => void logMeal(consumedAt)}
                                        className="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold px-5 py-3 rounded-sm transition-colors tracking-wide"
                                    >
                                        Log
                                    </button>
                                </div>
                            </>
                        )}
                    </div>
                </div>
            </div>
        </>
    )
}