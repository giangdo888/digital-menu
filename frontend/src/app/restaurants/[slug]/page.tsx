"use client"

import DishCard from "@/components/features/DishCard";
import DishDetailModal from "@/components/features/DishDetailModal";
import { restaurantService } from "@/services/restaurantService";
import { MenuDish, MenuResponse } from "@/types";
import { useParams } from "next/navigation"
import { useEffect, useState } from "react";

export default function MenuPage() {
    const params = useParams();
    const slug = params.slug as string;

    const [isLoading, setIsLoading] = useState(true);
    const [menu, setMenu] = useState<MenuResponse | null>(null);
    const [activeCategory, setActiveCategory] = useState<number | null>(null);
    const [selectedDish, setSelectedDish] = useState<MenuDish | null>(null);

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

    if (isLoading) {
        return <div className="text-center text-text-secondary py-20">Loading menu...</div>;
    }

    if (!menu) {
        return <div className="text-center text-text-secondary py-20">Menu not found</div>;
    }

    //get dishes from active category
    const currentCategory = menu.categories.find((c) => c.id === activeCategory);

    return (
        <div>
            {/* Hero Banner */}
            <div className="bg-bg-card py-8 px-4 mb-6">
                <div className="max-w-6xl mx-auto">
                    <h1 className="text-3xl font-bold">{menu.restaurant.name}</h1>
                    <p className="text-text-secondary mt-2">{menu.restaurant.address}</p>
                    {menu.restaurant.description && (
                        <p className="text-text-secondary mt-1 text-sm">{menu.restaurant.description}</p>
                    )}
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
                    onClose={() => setSelectedDish(null)}
                />
            )}
        </div>
    );
}