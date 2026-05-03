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

    type SortFilterType = "default" | "high_protein" | "low_cal" | "low_carb" | "low_fat";
    const [sortFilter, setSortFilter] = useState<SortFilterType>("default");

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

    // Filter / Sort dishes
    let displayDishes = currentCategory?.dishes ? [...currentCategory.dishes] : [];
    if (sortFilter === "high_protein") {
        displayDishes.sort((a, b) => b.proteinG - a.proteinG);
    } else if (sortFilter === "low_cal") {
        displayDishes.sort((a, b) => a.calories - b.calories);
    } else if (sortFilter === "low_carb") {
        displayDishes.sort((a, b) => a.carbsG - b.carbsG);
    } else if (sortFilter === "low_fat") {
        displayDishes.sort((a, b) => a.fatG - b.fatG);
    }

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

    const bgImage = menu.restaurant.logoUrl || "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80";

    return (
        <div className="relative min-h-screen">
            {/* Fixed Full Page Background */}
            <div className="fixed inset-0 -z-10 w-full h-full overflow-hidden">
                <div
                    className="absolute inset-0 bg-cover bg-center scale-105"
                    style={{ backgroundImage: `url(${bgImage})` }}
                />
                {/* Luxury Overlay: Dark Gradient + Subtle Blur */}
                <div className="absolute inset-0 bg-bg-primary/65 backdrop-blur-[4px]" />
                <div className="absolute inset-0 bg-gradient-to-t from-bg-primary via-transparent to-bg-primary/40" />
            </div>

            {/* Content Section */}
            <div className="relative z-10 py-12 px-4">
                <div className="max-w-6xl mx-auto">
                    {/* Floating Header Info */}
                    <div className="mb-10 text-center md:text-left">
                        <h1 className="text-5xl font-extrabold text-text-primary mb-4 tracking-tight">
                            {menu.restaurant.name}
                        </h1>
                        <div className="flex flex-col gap-2 items-center md:items-start">
                            <p className="text-text-secondary font-medium flex items-center gap-2 text-lg">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 text-accent">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M15 10.5a3 3 0 11-6 0 3 3 0 016 0z" />
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1115 0z" />
                                </svg>
                                {menu.restaurant.address}
                            </p>
                            {menu.restaurant.description && (
                                <p className="text-text-secondary text-base max-w-2xl leading-relaxed italic border-l-2 border-accent/40 pl-4 py-1">
                                    {menu.restaurant.description}
                                </p>
                            )}
                        </div>
                    </div>

                    {/* Navigation and Filters Container - Glassmorphism style */}
                    <div className="sticky top-14 md:top-20 z-20 mb-4 md:mb-8 p-4 bg-white/40 backdrop-blur-md border border-border rounded-sm shadow-xl animate-fade-in-up">
                        {/* Category Tabs */}
                        <div className="flex gap-3 overflow-x-auto pb-3 mb-4 scrollbar-hide">
                            {menu.categories.map((category) => (
                                <button
                                    key={category.id}
                                    onClick={() => setActiveCategory(category.id)}
                                    className={`whitespace-nowrap px-6 py-2.5 rounded-sm text-sm font-semibold tracking-wide transition-all duration-300 ${activeCategory === category.id
                                        ? "bg-accent text-white shadow-lg shadow-accent/20"
                                        : "bg-white/50 text-text-secondary hover:text-text-primary hover:bg-white/80 border border-border/50"
                                        }`}
                                >
                                    {category.name.toUpperCase()}
                                </button>
                            ))}
                        </div>

                        {/* Nutritional Filters */}
                        <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide items-center">
                            <span className="text-xs uppercase tracking-widest text-text-secondary whitespace-nowrap mr-2">Sort by</span>
                            {[
                                { id: "default", label: "Default" },
                                { id: "high_protein", label: "High Protein" },
                                { id: "low_cal", label: "Low Calorie" },
                                { id: "low_carb", label: "Low Carb" },
                                { id: "low_fat", label: "Low Fat" }
                            ].map((filter) => (
                                <button
                                    key={filter.id}
                                    onClick={() => setSortFilter(filter.id as SortFilterType)}
                                    className={`whitespace-nowrap px-4 py-1.5 rounded-sm text-[10px] uppercase font-bold tracking-widest transition-all duration-200 border ${sortFilter === filter.id
                                        ? "bg-accent/10 border-accent text-accent"
                                        : "bg-transparent border-border/50 text-text-secondary hover:text-text-primary hover:border-accent/40"
                                        }`}
                                >
                                    {filter.label}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Dishes Grid - Floating Effect */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pb-40 md:pb-24 stagger-fade-in" key={activeCategory}>
                        {displayDishes.map((dish) => (
                            <div key={dish.id} className="transform transition-transform hover:-translate-y-1 duration-300">
                                <DishCard
                                    dish={dish}
                                    profile={profile}
                                    accumulator={accumulator}
                                    onClick={() => setSelectedDish(dish)}
                                />
                            </div>
                        ))}
                    </div>

                    {/* Empty State */}
                    {currentCategory?.dishes.length === 0 && (
                        <div className="bg-white/20 backdrop-blur-md rounded-sm border border-white/20 p-12 text-center">
                            <p className="text-text-secondary text-lg">
                                No dishes found in this category.
                            </p>
                        </div>
                    )}
                </div>
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