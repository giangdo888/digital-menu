"use client"

import { restaurantService } from "@/services/restaurantService"
import { RestaurantPublic } from "@/types"
import Link from "next/link";
import { useEffect, useState } from "react"

export default function HomePage() {
  const [restaurants, setRestaurants] = useState<RestaurantPublic[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchRestaurants = async () => {
      try {
        const response = await restaurantService.getPublicList();
        setRestaurants(response.data);
      } catch (error) {
        console.error("Failed to load restaurants:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRestaurants();
  }, []);

  //filter using searchTerm
  const filter = restaurants.filter((r) =>
    r.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="max-w-6xl mx-auto px-4 pb-12 pt-0 md:pt-8">
      {/* Hero Section */}
      <section className="py-6 md:py-20 text-center max-w-3xl mx-auto px-4 animate-fade-in-up">
        {/* Mobile-only Logo */}
        <div className="flex justify-center mb-6 md:hidden">
          <img 
            src="/logo.png" 
            alt="Digital Menu Logo" 
            className="h-20 w-auto object-contain"
          />
        </div>
        <h1 className="text-3xl md:text-5xl font-serif font-bold text-text-primary mb-3 md:mb-4 leading-tight">
          Welcome to Digital Menu
        </h1>
        <p className="text-lg md:text-2xl font-serif italic text-accent mb-4 md:mb-6">
          "Precision nutrition for the modern lifestyle"
        </p>
        <p className="text-text-secondary text-base md:text-lg leading-relaxed mb-6 md:mb-8">
          To start, find the restaurant you had your meals today and start logging! 
          Elevate your dining experience with mindful nutrition.
        </p>
        <div className="w-16 md:w-24 h-px bg-border mx-auto mb-6 md:mb-10" />
      </section>

      {/*Search bar*/}
      <div className="mb-8">
        <input
          type="text"
          placeholder="Search restaurants..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full md:max-w-xl md:mx-auto md:block bg-bg-card border border-border rounded-sm px-5 py-3 text-text-primary placeholder-text-secondary focus:outline-none focus:border-accent text-lg"
        />
      </div>
      {/* Section Title */}
      <h2 className="text-2xl font-bold mb-6 tracking-wide">Popular Restaurants</h2>

      {/* Loading State */}
      {isLoading && (
        <div className="text-center text-text-secondary py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-accent mx-auto mb-4"></div>
          <p>Loading restaurants...</p>
        </div>
      )}

      {/* Restaurant Grid */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 md:gap-6 stagger-fade-in">
        {filter.map((restaurant) => (
          <Link
            key={restaurant.slug}
            href={(`/restaurants/${restaurant.slug}`)}
            className="bg-bg-card border border-border rounded-sm overflow-hidden hover:border-accent transition-all duration-300 ease-out group shadow-luxury hover:shadow-luxury-hover hover:-translate-y-1 hover:scale-[1.01]"
          >
            {/* Restaurant Image */}
            <div className="aspect-video bg-bg-elevated relative overflow-hidden">
                <img
                  src={restaurant.logoUrl || "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=800&q=80"}
                  alt={restaurant.name}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                />
            </div>

            {/* Restaurant Info */}
            <div className="p-3 md:p-4">
              <h3 className="font-semibold text-text-primary text-sm md:text-base truncate">
                {restaurant.name}
              </h3>
              <p className="text-text-secondary text-xs md:text-sm mt-1 truncate">
                {restaurant.address}
              </p>
            </div>
          </Link>
        ))}

      </div>

      {!isLoading && filter.length === 0 && (
        <div className="text-center text-text-secondary py-12">
          No restaurants found.
        </div>
      )}
    </div>
  );
}
