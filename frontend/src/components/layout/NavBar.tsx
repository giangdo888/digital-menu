"use client";

import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { usePathname, useRouter } from "next/navigation";

export default function NavBar() {
    const { user, isAuthenticated, logout } = useAuth();
    const pathName = usePathname();
    const router = useRouter();

    const handleLogout = () => {
        logout();
        router.push("/");
    };

    const isActive = (path: string) => {
        return pathName === path ? "text-accent" : "text-text-secondary hover:text-text-primary";
    }

    return (
        <>
            {/* ── Desktop Top Nav (hidden on mobile) ── */}
            <nav className="hidden md:flex fixed top-0 left-0 right-0 z-50 bg-bg-card border-b border-bg-elevated h-16 items-center px-8">
                <Link href="/" className="text-accent font-bold text-xl mr-8">
                    Digital Menu
                </Link>

                <div className="flex gap-6 flex-1">
                    <Link href="/" className={isActive("/")}>
                        Home
                    </Link>
                    {isAuthenticated && (
                        <>
                            <Link href="/dashboard/meals" className={isActive("/dashboard/meals")}>
                                My Meals
                            </Link>
                            <Link href="/dashboard/summary" className={isActive("/dashboard/summary")}>
                                Summary
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
                <Link href="/" className={`flex flex-col items-center text-xs ${isActive("/")}`}>
                    <span className="text-lg">🏠</span>
                    <span>Home</span>
                </Link>
                {isAuthenticated ? (
                    <>
                        <Link href="/dashboard/meals" className={`flex flex-col items-center text-xs ${isActive("/dashboard/meals")}`}>
                            <span className="text-lg">🍽️</span>
                            <span>My Meals</span>
                        </Link>
                        <Link href="/dashboard/summary" className={`flex flex-col items-center text-xs ${isActive("/dashboard/summary")}`}>
                            <span className="text-lg">📊</span>
                            <span>Summary</span>
                        </Link>
                        <Link href="/dashboard/profile" className={`flex flex-col items-center text-xs ${isActive("/dashboard/profile")}`}>
                            <span className="text-lg">👤</span>
                            <span>Profile</span>
                        </Link>
                    </>
                ) : (
                    <Link href="/login" className={`flex flex-col items-center text-xs ${isActive("/login")}`}>
                        <span className="text-lg">🔑</span>
                        <span>Login</span>
                    </Link>
                )}
            </nav>
        </>
    );
}