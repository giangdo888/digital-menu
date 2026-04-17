"use client";

import { useState, useEffect } from "react";
import { WeightHistory } from "@/types";
import { userService } from "@/services/userService";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";
import toast from "react-hot-toast";

export default function SummaryPage() {
    const [weightData, setWeightData] = useState<WeightHistory[]>([]);
    const [newWeight, setNewWeight] = useState("");
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const res = await userService.getWeightHistory(30);
                setWeightData(res.data);
            } catch {
                console.error("Failed to load weight history");
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, []);

    const handleLogWeight = async (e: React.FormEvent) => {
        e.preventDefault();
        const weightValue = parseFloat(newWeight);
        if (!newWeight || isNaN(weightValue)) return;

        // Check if already logged today
        const todayStr = new Date().toISOString().split("T")[0];
        const loggedToday = weightData.find(w => w.recordedAt.split("T")[0] === todayStr);

        if (loggedToday) {
            const confirmOverride = window.confirm(
                `Already logged weight today, do you want to override?`
            );
            if (!confirmOverride) return;
        }

        try {
            await userService.logWeight({ weightKg: weightValue });
            toast.success(loggedToday ? "Weight updated! 📈" : "Weight logged! 📊");
            // Refresh data
            const res = await userService.getWeightHistory(30);
            setWeightData(res.data);
            setNewWeight("");
        } catch {
            toast.error("Failed to log weight");
        }
    };

    // Transform data for the chart
    const chartData = weightData.map((w) => ({
        date: new Date(w.recordedAt).toLocaleDateString("en-AU", { day: "numeric", month: "short" }),
        weight: w.weightKg,
    })).reverse(); // API returns newest first, chart needs oldest first

    const currentWeight = weightData[0]?.weightKg;
    const change = weightData[0]?.changeFromPrevious;

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <h1 className="text-2xl font-bold mb-6">Weekly Summary</h1>

            {/* Weight Chart */}
            <div className="bg-bg-card rounded-xl p-5 mb-6">
                <h2 className="font-semibold mb-4">Weight History</h2>

                {chartData.length > 1 ? (
                    <ResponsiveContainer width="100%" height={250}>
                        <LineChart data={chartData}>
                            <XAxis dataKey="date" tick={{ fill: "#9CA3AF", fontSize: 12 }} axisLine={false} />
                            <YAxis domain={["auto", "auto"]} tick={{ fill: "#9CA3AF", fontSize: 12 }} axisLine={false} />
                            <Tooltip
                                contentStyle={{ backgroundColor: "#252836", border: "none", borderRadius: "8px", color: "#F9FAFB" }}
                            />
                            <Line type="monotone" dataKey="weight" stroke="#10B981" strokeWidth={2} dot={{ fill: "#10B981", r: 4 }} />
                        </LineChart>
                    </ResponsiveContainer>
                ) : (
                    <p className="text-text-secondary text-center py-8">Log at least 2 weights to see a chart.</p>
                )}

                {/* Current weight info */}
                {currentWeight && (
                    <div className="flex items-center justify-between mt-4 pt-4 border-t border-bg-elevated">
                        <div>
                            <p className="text-sm text-text-secondary">Current Weight</p>
                            <p className="text-xl font-bold">{currentWeight} kg</p>
                        </div>
                        {change !== null && change !== undefined && (
                            <p className={`text-sm font-medium ${change < 0 ? "text-success" : change > 0 ? "text-danger" : "text-text-secondary"}`}>
                                {change > 0 ? "+" : ""}{change.toFixed(1)} kg from last
                            </p>
                        )}
                    </div>
                )}
            </div>

            {/* Log Weight Form */}
            <div className="bg-bg-card rounded-xl p-5">
                <h2 className="font-semibold mb-3">Log New Weight</h2>
                <form onSubmit={handleLogWeight} className="flex gap-3">
                    <input
                        type="number"
                        step="0.1"
                        value={newWeight}
                        onChange={(e) => setNewWeight(e.target.value)}
                        placeholder="e.g., 76.5"
                        className="flex-1 bg-bg-elevated border border-bg-elevated rounded-lg px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                    />
                    <button type="submit" className="bg-accent hover:bg-accent-hover text-white px-6 py-3 rounded-lg font-medium">
                        Log
                    </button>
                </form>
            </div>
        </div>
    );
}
