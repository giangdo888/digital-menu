"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { Restaurant, Category, CreateRestaurantRequest } from "@/types";
import { restaurantService } from "@/services/restaurantService";
import { categoryService } from "@/services/categoryService";
import ProtectedRoute from "@/components/layout/ProtectedRoute";
import Breadcrumbs from "@/components/layout/Breadcrumbs";
import Link from "next/link";
import toast from "react-hot-toast";

export default function RestaurantDetailPage() {
    const params = useParams();
    const id = parseInt(params.id as string);

    const [restaurant, setRestaurant] = useState<Restaurant | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [showCatForm, setShowCatForm] = useState(false);
    const [catForm, setCatForm] = useState({ name: "", type: "food", displayOrder: 0 });

    // Edit restaurant
    const [isEditingInfo, setIsEditingInfo] = useState(false);
    const [editForm, setEditForm] = useState<CreateRestaurantRequest>({ name: "", address: "" });

    useEffect(() => {
        Promise.all([
            restaurantService.getMyRestaurants(),
            categoryService.getByRestaurant(id),
        ]).then(([restRes, catRes]) => {
            const found = restRes.data.find((r: Restaurant) => r.id === id);
            setRestaurant(found || null);
            setCategories(catRes.data);
        });
    }, [id]);

    const handleCreateCategory = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const res = await categoryService.create({
                restaurantId: id,
                name: catForm.name,
                type: catForm.type,
                displayOrder: catForm.displayOrder,
            });
            setCategories((prev) => [...prev, res.data]);
            setShowCatForm(false);
            setCatForm({ name: "", type: "food", displayOrder: 0 });
            toast.success("Category created!");
        } catch {
            toast.error("Failed to create category");
        }
    };

    const handleDeleteCategory = async (catId: number, catName: string) => {
        if (!window.confirm(`Delete "${catName}"? This will also delete all dishes in this category.`)) return;
        try {
            await categoryService.delete(catId);
            setCategories((prev) => prev.filter((c) => c.id !== catId));
            toast.success("Category deleted!");
        } catch {
            toast.error("Failed to delete category");
        }
    };

    const openEditForm = () => {
        if (!restaurant) return;
        setEditForm({
            name: restaurant.name,
            address: restaurant.address,
            phone: restaurant.phone || "",
            email: restaurant.email || "",
            description: restaurant.description || "",
            logoUrl: restaurant.logoUrl || "",
            openingHours: restaurant.openingHours || "",
        });
        setIsEditingInfo(true);
    };

    const handleUpdateRestaurant = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const res = await restaurantService.update(id, editForm);
            setRestaurant(res.data);
            setIsEditingInfo(false);
            toast.success("Restaurant updated!");
        } catch {
            toast.error("Failed to update");
        }
    };

    if (!restaurant) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    return (
        <ProtectedRoute allowedRoles={["restaurant_admin", "system_admin"]}>
            <div className="max-w-4xl mx-auto px-4 py-8">
                <Breadcrumbs items={[
                    { label: "Restaurants", href: "/admin/restaurants" },
                    { label: restaurant.name },
                ]} />

                {/* Restaurant Info */}
                <div className="bg-bg-card border border-border rounded-sm p-5 mb-6">
                    <div className="flex justify-between items-start mb-4">
                        <div className="flex gap-4 items-center">
                            <img
                                src={restaurant.logoUrl || "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=800&q=80"}
                                alt={restaurant.name}
                                className="w-16 h-16 rounded-sm object-cover border border-border"
                            />
                            <div>
                                <h1 className="text-2xl font-bold">{restaurant.name}</h1>
                                <p className="text-text-secondary">{restaurant.address}</p>
                            </div>
                        </div>
                        <button onClick={openEditForm}
                            className="text-sm text-accent hover:text-accent-hover">
                            Edit
                        </button>
                    </div>
                    <div className="grid grid-cols-2 gap-3 text-sm">
                        {restaurant.phone && (
                            <div>
                                <span className="text-text-secondary">Phone: </span>
                                <span>{restaurant.phone}</span>
                            </div>
                        )}
                        {restaurant.email && (
                            <div>
                                <span className="text-text-secondary">Email: </span>
                                <span>{restaurant.email}</span>
                            </div>
                        )}
                        {restaurant.openingHours && (
                            <div>
                                <span className="text-text-secondary">Hours: </span>
                                <span>{restaurant.openingHours}</span>
                            </div>
                        )}
                        {restaurant.description && (
                            <div className="col-span-2">
                                <span className="text-text-secondary">Description: </span>
                                <span>{restaurant.description}</span>
                            </div>
                        )}
                    </div>
                </div>

                {/* Edit Info Modal */}
                {isEditingInfo && (
                    <>
                        <div className="fixed inset-0 bg-stone-900/40 z-50" onClick={() => setIsEditingInfo(false)} />
                        <div className="fixed z-50 inset-x-0 bottom-0 md:inset-0 md:flex md:items-center md:justify-center">
                            <div className="bg-bg-card border border-border rounded-t-sm md:rounded-sm max-h-[85vh] overflow-y-auto w-full md:max-w-lg p-5">
                                <div className="flex justify-between items-center mb-4">
                                    <h2 className="text-lg font-bold">Edit Restaurant</h2>
                                    <button onClick={() => setIsEditingInfo(false)} className="text-text-secondary hover:text-text-primary text-xl">✕</button>
                                </div>
                                <form onSubmit={handleUpdateRestaurant} className="space-y-3">
                                    <div>
                                        <label className="text-xs text-text-secondary">Name *</label>
                                        <input value={editForm.name} required onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                                            className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                    </div>
                                    <div>
                                        <label className="text-xs text-text-secondary">Address *</label>
                                        <input value={editForm.address} required onChange={(e) => setEditForm({ ...editForm, address: e.target.value })}
                                            className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                    </div>
                                    <div className="grid grid-cols-2 gap-3">
                                        <div>
                                            <label className="text-xs text-text-secondary">Phone</label>
                                            <input value={editForm.phone || ""} onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                                                className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                        </div>
                                        <div>
                                            <label className="text-xs text-text-secondary">Email</label>
                                            <input value={editForm.email || ""} onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                                                className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                        </div>
                                    </div>
                                    <div>
                                        <label className="text-xs text-text-secondary">Logo URL</label>
                                        <input value={editForm.logoUrl || ""} onChange={(e) => setEditForm({ ...editForm, logoUrl: e.target.value })}
                                            className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                        {editForm.logoUrl && (
                                            <img src={editForm.logoUrl} alt="Preview" className="mt-2 w-16 h-16 rounded-sm object-cover border border-border" />
                                        )}
                                    </div>
                                    <div>
                                        <label className="text-xs text-text-secondary">Opening Hours</label>
                                        <input value={editForm.openingHours || ""} onChange={(e) => setEditForm({ ...editForm, openingHours: e.target.value })}
                                            placeholder="e.g., Mon-Fri 9am-9pm"
                                            className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                                    </div>
                                    <div>
                                        <label className="text-xs text-text-secondary">Description</label>
                                        <textarea value={editForm.description || ""} onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                                            className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent h-20" />
                                    </div>
                                    <button type="submit" className="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-sm">
                                        Save Changes
                                    </button>
                                </form>
                            </div>
                        </div>
                    </>
                )}

                {/* Categories Section */}
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-semibold">Categories</h2>
                    <button onClick={() => setShowCatForm(!showCatForm)}
                        className="bg-accent hover:bg-accent-hover text-white px-3 py-1.5 rounded-sm text-sm">
                        + Add Category
                    </button>
                </div>

                {showCatForm && (
                    <form onSubmit={handleCreateCategory} className="bg-bg-card border border-border rounded-sm p-4 mb-4 flex gap-3 items-end">
                        <div className="flex-1">
                            <label className="text-xs text-text-secondary">Name</label>
                            <input value={catForm.name} onChange={(e) => setCatForm({ ...catForm, name: e.target.value })}
                                required placeholder="e.g., Mains"
                                className="w-full bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent" />
                        </div>
                        <div>
                            <label className="text-xs text-text-secondary">Type</label>
                            <select value={catForm.type} onChange={(e) => setCatForm({ ...catForm, type: e.target.value })}
                                className="bg-bg-primary border border-border rounded-sm px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-accent">
                                <option value="food">Food</option>
                                <option value="drink">Drink</option>
                            </select>
                        </div>
                        <button type="submit" className="bg-accent hover:bg-accent-hover text-white px-4 py-2 rounded-sm text-sm border border-transparent hover:border-accent transition-colors">Add</button>
                    </form>
                )}

                <div className="space-y-3">
                    {categories.map((cat) => (
                        <div key={cat.id} className="bg-bg-card border border-border rounded-sm p-4 flex justify-between items-center hover:border-accent transition-all">
                            <Link href={`/admin/restaurants/${id}/categories/${cat.id}`} className="flex-1 min-w-0">
                                <h3 className="font-medium">{cat.name}</h3>
                                <p className="text-sm text-text-secondary">{cat.dishCount} dishes • {cat.type}</p>
                            </Link>
                            <div className="flex items-center gap-3 ml-3">
                                <button onClick={() => handleDeleteCategory(cat.id, cat.name)} className="text-danger hover:text-red-500 transition-colors p-1" title="Remove category">
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="w-5 h-5">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </ProtectedRoute>
    );
}
