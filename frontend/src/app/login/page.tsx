"use client"

import { useAuth } from "@/context/AuthContext";
import Link from "next/link";
import { useRouter } from 'next/navigation';
import { useState } from "react";
import toast from "react-hot-toast";

export default function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { login } = useAuth();
    const router = useRouter();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault(); // Prevent browser from refreshing the page
        setIsSubmitting(true);

        try {
            await login(email, password);
            toast.success("Welcome back! 👋");
            router.push("/");
        } catch {
            toast.error("Invalid email or password");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div>
            <div className="min-h-[calc(100vh-144px)] md:min-h-[calc(100vh-64px)] flex items-center justify-center px-4">
                <div className="w-full max-w-md bg-bg-card rounded-2xl p-8">
                    <h1 className="text-2xl font-bold text-center mb-2"> Welcome Back</h1>
                    <p className="text-center text-text-secondary mb-8">Sign in to your account</p>

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div>
                            <label className="text-sm text-text-secondary block mb-1">
                                Email
                            </label>
                            <input
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                                className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 mb-5 text-text-primary focus:outline-none focus:border-accent"
                                placeholder="you@example.com"
                            />

                            <div>
                                <label className="text-sm text-text-secondary block mb-1">
                                    Password
                                </label>
                                <input
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 mb-10 text-text-primary focus:outline-none focus:border-accent"
                                    placeholder="your password"
                                />
                            </div>

                            <button
                                type="submit"
                                disabled={isSubmitting}
                                className="w-full bg-accent hover:bg-accent-hover disabled:opacity-50 text-white font-semibold py-3 rounded-xl transition-colors"
                            >
                                {isSubmitting ? "Logging in..." : "Log In"}
                            </button>
                        </div>
                    </form>

                    <p className="text-center text-text-secondary text-sm mt-6">
                        Don&apos;t have an account?{" "}
                        <Link href="/register" className="text-accent hover:underline">
                            Sign Up
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
}