// ═══════════════════════════════════════
// AUTH
// ═══════════════════════════════════════
export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    role: "customer" | "restaurant_admin";
}

export interface RevokeToken {
    token: string;
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    tokenExpiresAt: string;
    refreshTokenExpiresAt: string;
}

// ═══════════════════════════════════════
// RESTAURANT (Public)
// ═══════════════════════════════════════
export interface RestaurantPublic {
    name: string;
    slug: string;
    address: string;
    phone?: string;
    email?: string;
    description?: string;
    openingHours?: string;
    logoUrl?: string;
}

// ═══════════════════════════════════════
// RESTAURANT (Admin)
// ═══════════════════════════════════════
export interface Restaurant {
    id: number;
    ownerId: number;
    ownerName: string;
    name: string;
    slug: string;
    address: string;
    phone?: string;
    email?: string;
    description?: string;
    openingHours?: string;
    logoUrl?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
    categoryCount: number;
}
export interface CreateRestaurantRequest {
    name: string;
    address: string;
    phone?: string;
    email?: string;
    description?: string;
    openingHours?: string;
    logoUrl?: string;
}
export interface UpdateRestaurantRequest {
    name?: string;
    address?: string;
    phone?: string;
    email?: string;
    description?: string;
    openingHours?: string;
    logoUrl?: string;
}
// ═══════════════════════════════════════
// MENU (Public — the nested response)
// ═══════════════════════════════════════
export interface MenuResponse {
    restaurant: RestaurantPublic;
    categories: MenuCategory[];
}
export interface MenuCategory {
    id: number;
    name: string;
    type: string;
    displayOrder: number;
    dishes: MenuDish[];
}
export interface MenuDish {
    id: number;
    name: string;
    price: number;
    imageUrl?: string;
    displayOrder: number;
    calories: number;
    proteinG: number;
    carbsG: number;
    fatG: number;
    ingredients: MenuIngredient[];
}
export interface MenuIngredient {
    name: string;
    variant?: string;
    amountInGrams: number;
}
// ═══════════════════════════════════════
// CATEGORY (Admin CRUD)
// ═══════════════════════════════════════
export interface Category {
    id: number;
    restaurantId: number;
    name: string;
    type: string;
    displayOrder: number;
    dishCount: number;
    createdAt: string;
}
export interface CreateCategoryRequest {
    restaurantId: number;
    name: string;
    type: string;
    displayOrder: number;
}
// ═══════════════════════════════════════
// DISH (Admin CRUD)
// ═══════════════════════════════════════
export interface Dish {
    id: number;
    categoryId: number;
    categoryName: string;
    name: string;
    price: number;
    imageUrl?: string;
    displayOrder: number;
    isActive: boolean;
    calories: number;
    proteinG: number;
    carbsG: number;
    fatG: number;
    ingredientCount: number;
    createdAt: string;
}
// ═══════════════════════════════════════
// USER PROFILE
// ═══════════════════════════════════════
export interface UserProfile {
    userId: number;
    gender: string;
    dateOfBirth: string;
    age: number;
    heightCm: number;
    currentWeightKg: number;
    bmiGoal: number;
    weightGoal: number;
    dietaryGoal: string;
    bmi: number;
    bmiCategory: string;
    bmr: number;
    tdee: number;
    dailyCaloriesTarget: number;
    dailyProteinG: number;
    dailyCarbsG: number;
    dailyFatG: number;
    weightToGoal: number;
    lastWeightUpdate: string;
}
export interface CreateProfileRequest {
    gender: string;
    dateOfBirth: string;
    heightCm: number;
    currentWeightKg: number;
    bmiGoal: number;
    activityLevel: string;
}
// ═══════════════════════════════════════
// MEAL LOG
// ═══════════════════════════════════════
export interface MealLog {
    id: string;
    userId: string;
    dishId: string;
    dishName: string;
    calories: string;
    proteinG: string;
    carbsG: string;
    fatG: string;
    createdAt: string;
}
export interface CreateMealLogRequest {
    userId?: number;
    dishId: number;
}
// ═══════════════════════════════════════
// WEIGHT HISTORY
// ═══════════════════════════════════════
export interface WeightHistory {
    id: number;
    weightKg: number;
    recordedAt: string;
    changeFromPrevious?: number;
}
export interface LogWeightRequest {
    weightKg: number;
}
// ═══════════════════════════════════════
// AFCD ITEM (Nutrition Database)
// ═══════════════════════════════════════
export interface AFCDItem {
    id: number;
    name: string;
    variant?: string;
    calories: number;
    proteinG: number;
    carbsG: number;
    fatG: number;
}