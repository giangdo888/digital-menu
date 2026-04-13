"use client"

import { useAuth } from "@/context/AuthContext";
import { mealLogService } from "@/services/mealLogService";
import { MenuDish, UserProfile } from "@/types"
import toast from "react-hot-toast";

interface DishDetailModalProps {
    dish: MenuDish;
    onClose: () => void;
    profile?: UserProfile | null;
    accumulator?: { calories: number; protein: number; carbs: number; fat: number };
}

export default function DishDetailModal({ dish, onClose, profile, accumulator }: DishDetailModalProps) {
    const { isAuthenticated, isCustomer } = useAuth();

    const handleLogMeal = async () => {
        try {
            await mealLogService.create({ dishId: dish.id });
            toast.success("Meal logged! 🎉");
            onClose();
        } catch {
            toast.error("Failed to log meal");
        }
    };

    return (
        <>
            {/* Backdrop */}
            <div className="fixed inset-0 bg-black/60 z-50" onClick={onClose} />

            {/* Modal — bottom-sheet on mobile, centered on desktop */}
            <div className="fixed z-50 inset-x-0 bottom-0 md:inset-0 md:flex md:items-center md:justify-center">
                <div className="bg-bg-card rounded-t-2xl md:rounded-2xl max-h-[85vh] overflow-y-auto w-full md:max-w-lg">
                    {/* Hero Image */}
                    <div className="aspect-video bg-bg-elevated relative">
                        <img 
                            src={dish.imageUrl || "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=400&q=80"} 
                            alt={dish.name} 
                            className="w-full h-full object-cover" 
                        />
                        <button
                            onClick={onClose}
                            className="absolute top-3 right-3 bg-black/50 text-white w-8 h-8 rounded-full cursor-pointer flex items-center justify-center"
                        >
                            ✕
                        </button>
                    </div>

                    <div className="p-5">
                        {/* Title + Price */}
                        <h2 className="text-xl font-bold">{dish.name}</h2>
                        <p className="text-accent font-semibold mt-1">${dish.price.toFixed(2)}</p>
                        {/* Nutrition Grid */}
                        <div className="grid grid-cols-4 gap-2 mt-4">
                            {[
                                { label: "Calories", value: Math.round(dish.calories), unit: "cal" },
                                { label: "Protein", value: Math.round(dish.proteinG), unit: "g" },
                                { label: "Carbs", value: Math.round(dish.carbsG), unit: "g" },
                                { label: "Fat", value: Math.round(dish.fatG), unit: "g" },
                            ].map((item) => (
                                <div key={item.label} className="bg-bg-elevated rounded-lg p-3 text-center">
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
                            <div className="mt-4 bg-bg-elevated rounded-lg p-3 space-y-3">
                                <h3 className="text-sm font-semibold text-text-secondary">Projected Daily Totals (if eaten)</h3>
                                {[
                                    { label: "Calories", cur: accumulator.calories + dish.calories, tgt: profile.dailyCaloriesTarget },
                                    { label: "Protein", cur: accumulator.protein + dish.proteinG, tgt: profile.dailyProteinG },
                                    { label: "Carbs", cur: accumulator.carbs + dish.carbsG, tgt: profile.dailyCarbsG },
                                    { label: "Fat", cur: accumulator.fat + dish.fatG, tgt: profile.dailyFatG },
                                ].map(macro => {
                                    const pct = Math.round((macro.cur / macro.tgt) * 100);
                                    let colorCls = "bg-success";
                                    let textCls = "text-success";
                                    if (pct > 80 && pct <= 100) { colorCls = "bg-warning"; textCls = "text-warning"; }
                                    else if (pct > 100) { colorCls = "bg-danger"; textCls = "text-danger"; }

                                    return (
                                        <div key={macro.label}>
                                            <div className="flex justify-between text-sm mb-1">
                                                <span className="text-text-secondary">{macro.label}</span>
                                                <span className={`font-medium ${textCls}`}>
                                                    {Math.round(macro.cur)} / {Math.round(macro.tgt)} ({pct}%)
                                                </span>
                                            </div>
                                            <div className="w-full bg-bg-primary rounded-full h-2">
                                                <div
                                                    className={`h-2 rounded-full transition-all ${colorCls}`}
                                                    style={{ width: `${Math.min(pct, 100)}%` }}
                                                />
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
                                    <span key={i} className="text-xs bg-bg-elevated px-2 py-1 rounded text-text-secondary">
                                        {ing.name} ({ing.amountInGrams}g)
                                    </span>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Action Buttons */}
                    <div className="p-5 mt-6 flex gap-3">
                        {isAuthenticated && isCustomer && (
                            <button
                                onClick={handleLogMeal}
                                className="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-xl transition-colors"
                            >
                                Log This Meal
                            </button>
                        )}
                    </div>
                </div>
            </div>
        </>
    )
}