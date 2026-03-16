"use client"

import { useAuth } from "@/context/AuthContext";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import toast from "react-hot-toast";

export default function RegisterPage() {
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [accountType, setAccountType] = useState<"customer" | "restaurant_admin">("customer");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { register } = useAuth();
    const router = useRouter();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        console.log("Form submitted, preventing default...");
        setIsSubmitting(true);

        try {
            await register(firstName, lastName, email, password, accountType);
            toast.success("Account created! 🎉");
            router.push("/dashboard/profile");
        } catch {
            toast.error("Registration failed. Email might already exist.");
        } finally {
            setIsSubmitting(false);
        }
    }
    return (
        <div>
            <div className="min-h-[calc(100vh-144px)] md:min-h-[calc(100vh-64px)] flex items-center justify-center px-4">
                <div className="w-full max-w-md bg-bg-card rounded-2xl p-8">
                    <h1 className="text-2xl font-bold text-center mb-2">Create Account</h1>
                    <p className="text-text-secondary text-center mb-8">Start tracking your nutrition</p>

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="grid grid-cols-12 gap-4">
                            <div className="col-span-7">
                                <label className="text-sm text-text-secondary block mb-1">First Name</label>
                                <input
                                    type="text"
                                    value={firstName}
                                    onChange={(e) => setFirstName(e.target.value)}
                                    required
                                    className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                                    placeholder="First name"
                                />
                            </div>
                            <div className="col-span-5">
                                <label className="text-sm text-text-secondary block mb-1">Last Name</label>
                                <input
                                    type="text"
                                    value={lastName}
                                    onChange={(e) => setLastName(e.target.value)}
                                    required
                                    className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                                    placeholder="Last name"
                                />
                            </div>
                        </div>

                        <div>
                            <label className="text-sm text-text-secondary block mb-1">Email</label>
                            <input
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                                className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                                placeholder="you@example.com"
                            />
                        </div>

                        <div>
                            <label className="text-sm text-text-secondary block mb-1">Password</label>
                            <input
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                minLength={6}
                                className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                                placeholder="Min 6 characters"
                            />
                        </div>

                        <div>
                            <label className="text-sm text-text-secondary block mb-1">You are a ...</label>
                            <select
                                value={accountType}
                                onChange={(e) => {
                                    var value = e.target.value;
                                    if (value === "restaurant_admin") {
                                        setAccountType("restaurant_admin");
                                    } else {
                                        setAccountType("customer");
                                    }
                                }}
                                className="w-full bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent">
                                <option value="customer">Customer - Browse menus & track meals</option>
                                <option value="restaurant_admin">Restaurant Owner - Manage my menu</option>
                            </select>
                        </div>

                        <button
                            type="submit"
                            disabled={isSubmitting}
                            className="w-full bg-accent hover:bg-accent-hover disabled:opacity-50 text-white font-semibold py-3 rounded-xl transition-colors">
                            {isSubmitting ? "Creating Account..." : "Create Account"}
                        </button>
                    </form>

                    <p className="text-center text-text-secondary text-sm mt-6">
                        Already have an account?{" "}
                        <Link href="/login" className="text-accent hover:underline">
                            Sign In
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
}