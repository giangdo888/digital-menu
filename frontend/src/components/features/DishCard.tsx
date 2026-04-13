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
        
        const pCal = ((accumulator.calories + dish.calories) / profile.dailyCaloriesTarget) * 100;
        const pPro = ((accumulator.protein + dish.proteinG) / profile.dailyProteinG) * 100;
        const pCarb = ((accumulator.carbs + dish.carbsG) / profile.dailyCarbsG) * 100;
        const pFat = ((accumulator.fat + dish.fatG) / profile.dailyFatG) * 100;
        
        const maxP = Math.max(pCal, pPro, pCarb, pFat);
        
        if (maxP <= 80) return "bg-success";
        if (maxP <= 100) return "bg-warning";
        return "bg-danger";
    }

    const trafficLight = getTrafficLight();

    return (
        <button
            className="bg-bg-card rounded-xl p-3 flex gap-3 text-left hover:ring-1 hover:ring-accent cursor-pointer trasition-all w-full"
            onClick={onClick}
        >
            {/* Dish Image */}
            <div className="w-20 h-20 md:w-24 md:h-24 bg-bg-elevated rounded-lg flex-shrink-0 relative overflow-hidden">
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
                <h3 className="font-semibold text-text-primary text-sm md:text-base truncate">
                    {dish.name}
                </h3>
                <p className="text-accent font-medium text-sm mt-0.5">
                    ${dish.price.toFixed(2)}
                </p>

                {/* Compact Nutrition Badges */}
                <div className="flex gap-2 mt-2 flex-wrap">
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded text-text-secondary">
                        {Math.round(dish.calories)} cal
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded text-text-secondary">
                        {Math.round(dish.proteinG)} P
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded text-text-secondary">
                        {Math.round(dish.carbsG)} C
                    </span>
                    <span className="text-xs bg-bg-elevated px-2 py-0.5 rounded text-text-secondary">
                        {Math.round(dish.fatG)} F
                    </span>
                </div>
            </div>
        </button>
    );
}