"use client"

import DishCard from "@/components/features/DishCard";
import DishDetailModal from "@/components/features/DishDetailModal";
import { restaurantService } from "@/services/restaurantService";
import { MenuDish, MenuResponse } from "@/types";
import { useParams } from "next/navigation"
import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { userService } from "@/services/userService";
import { mealLogService } from "@/services/mealLogService";
import { UserProfile, MealLog } from "@/types";

export default function MenuPage() {
    const params = useParams();
    const slug = params.slug as string;
    const { user, isAuthenticated } = useAuth();

    const [isLoading, setIsLoading] = useState(true);
    const [menu, setMenu] = useState<MenuResponse | null>(null);
    const [activeCategory, setActiveCategory] = useState<number | null>(null);
    const [selectedDish, setSelectedDish] = useState<MenuDish | null>(null);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [meals, setMeals] = useState<MealLog[]>([]);

    useEffect(() => {
        const fetchMenu = async () => {
            try {
                const response = await restaurantService.getMenuBySlug(slug);
                setMenu(response.data);

                //default - set active category to the first one
                if (response.data.categories.length > 0) {
                    setActiveCategory(response.data.categories[0].id);
                }
            } catch (error) {
                console.error("Failed to load menu:", error);
            } finally {
                setIsLoading(false);
            }
        };

        fetchMenu();
    }, [slug]);

    const fetchUserData = async () => {
        if (isAuthenticated) {
            try {
                const [profileRes, mealsRes] = await Promise.all([
                    userService.getProfile(),
                    mealLogService.getMyLogs()
                ]);
                setProfile(profileRes.data);
                setMeals(mealsRes.data);
            } catch (error) {
                console.error("Failed to load user data:", error);
            }
        }
    };

    useEffect(() => {
        fetchUserData();
    }, [isAuthenticated]);

    if (isLoading) {
        return <div className="text-center text-text-secondary py-20">Loading menu...</div>;
    }

    if (!menu) {
        return <div className="text-center text-text-secondary py-20">Menu not found</div>;
    }

    //get dishes from active category
    const currentCategory = menu.categories.find((c) => c.id === activeCategory);

    // Calculate daily accumulator
    const accumulator = meals.reduce(
        (acc, meal) => ({
            calories: acc.calories + parseFloat(meal.calories as any),
            protein: acc.protein + parseFloat(meal.proteinG as any),
            carbs: acc.carbs + parseFloat(meal.carbsG as any),
            fat: acc.fat + parseFloat(meal.fatG as any),
        }),
        { calories: 0, protein: 0, carbs: 0, fat: 0 }
    );

    return (
        <div>
            {/* Hero Banner with Background Image */}
            <div 
                className="relative py-12 px-4 mb-6 bg-cover bg-center overflow-hidden"
                style={{ 
                    backgroundImage: `url(${menu.restaurant.logoUrl || "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80"})` 
                }}
            >
                {/* Dark Overlay for Readability */}
                <div className="absolute inset-0 bg-black/60" />

                <div className="max-w-6xl mx-auto relative z-10">
                    <h1 className="text-4xl font-extrabold text-white drop-shadow-md">{menu.restaurant.name}</h1>
                    <div className="flex flex-col gap-1 mt-3">
                        <p className="text-gray-200 font-medium flex items-center gap-2">
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-4 h-4 text-accent">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15 10.5a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1115 0z" />
                            </svg>
                            {menu.restaurant.address}
                        </p>
                        {menu.restaurant.description && (
                            <p className="text-gray-300 text-sm max-w-2xl leading-relaxed">
                                {menu.restaurant.description}
                            </p>
                        )}
                    </div>
                </div>
            </div>

            <div className="max-w-6xl mx-auto px-4">
                {/* Category Tabs */}
                <div className="flex gap-2 overflow-x-auto pb-4 mb-6 scrollbar-hide">
                    {menu.categories.map((category) => (
                        <button
                            key={category.id}
                            onClick={() => setActiveCategory(category.id)}
                            className={`whitespace-nowrap px-4 py-2 rounded-full text-sm font-medium transition-colors ${activeCategory === category.id
                                ? "bg-accent text-white"
                                : "bg-bg-card text-text-secondary hover:text-text-primary"
                                }`}
                        >
                            {category.name}
                        </button>
                    ))}
                </div>

                {/* Dishes */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {currentCategory?.dishes.map((dish) => (
                        <DishCard
                            key={dish.id}
                            dish={dish}
                            profile={profile}
                            accumulator={accumulator}
                            onClick={() => setSelectedDish(dish)}
                        />
                    ))}
                </div>

                {/* Empty State */}
                {currentCategory?.dishes.length === 0 && (
                    <p className="text-center text-text-secondary py-8">
                        No dishes found
                    </p>
                )}
            </div>

            {/* Dish Detail Modal */}
            {selectedDish && (
                <DishDetailModal
                    dish={selectedDish}
                    profile={profile}
                    accumulator={accumulator}
                    onClose={() => setSelectedDish(null)}
                    onMealLogged={fetchUserData}
                />
            )}
        </div>
    );
}