"use client"

import { useAuth } from "@/context/AuthContext";
import { mealLogService } from "@/services/mealLogService";
import { MenuDish } from "@/types"
import toast from "react-hot-toast";

interface DishDetailModalProps {
    dish: MenuDish;
    onClose: () => void
    dailyCalorieTarget?: number;
}

export default function DishDetailModal({ dish, onClose, dailyCalorieTarget }: DishDetailModalProps) {
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

    const budgetPercentage = dailyCalorieTarget
        ? Math.round((dish.calories / dailyCalorieTarget) * 100)
        : null;

    return (
        <>
            {/* Backdrop */}
            <div className="fixed inset-0 bg-black/60 z-50" onClick={onClose} />

            {/* Modal — bottom-sheet on mobile, centered on desktop */}
            <div className="fixed z-50 inset-x-0 bottom-0 md:inset-0 md:flex md:items-center md:justify-center">
                <div className="bg-bg-card rounded-t-2xl md:rounded-2xl max-h-[85vh] overflow-y-auto w-full md:max-w-lg">
                    {/* Hero Image */}
                    <div className="aspect-video bg-bg-elevated relative">
                        {dish.imageUrl ? (
                            <img src={dish.imageUrl} alt={dish.name} className="w-full h-full object-cover" />
                        ) : (
                            <div className="w-full h-full flex items-center justify-center text-6xl">🍛</div>
                        )}
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
                        {/* Traffic Light Bar (only if user has a profile) */}
                        {budgetPercentage !== null && (
                            <div className="mt-4 bg-bg-elevated rounded-lg p-3">
                                <div className="flex justify-between text-sm mb-2">
                                    <span className="text-text-secondary">Daily calorie budget</span>
                                    <span className={`font-medium ${budgetPercentage <= 30 ? "text-success" :
                                        budgetPercentage <= 50 ? "text-warning" : "text-danger"
                                        }`}>
                                        {budgetPercentage}%
                                    </span>
                                </div>
                                <div className="w-full bg-bg-primary rounded-full h-2">
                                    <div
                                        className={`h-2 rounded-full transition-all ${budgetPercentage <= 30 ? "bg-success" :
                                            budgetPercentage <= 50 ? "bg-warning" : "bg-danger"
                                            }`}
                                        style={{ width: `${Math.min(budgetPercentage, 100)}%` }}
                                    />
                                </div>
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