import { MenuDish, UserProfile } from "@/types";

interface DishCardProps {
    dish: MenuDish;
    onClick: () => void;
    profile?: UserProfile | null;
    accumulator?: { calories: number; protein: number; carbs: number; fat: number };
}

export default function DishCard({ dish, onClick, profile, accumulator }: DishCardProps) {
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
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary">
                        {Math.round(dish.calories)} cal
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary">
                        {Math.round(dish.proteinG)} P
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary">
                        {Math.round(dish.carbsG)} C
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded-sm text-text-secondary">
                        {Math.round(dish.fatG)} F
                    </span>
                </div>
            </div>
        </button>
    );
}