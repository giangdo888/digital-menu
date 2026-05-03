"use client"

import { MenuDish, UserProfile } from "@/types";
import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";

interface DishCardProps {
    dish: MenuDish;
    onClick: () => void;
    profile?: UserProfile | null;
    accumulator?: { calories: number; protein: number; carbs: number; fat: number };
}

export default function DishCard({ dish, onClick, profile, accumulator }: DishCardProps) {
    const [visibleTooltip, setVisibleTooltip] = useState<Record<string, boolean>>({});
    const chipsRef = useRef<HTMLDivElement | null>(null);
    const [portalTooltip, setPortalTooltip] = useState<{ key: string; left: number; top: number } | null>(null);

    useEffect(() => {
        const handleOutside = (e: Event) => {
            if (!chipsRef.current) return;
            if (!chipsRef.current.contains(e.target as Node)) {
                setVisibleTooltip({});
                setPortalTooltip(null);
            }
        };
        document.addEventListener("pointerdown", handleOutside);
        document.addEventListener("touchstart", handleOutside);
        return () => {
            document.removeEventListener("pointerdown", handleOutside);
            document.removeEventListener("touchstart", handleOutside);
        };
    }, []);

    const toggleTooltip = (key: string, el?: HTMLElement) => {
        setVisibleTooltip((prev) => {
            const next = { ...prev, [key]: !prev[key] };
            // If opening, compute portal position
            if (next[key] && el) {
                const rect = el.getBoundingClientRect();
                setPortalTooltip({ key, left: rect.left + rect.width / 2, top: rect.top });
            } else if (!next[key]) {
                setPortalTooltip(null);
            }
            return next;
        });
    };

    const openTooltip = (key: string, el?: HTMLElement) => {
        setVisibleTooltip((prev) => ({ ...prev, [key]: true }));
        if (el) {
            const rect = el.getBoundingClientRect();
            setPortalTooltip({ key, left: rect.left + rect.width / 2, top: rect.top });
        }
    };

    const closeTooltip = (key: string) => {
        setVisibleTooltip((prev) => ({ ...prev, [key]: false }));
        setPortalTooltip(null);
    };

    const getTrafficLight = () => {
        if (!profile || !accumulator) return null;


        // Logic: Only turn red if THIS dish contributes significantly (>1% of daily target)
        // when a budget is already exceeded. This prevents "All Red" syndrome.
        const getImpactPct = (base: number, added: number, target: number) => {
            const total = base + added;
            const pct = (total / target) * 100;
            // If already over, only penalize if this dish itself is heavy (>1% of target)
            if (base > target && (added / target) < 0.01) return 0;
            return pct;
        };

        const pCal = getImpactPct(accumulator.calories, dish.calories, profile.dailyCaloriesTarget);
        const pPro = getImpactPct(accumulator.protein, dish.proteinG, profile.dailyProteinG);
        const pCarb = getImpactPct(accumulator.carbs, dish.carbsG, profile.dailyCarbsG);
        const pFat = getImpactPct(accumulator.fat, dish.fatG, profile.dailyFatG);

        const maxP = Math.max(pCal, pPro, pCarb, pFat);

        if (maxP <= 80) return "bg-success";
        if (maxP <= 100) return "bg-warning";
        return "bg-danger";
    }

    const trafficLight = getTrafficLight();

    return (
        <button
            className="bg-bg-card border border-border rounded-sm p-3 flex gap-3 text-left hover:border-accent cursor-pointer transition-all duration-300 ease-out w-full shadow-luxury hover:shadow-luxury-hover hover:-translate-y-1 hover:scale-[1.01]"
            onClick={onClick}
        >
            {/* Dish Image */}
            <div className="w-20 h-20 md:w-24 md:h-24 bg-bg-elevated rounded-sm flex-shrink-0 relative overflow-hidden">
                <img
                    src={dish.imageUrl || "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=400&q=80"}
                    alt={dish.name}
                    className="w-full h-full object-cover"
                />

                {/* Traffic Light Dot */}
                {trafficLight && (
                    <div className={`absolute top-1.5 left-1.5 w-3 h-3 rounded-full ${trafficLight} ring-2 ring-bg-card`} />
                )}
            </div>
            {/* Portal tooltip rendered at body to escape stacking contexts */}
            {portalTooltip && createPortal(
                <div style={{ position: "fixed", left: portalTooltip.left, top: portalTooltip.top, transform: "translate(-50%, calc(-100% - 8px))" }} className="z-[99999] pointer-events-auto">
                    <div className="bg-bg-card border border-border rounded-sm p-3 text-sm text-text-primary shadow-lg w-64">
                        {portalTooltip.key === "calories" && (
                            <>
                                <div className="font-bold text-base mb-1">Calories</div>
                                <div className="text-xs text-text-secondary">Measure of the energy provided by this serving. Track calories to manage daily energy balance (weight loss, maintenance, or gain).</div>
                            </>
                        )}
                        {portalTooltip.key === "protein" && (
                            <>
                                <div className="font-bold text-base mb-1">Protein</div>
                                <div className="text-xs text-text-secondary">Essential macronutrient for building and repairing muscle and tissues. Supports satiety and metabolic health, aim to meet your daily protein target.</div>
                            </>
                        )}
                        {portalTooltip.key === "carbs" && (
                            <>
                                <div className="font-bold text-base mb-1">Carbohydrates</div>
                                <div className="text-xs text-text-secondary">Primary energy source for the body and brain. Includes sugars, starches and fiber, fiber slows digestion and supports gut health.</div>
                            </>
                        )}
                        {portalTooltip.key === "fat" && (
                            <>
                                <div className="font-bold text-base mb-1">Fat</div>
                                <div className="text-xs text-text-secondary">Concentrated energy and carrier for fat‑soluble vitamins. Important for hormones, cell structure, and satiety; prioritise unsaturated fats where possible.</div>
                            </>
                        )}
                    </div>
                </div>,
                document.body
            )}

            {/* Dish Info */}
            <div className="flex-1 min-w-0">
                <h3 className="font-serif font-bold text-text-primary text-base md:text-lg truncate">
                    {dish.name}
                </h3>
                <p className="text-accent font-semibold text-sm mt-0.5 tracking-tight">
                    ${dish.price.toFixed(2)}
                </p>

                {/* Compact Nutrition Badges */}
                <div className="flex gap-2 mt-2 flex-wrap">
                    {/* Calories */}
                    <div className="relative group">
                        <span
                            onClick={(e) => { e.stopPropagation(); toggleTooltip("calories", e.currentTarget as HTMLElement); }}
                            onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); e.stopPropagation(); toggleTooltip("calories", e.currentTarget as HTMLElement); } }}
                            onMouseEnter={(e) => { openTooltip("calories", e.currentTarget as HTMLElement); }}
                            onMouseLeave={() => { closeTooltip("calories"); }}
                            role="button"
                            tabIndex={0}
                            className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary"
                        >
                            {Math.round(dish.calories)} cal
                        </span>
                        
                    </div>

                    {/* Protein */}
                    <div className="relative group">
                        <span
                            onClick={(e) => { e.stopPropagation(); toggleTooltip("protein", e.currentTarget as HTMLElement); }}
                            onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); e.stopPropagation(); toggleTooltip("protein", e.currentTarget as HTMLElement); } }}
                            onMouseEnter={(e) => { openTooltip("protein", e.currentTarget as HTMLElement); }}
                            onMouseLeave={() => { closeTooltip("protein"); }}
                            role="button"
                            tabIndex={0}
                            className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary"
                        >
                            {Math.round(dish.proteinG)} P
                        </span>
                        
                    </div>

                    {/* Carbs */}
                    <div className="relative group">
                        <span
                            onClick={(e) => { e.stopPropagation(); toggleTooltip("carbs", e.currentTarget as HTMLElement); }}
                            onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); e.stopPropagation(); toggleTooltip("carbs", e.currentTarget as HTMLElement); } }}
                            onMouseEnter={(e) => { openTooltip("carbs", e.currentTarget as HTMLElement); }}
                            onMouseLeave={() => { closeTooltip("carbs"); }}
                            role="button"
                            tabIndex={0}
                            className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary"
                        >
                            {Math.round(dish.carbsG)} C
                        </span>
                        
                    </div>

                    {/* Fat */}
                    <div className="relative group">
                        <span
                            onClick={(e) => { e.stopPropagation(); toggleTooltip("fat", e.currentTarget as HTMLElement); }}
                            onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); e.stopPropagation(); toggleTooltip("fat", e.currentTarget as HTMLElement); } }}
                            onMouseEnter={(e) => { openTooltip("fat", e.currentTarget as HTMLElement); }}
                            onMouseLeave={() => { closeTooltip("fat"); }}
                            role="button"
                            tabIndex={0}
                            className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary"
                        >
                            {Math.round(dish.fatG)} F
                        </span>
                        
                    </div>
                </div>
            </div>
        </button>
    );
}