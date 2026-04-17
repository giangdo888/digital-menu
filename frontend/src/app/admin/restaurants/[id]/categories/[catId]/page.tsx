"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { Restaurant, Category, Dish, AFCDItem } from "@/types";
import { restaurantService } from "@/services/restaurantService";
import { categoryService } from "@/services/categoryService";
import { dishService } from "@/services/dishService";
import ProtectedRoute from "@/components/layout/ProtectedRoute";
import Breadcrumbs from "@/components/layout/Breadcrumbs";
import toast from "react-hot-toast";

interface DishIngredient {
    afcdItemId: number;
    name: string;
    amountInGrams: number;
}

export default function CategoryDetailPage() {
    const params = useParams();
    const restaurantId = parseInt(params.id as string);
    const categoryId = parseInt(params.catId as string);

    const [restaurant, setRestaurant] = useState<Restaurant | null>(null);
    const [category, setCategory] = useState<Category | null>(null);
    const [dishes, setDishes] = useState<Dish[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // Add dish form
    const [showDishForm, setShowDishForm] = useState(false);
    const [dishForm, setDishForm] = useState({ name: "", price: 0, displayOrder: 0 });

    // Dish detail modal
    const [editingDish, setEditingDish] = useState<Dish | null>(null);
    const [ingredients, setIngredients] = useState<DishIngredient[]>([]);
    const [dishImageUrl, setDishImageUrl] = useState("");
    const [searchQuery, setSearchQuery] = useState("");
    const [searchResults, setSearchResults] = useState<AFCDItem[]>([]);
    const [isSearching, setIsSearching] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [restRes, catRes, dishRes] = await Promise.all([
                    restaurantService.getMyRestaurants(),
                    categoryService.getByRestaurant(restaurantId),
                    dishService.getByCategory(categoryId),
                ]);
                const foundRest = restRes.data.find((r: Restaurant) => r.id === restaurantId);
                const foundCat = catRes.data.find((c: Category) => c.id === categoryId);
                setRestaurant(foundRest || null);
                setCategory(foundCat || null);
                setDishes(dishRes.data);
            } catch {
                toast.error("Failed to load data");
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, [restaurantId, categoryId]);

    // ── Create Dish ──
    const handleCreateDish = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const res = await dishService.create({
                categoryId,
                name: dishForm.name,
                price: dishForm.price,
                displayOrder: dishForm.displayOrder,
            });
            setDishes((prev) => [...prev, res.data]);
            setShowDishForm(false);
            setDishForm({ name: "", price: 0, displayOrder: 0 });
            toast.success("Dish created!");
        } catch {
            toast.error("Failed to create dish");
        }
    };

    const handleDelete = async (id: number) => {
        if (!window.confirm("Are you sure you want to delete this dish?")) return;
        try {
            await dishService.delete(id);
            setDishes((prev) => prev.filter((d) => d.id !== id));
            toast.success("Dish deleted!");
        } catch {
            toast.error("Failed to delete dish");
        }
    };

    // ── Open Dish Modal ──
    const openDishModal = async (dish: Dish) => {
        setEditingDish(dish);
        setDishImageUrl(dish.imageUrl || "");
        try {
            const res = await dishService.getIngredients(dish.id);
            // API returns ingredientName, not name
            const data = res.data as { afcdItemId: number; ingredientName: string; amountInGrams: number }[];
            setIngredients(data.map((i) => ({ afcdItemId: i.afcdItemId, name: i.ingredientName, amountInGrams: i.amountInGrams })));
        } catch {
            setIngredients([]);
        }
    };

    // ── AFCD Search ──
    const handleSearch = async () => {
        if (!searchQuery.trim()) return;
        setIsSearching(true);
        try {
            const res = await dishService.searchAFCD(searchQuery);
            setSearchResults(res.data);
        } catch {
            toast.error("Search failed");
        } finally {
            setIsSearching(false);
        }
    };

    const addIngredient = (item: AFCDItem) => {
        if (ingredients.some((i) => i.afcdItemId === item.id)) {
            toast.error("Already added");
            return;
        }
        setIngredients((prev) => [...prev, { afcdItemId: item.id, name: item.name, amountInGrams: 100 }]);
        setSearchResults([]);
        setSearchQuery("");
    };

    const removeIngredient = (afcdItemId: number) => {
        setIngredients((prev) => prev.filter((i) => i.afcdItemId !== afcdItemId));
    };

    const updateAmount = (afcdItemId: number, amount: number) => {
        setIngredients((prev) => prev.map((i) => i.afcdItemId === afcdItemId ? { ...i, amountInGrams: amount } : i));
    };

    const saveDish = async () => {
        if (!editingDish) return;
        try {
            // Save image URL if changed
            if (dishImageUrl !== (editingDish.imageUrl || "")) {
                await dishService.update(editingDish.id, { imageUrl: dishImageUrl || undefined });
            }
            // Save ingredients
            await dishService.updateIngredients(editingDish.id, ingredients.map((i) => ({
                afcdItemId: i.afcdItemId,
                amountInGrams: i.amountInGrams,
            })));
            toast.success("Dish saved!");
            // Refresh dishes
            const res = await dishService.getByCategory(categoryId);
            setDishes(res.data);
            setEditingDish(null);
        } catch {
            toast.error("Failed to save");
        }
    };

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;
    if (!restaurant || !category) return <div className="text-center py-20 text-text-secondary">Not found</div>;

    return (
        <ProtectedRoute allowedRoles={["restaurant_admin", "system_admin"]}>
            <div className="max-w-4xl mx-auto px-4 py-8">
                <Breadcrumbs items={[
                    { label: "Restaurants", href: "/admin/restaurants" },
                    { label: restaurant.name, href: `/admin/restaurants/${restaurantId}` },
                    { label: category.name },
                ]} />

                <div className="flex justify-between items-center mb-6">
                    <div>
                        <h1 className="text-2xl font-bold">{category.name}</h1>
                        <p className="text-text-secondary text-sm">{category.type} • {dishes.length} dishes</p>
                    </div>
                    <button onClick={() => setShowDishForm(!showDishForm)}
                        className="bg-accent hover:bg-accent-hover text-white px-3 py-1.5 rounded-lg text-sm">
                        + Add Dish
                    </button>
                </div>

                {/* Add Dish Form */}
                {showDishForm && (
                    <form onSubmit={handleCreateDish} className="bg-bg-card rounded-xl p-4 mb-4 flex gap-3 items-end">
                        <div className="flex-1">
                            <label className="text-xs text-text-secondary">Name</label>
                            <input value={dishForm.name} onChange={(e) => setDishForm({ ...dishForm, name: e.target.value })}
                                required placeholder="e.g., Grilled Chicken"
                                className="w-full bg-bg-elevated rounded-lg px-3 py-2 text-sm text-text-primary focus:outline-none" />
                        </div>
                        <div className="w-24">
                            <label className="text-xs text-text-secondary">Price ($)</label>
                            <input type="number" step="0.01" value={dishForm.price} onChange={(e) => setDishForm({ ...dishForm, price: +e.target.value })}
                                required
                                className="w-full bg-bg-elevated rounded-lg px-3 py-2 text-sm text-text-primary focus:outline-none" />
                        </div>
                        <button type="submit" className="bg-accent text-white px-4 py-2 rounded-lg text-sm">Add</button>
                    </form>
                )}

                {/* Dishes List */}
                <div className="space-y-3">
                    {dishes.map((dish) => (
                        <button key={dish.id} onClick={() => openDishModal(dish)}
                            className="w-full text-left bg-bg-card rounded-xl p-4 hover:ring-1 hover:ring-accent transition-all">
                            <div className="flex gap-4 items-center">
                                {/* Dish Image */}
                                {dish.imageUrl ? (
                                    <img src={dish.imageUrl} alt={dish.name}
                                        className="w-16 h-16 rounded-lg object-cover flex-shrink-0" />
                                ) : (
                                    <div className="w-16 h-16 rounded-lg bg-bg-elevated flex items-center justify-center text-2xl flex-shrink-0">
                                        🥘
                                    </div>
                                )}
                                <div className="flex-1 min-w-0">
                                    <div className="flex justify-between items-start">
                                        <div>
                                            <h3 className="font-medium">{dish.name}</h3>
                                            <p className="text-sm text-accent font-semibold">${dish.price.toFixed(2)}</p>
                                        </div>
                                        <span className="text-xs text-text-secondary">
                                            {dish.ingredientCount > 0 ? `${dish.ingredientCount} ingredients` : "No ingredients"}
                                        </span>
                                    </div>
                                    <div className="flex gap-3 mt-2 text-xs text-text-secondary">
                                        <span>{Math.round(dish.calories)} cal</span>
                                        <span>P {Math.round(dish.proteinG)}g</span>
                                        <span>C {Math.round(dish.carbsG)}g</span>
                                        <span>F {Math.round(dish.fatG)}g</span>
                                    </div>
                                </div>
                                <button
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleDelete(dish.id);
                                    }}
                                    className="text-danger justify-end hover:text-red-500 transition-colors p-1"
                                    title="Remove dish"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="w-5 h-5">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                                    </svg>
                                </button>
                            </div>
                        </button>
                    ))}
                    {dishes.length === 0 && (
                        <p className="text-text-secondary text-center py-8">No dishes yet. Add your first dish above!</p>
                    )}
                </div>
            </div>

            {/* ── Dish Detail Modal ── */}
            {editingDish && (
                <>
                    <div className="fixed inset-0 bg-black/60 z-50" onClick={() => setEditingDish(null)} />
                    <div className="fixed z-50 inset-x-0 bottom-0 md:inset-0 md:flex md:items-center md:justify-center">
                        <div className="bg-bg-card rounded-t-2xl md:rounded-2xl max-h-[85vh] overflow-y-auto w-full md:max-w-xl p-5 shadow-2xl transition-all duration-300">
                            <div className="flex justify-between items-center mb-4">
                                <h2 className="text-lg font-bold">{editingDish.name}</h2>
                                <button onClick={() => setEditingDish(null)} className="text-text-secondary hover:text-text-primary text-xl">✕</button>
                            </div>

                            {/* Dish Image URL */}
                            <div className="mb-4">
                                <label className="text-xs text-text-secondary block mb-1">Dish Image URL</label>
                                <input value={dishImageUrl} onChange={(e) => setDishImageUrl(e.target.value)}
                                    placeholder="https://example.com/dish.jpg"
                                    className="w-full bg-bg-elevated rounded-lg px-3 py-2 text-sm text-text-primary focus:outline-none" />
                                {dishImageUrl && (
                                    <img src={dishImageUrl} alt="Preview" className="mt-2 w-full h-32 object-cover rounded-lg" />
                                )}
                            </div>

                            {/* Search AFCD */}
                            <label className="text-xs text-text-secondary block mb-1">Ingredients</label>
                            <div className="flex gap-2 mb-4">
                                <input
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), handleSearch())}
                                    placeholder="Search ingredients (e.g. chicken breast)"
                                    className="flex-1 bg-bg-elevated rounded-lg px-3 py-2 text-sm text-text-primary focus:outline-none"
                                />
                                <button onClick={handleSearch} disabled={isSearching}
                                    className="bg-accent text-white px-3 py-2 rounded-lg text-sm">
                                    {isSearching ? "..." : "Search"}
                                </button>
                            </div>

                            {/* Search Results */}
                            {searchResults.length > 0 && (
                                <div className="bg-bg-elevated rounded-lg mb-4 max-h-[32rem] overflow-y-auto border border-accent/20">
                                    {searchResults.map((item) => (
                                        <button key={item.id} onClick={() => addIngredient(item)}
                                            className="w-full text-left px-3 py-2 hover:bg-bg-card text-sm border-b border-bg-card last:border-0">
                                            <span className="font-medium">{item.name}</span>
                                            {item.variant && <span className="text-text-secondary ml-1">({item.variant})</span>}
                                            <span className="text-text-secondary ml-2">• {item.calories} cal/100g</span>
                                        </button>
                                    ))}
                                </div>
                            )}

                            {/* Current Ingredients */}
                            <div className="space-y-2 mb-4">
                                {ingredients.length === 0 && (
                                    <p className="text-text-secondary text-sm text-center py-4">No ingredients yet. Search and add above.</p>
                                )}
                                {ingredients.map((ing) => (
                                    <div key={ing.afcdItemId} className="flex items-center gap-2 bg-bg-elevated rounded-lg px-3 py-2">
                                        <span className="flex-1 text-sm">{ing.name}</span>
                                        <input
                                            type="number"
                                            value={ing.amountInGrams}
                                            onChange={(e) => updateAmount(ing.afcdItemId, +e.target.value)}
                                            className="w-20 bg-bg-card rounded px-2 py-1 text-sm text-center text-text-primary focus:outline-none"
                                        />
                                        <span className="text-xs text-text-secondary">g</span>
                                        <button onClick={() => removeIngredient(ing.afcdItemId)} className="text-danger text-sm hover:text-red-400">✕</button>
                                    </div>
                                ))}
                            </div>

                            {/* Save */}
                            <button onClick={saveDish}
                                className="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-xl">
                                Save
                            </button>
                        </div>
                    </div>
                </>
            )}
        </ProtectedRoute>
    );
}
