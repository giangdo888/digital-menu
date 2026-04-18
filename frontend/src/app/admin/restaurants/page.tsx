"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Restaurant, CreateRestaurantRequest } from "@/types";
import { restaurantService } from "@/services/restaurantService";
import ProtectedRoute from "@/components/layout/ProtectedRoute";
import toast from "react-hot-toast";

export default function MyRestaurantsPage() {
    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [showForm, setShowForm] = useState(false);
    const [form, setForm] = useState<CreateRestaurantRequest>({
        name: "", address: "", phone: "", email: "", description: "", logoUrl: "", openingHours: "",
    });

    useEffect(() => {
        restaurantService.getMyRestaurants().then((res) => setRestaurants(res.data));
    }, []);

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const res = await restaurantService.create(form);
            setRestaurants((prev) => [...prev, res.data]);
            setShowForm(false);
            setForm({ name: "", address: "", logoUrl: "", openingHours: "" });
            toast.success("Restaurant created! 🎉");
        } catch {
            toast.error("Failed to create restaurant");
        }
    };

    return (
        <ProtectedRoute allowedRoles={["restaurant_admin"]}>
            <div className="max-w-4xl mx-auto px-4 py-8">
                <div className="flex justify-between items-center mb-6">
                    <h1 className="text-2xl font-bold">My Restaurants</h1>
                    <button onClick={() => setShowForm(!showForm)}
                        className="bg-accent hover:bg-accent-hover text-white px-4 py-2 rounded-sm text-sm">
                        + New Restaurant
                    </button>
                </div>

                {/* Create Form (toggleable) */}
                {showForm && (
                    <form onSubmit={handleCreate} className="bg-bg-card border border-border rounded-sm p-5 mb-6 space-y-3">
                        <input placeholder="Restaurant Name *" value={form.name} required
                            onChange={(e) => setForm({ ...form, name: e.target.value })}
                            className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                        <input placeholder="Address *" value={form.address} required
                            onChange={(e) => setForm({ ...form, address: e.target.value })}
                            className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                        <div className="grid grid-cols-2 gap-3">
                            <input placeholder="Phone" value={form.phone || ""}
                                onChange={(e) => setForm({ ...form, phone: e.target.value })}
                                className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                            <input placeholder="Email" value={form.email || ""}
                                onChange={(e) => setForm({ ...form, email: e.target.value })}
                                className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                        </div>
                        <input placeholder="Logo URL" value={form.logoUrl || ""}
                            onChange={(e) => setForm({ ...form, logoUrl: e.target.value })}
                            className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                        <input placeholder="Opening Hours (e.g., Mon-Fri 9am-9pm)" value={form.openingHours || ""}
                            onChange={(e) => setForm({ ...form, openingHours: e.target.value })}
                            className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent" />
                        <textarea placeholder="Description" value={form.description || ""}
                            onChange={(e) => setForm({ ...form, description: e.target.value })}
                            className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent h-20" />
                        <button type="submit" className="bg-accent hover:bg-accent-hover text-white px-6 py-2 rounded-sm">
                            Create
                        </button>
                    </form>
                )}

                {/* Restaurant List */}
                <div className="space-y-3">
                    {restaurants.map((r) => (
                        <Link key={r.id} href={`/admin/restaurants/${r.id}`}
                            className="bg-bg-card border border-border rounded-sm p-5 flex items-center gap-4 hover:border-accent transition-all block">
                            <img 
                                src={r.logoUrl || "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=800&q=80"} 
                                alt={r.name} 
                                className="w-12 h-12 rounded-sm object-cover flex-shrink-0" 
                            />
                            <div className="flex-1 min-w-0">
                                <h3 className="font-semibold">{r.name}</h3>
                                <p className="text-sm text-text-secondary">{r.address}</p>
                                <p className="text-sm text-text-secondary">{r.categoryCount} categories</p>
                            </div>
                            <span className={`text-xs px-2 py-1 rounded ${r.isActive ? "bg-success/20 text-success" : "bg-danger/20 text-danger"}`}>
                                {r.isActive ? "Active" : "Inactive"}
                            </span>
                        </Link>
                    ))}
                </div>
            </div>
        </ProtectedRoute>
    );
}

