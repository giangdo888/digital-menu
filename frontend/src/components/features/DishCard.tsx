import { MenuDish } from "@/types";

interface DishCardProps {
    dish: MenuDish;
    onClick: () => void;

    //optional: daily calorie target for traffic light system
    dailyCalorieTarget?: number;
}

export default function DishCard({ dish, onClick, dailyCalorieTarget }: DishCardProps) {
    const getTrafficLight = () => {
        if (!dailyCalorieTarget) return null;
        const percentage = (dish.calories / dailyCalorieTarget) * 100;
        if (percentage <= 30) return "bg-success";      //Green
        if (percentage <= 50) return "bg-warning";      //Yellow
        return "bg-danger";                             //Red
    }

    const trafficLight = getTrafficLight();

    return (
        <button
            className="bg-bg-card rounded-xl p-3 flex gap-3 text-left hover:ring-1 hover:ring-accent trasition-all w-full"
            onClick={onClick}
        >
            {/* Dish Image */}
            <div className="w-20 h-20 md:w-24 md:h-24 bg-bg-elevated rounded-lg flex-shrink-0 relative overflow-hidden">
                {dish.imageUrl ? (
                    <img
                        src={dish.imageUrl}
                        alt={dish.name}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <div className="w-full h-full flex items-center justify-center text-2xl">🍛</div>
                )}

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