"use client";

import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { usePathname, useRouter } from "next/navigation";

export default function NavBar() {
    const { user, isAuthenticated, logout } = useAuth();
    const pathName = usePathname();
    const router = useRouter();
    const isCustomer = user?.role === "customer";
    const isRestaurantAdmin = user?.role === "restaurant_admin";

    const handleLogout = () => {
        logout();
        router.push("/");
    };

    const isActive = (path: string) => {
        return pathName === path ? "text-accent" : "text-text-secondary hover:text-text-primary";
    }

    // ── Mobile bottom nav items (role-based) ──
    let mobileNav;

    if (!isAuthenticated) {
        mobileNav = (
            <>
                <Link href="/" className={`flex flex-col items-center gap-1 text-xs ${isActive("/")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12l8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25" /></svg>
                    <span>Home</span>
                </Link>
                <Link href="/login" className={`flex flex-col items-center gap-1 text-xs ${isActive("/login")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M15.75 5.25a3 3 0 013 3m3 0a6 6 0 01-7.029 5.912c-.563-.097-1.159.026-1.563.43L10.5 17.25H8.25v2.25H6v2.25H2.25v-2.818c0-.597.237-1.17.659-1.591l6.499-6.499c.404-.404.527-.1.43-1.563A6 6 0 1121.75 8.25z" /></svg>
                    <span>Login</span>
                </Link>
            </>
        );
    } else if (isCustomer) {
        mobileNav = (
            <>
                <Link href="/" className={`flex flex-col items-center gap-1 text-xs ${isActive("/")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12l8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25" /></svg>
                    <span>Home</span>
                </Link>
                <Link href="/dashboard/meals" className={`flex flex-col items-center gap-1 text-xs ${isActive("/dashboard/meals")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" /></svg>
                    <span>My Meals</span>
                </Link>
                <Link href="/dashboard/summary" className={`flex flex-col items-center gap-1 text-xs ${isActive("/dashboard/summary")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 013 19.875v-6.75zM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V8.625zM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V4.125z" /></svg>
                    <span>Summary</span>
                </Link>
                <Link href="/dashboard/profile" className={`flex flex-col items-center gap-1 text-xs ${isActive("/dashboard/profile")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" /></svg>
                    <span>Profile</span>
                </Link>
            </>
        );
    } else if (isRestaurantAdmin) {
        mobileNav = (
            <>
                <Link href="/admin/restaurants" className={`flex flex-col items-center gap-1 text-xs ${isActive("/admin/restaurants")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M13.5 21v-7.5a.75.75 0 01.75-.75h3a.75.75 0 01.75.75V21m-4.5 0H2.36m11.14 0H18m0 0h3.64m-1.39 0V9.349m-16.5 11.65V9.35m0 0a3.001 3.001 0 003.75-.615A2.993 2.993 0 009.75 9.75c.896 0 1.7-.393 2.25-1.016a2.993 2.993 0 002.25 1.016c.896 0 1.7-.393 2.25-1.016a3.001 3.001 0 003.75.614m-16.5 0a3.004 3.004 0 01-.621-4.72L4.318 3.44A1.5 1.5 0 015.378 3h13.243a1.5 1.5 0 011.06.44l1.19 1.189a3 3 0 01-.621 4.72m-13.5 8.65h3.75a.75.75 0 00.75-.75V13.5a.75.75 0 00-.75-.75H6.75a.75.75 0 00-.75.75v3.75c0 .415.336.75.75.75z" /></svg>
                    <span>Restaurants</span>
                </Link>
                <Link href="/admin/account" className={`flex flex-col items-center gap-1 text-xs ${isActive("/admin/account")}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6"><path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" /></svg>
                    <span>Account</span>
                </Link>
            </>
        );
    }

    return (
        <>
            {/* ── Desktop Top Nav (hidden on mobile) ── */}
            <nav className="hidden md:flex sticky top-0 left-0 right-0 z-50 bg-bg-card border-b border-bg-elevated h-16 items-center px-8">
                <Link href={isRestaurantAdmin ? "/admin/restaurants" : "/"} className="text-accent font-bold text-xl mr-8">
                    Digital Menu
                </Link>

                <div className="flex gap-6 flex-1">
                    {/* Customer desktop links */}
                    {(!isAuthenticated || isCustomer) && (
                        <Link href="/" className={isActive("/")}>
                            Home
                        </Link>
                    )}
                    {isCustomer && (
                        <>
                            <Link href="/dashboard/meals" className={isActive("/dashboard/meals")}>
                                My Meals
                            </Link>
                            <Link href="/dashboard/summary" className={isActive("/dashboard/summary")}>
                                Summary
                            </Link>
                            <Link href="/dashboard/profile" className={isActive("/dashboard/profile")}>
                                Profile
                            </Link>
                        </>
                    )}
                    {/* Restaurant admin desktop links */}
                    {isRestaurantAdmin && (
                        <>
                            <Link href="/admin/restaurants" className={isActive("/admin/restaurants")}>
                                Restaurants
                            </Link>
                            <Link href="/admin/account" className={isActive("/admin/account")}>
                                Account
                            </Link>
                        </>
                    )}
                </div>

                <div className="flex items-center gap-4">
                    {isAuthenticated ? (
                        <>
                            <span className="text-text-secondary text-sm">
                                {user?.email}
                            </span>
                            <button
                                onClick={handleLogout}
                                className="text-sm text-danger hover:text-red-400"
                            >
                                Logout
                            </button>
                        </>
                    ) : (
                        <>
                            <Link href="/login" className={isActive("/login")}>
                                Login
                            </Link>
                            <Link href="/register" className="bg-accent hover:bg-accent-hover text-white px-4 py-2 rounded-lg text-sm">
                                Sign up
                            </Link>
                        </>
                    )}
                </div>
            </nav>

            {/* ── Mobile Bottom Nav (hidden on desktop) ── */}
            <nav className="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-bg-card border-t border-bg-elevated h-16 flex items-center justify-around px-4">
                {mobileNav}
            </nav>
        </>
    );
}