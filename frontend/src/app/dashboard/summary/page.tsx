"use client";

import { useState, useEffect } from "react";
import { UserProfile, WeightHistory } from "@/types";
import { userService } from "@/services/userService";
import { mealLogService } from "@/services/mealLogService";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";
import toast from "react-hot-toast";

export default function SummaryPage() {
    const defaultStart = new Date();
    defaultStart.setDate(defaultStart.getDate() - 6);
    
    const [startDate, setStartDate] = useState(defaultStart.toISOString().split("T")[0]);
    const [endDate, setEndDate] = useState(new Date().toISOString().split("T")[0]);
    
    const [weightData, setWeightData] = useState<WeightHistory[]>([]);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [nutritionData, setNutritionData] = useState<any[]>([]);
    
    const [newWeight, setNewWeight] = useState("");
    const [isLoading, setIsLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const MAX_DAYS = 180; // 6 months limit
    const todayString = new Date().toISOString().split("T")[0];

    const handleStartDateChange = (val: string) => {
        const start = new Date(val);
        const end = new Date(endDate);
        if (start > new Date()) {
            toast.error("Start date cannot be in the future");
            return;
        }
        if ((end.getTime() - start.getTime()) / (1000 * 3600 * 24) > MAX_DAYS) {
            toast.error("Date span cannot exceed 6 months");
            return;
        }
        setStartDate(val);
    };

    const handleEndDateChange = (val: string) => {
        const start = new Date(startDate);
        const end = new Date(val);
        if (end > new Date()) {
            toast.error("End date cannot be in the future");
            return;
        }
        if (end < start) {
            toast.error("End date cannot be before start date");
            return;
        }
        if ((end.getTime() - start.getTime()) / (1000 * 3600 * 24) > MAX_DAYS) {
            toast.error("Date span cannot exceed 6 months");
            return;
        }
        setEndDate(val);
    };

    useEffect(() => {
// ... omitting unchanged fetch code for space ...
        const fetchData = async () => {
            setIsLoading(true);
            try {
                const [weightRes, profileRes] = await Promise.all([
                    userService.getWeightHistory(30, startDate, endDate),
                    userService.getProfile()
                ]);
                setWeightData(weightRes.data);
                setProfile(profileRes.data);
            } catch {
                console.error("Failed to load summary data");
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, [startDate, endDate]);

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
            const res = await userService.getWeightHistory(30, startDate, endDate);
            setWeightData(res.data);
            setNewWeight("");
        } catch {
            toast.error("Failed to log weight");
        }
    };

    // Transform data for the chart, using numeric timestamps for accurate spacing
    const chartData = weightData.map((w) => ({
        timestamp: new Date(w.recordedAt).getTime(),
        weight: w.weightKg,
    })).reverse(); // API returns newest first, chart needs oldest first

    const currentWeight = weightData[0]?.weightKg;
    const change = weightData[0]?.changeFromPrevious;

    const getWeightChangeColor = () => {
        if (change === 0 || change === undefined || change === null) return "text-text-secondary";
        if (!profile) return change < 0 ? "text-success" : "text-danger";
        
        if (profile.weeklyWeightGoal < 0) { // Goal: Lose weight
            return change < 0 ? "text-success" : "text-danger";
        } else if (profile.weeklyWeightGoal > 0) { // Goal: Gain weight
            return change > 0 ? "text-success" : "text-danger";
        }
        return "text-text-secondary"; // Maintenance
    };

    const openNutritionModal = async () => {
        setIsModalOpen(true);
        try {
            const res = await mealLogService.getSummary(startDate, endDate);
            setNutritionData(res.data);
        } catch {
            toast.error("Failed to load nutrition summary");
        }
    };

    if (isLoading) return <div className="text-center py-20 text-text-secondary">Loading...</div>;

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
                <h1 className="text-2xl font-bold">Time-Period Summary</h1>
                <div className="flex items-center gap-2 bg-bg-card border border-border p-2 rounded-sm">
                    <input 
                        type="date" 
                        value={startDate} 
                        max={todayString}
                        onChange={(e) => handleStartDateChange(e.target.value)}
                        className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm focus:outline-none"
                    />
                    <span className="text-text-secondary">to</span>
                    <input 
                        type="date" 
                        value={endDate}
                        max={todayString}
                        onChange={(e) => handleEndDateChange(e.target.value)}
                        className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm focus:outline-none"
                    />
                </div>
            </div>

            {/* Weight Chart */}
            <div className="bg-bg-card border border-border rounded-sm p-5 mb-6 shadow-luxury">
                <h2 className="font-semibold mb-4">Weight History</h2>

                {chartData.length > 1 ? (
                    <ResponsiveContainer width="100%" height={250}>
                        <LineChart data={chartData}>
                            <XAxis 
                                dataKey="timestamp" 
                                type="number" 
                                domain={['dataMin', 'dataMax']} 
                                scale="time"
                                tickFormatter={(tick) => new Date(tick).toLocaleDateString("en-AU", { day: "numeric", month: "short" })} 
                                tick={{ fill: "#78716C", fontSize: 12 }} 
                                axisLine={false} 
                            />
                            <YAxis domain={["auto", "auto"]} tick={{ fill: "#78716C", fontSize: 12 }} axisLine={false} />
                            <Tooltip
                                labelFormatter={(label) => new Date(label).toLocaleDateString("en-AU", { day: "numeric", month: "short", year: "numeric" })}
                                contentStyle={{ backgroundColor: "#FFFFFF", border: "1px solid #E8E2D9", borderRadius: "4px", color: "#1C1917" }}
                            />
                            <Line type="monotone" dataKey="weight" stroke="#B8943F" strokeWidth={2} dot={{ fill: "#B8943F", r: 4 }} />
                        </LineChart>
                    </ResponsiveContainer>
                ) : (
                    <p className="text-text-secondary text-center py-8">Log at least 2 weights to see a chart.</p>
                )}

                {/* Current weight info */}
                {currentWeight && (
                    <div className="flex items-center justify-between mt-4 pt-4 border-t border-border">
                        <div>
                            <p className="text-sm text-text-secondary">Current Weight</p>
                            <p className="text-xl font-bold">{currentWeight} kg</p>
                        </div>
                        {change !== null && change !== undefined && (
                            <p className={`text-sm font-medium ${getWeightChangeColor()}`}>
                                {change > 0 ? "+" : ""}{change.toFixed(1)} kg from last
                            </p>
                        )}
                    </div>
                )}
            </div>

            {/* Bottom Actions */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Log Weight Form */}
                <div className="bg-bg-card border border-border rounded-sm p-5 shadow-luxury">
                    <h2 className="font-semibold mb-3">Log New Weight</h2>
                    <form onSubmit={handleLogWeight} className="flex gap-3">
                        <input
                            type="number"
                            step="0.1"
                            value={newWeight}
                            onChange={(e) => setNewWeight(e.target.value)}
                            placeholder="e.g., 76.5"
                            className="flex-1 bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
                        />
                        <button type="submit" className="bg-accent hover:bg-accent-hover text-white px-6 py-3 rounded-sm font-medium">
                            Log
                        </button>
                    </form>
                </div>

                {/* Nutrition Summary Action */}
                <div className="bg-bg-card border border-border rounded-sm p-5 flex flex-col justify-center items-center text-center shadow-luxury">
                    <h2 className="font-semibold mb-2">Nutrition Intake</h2>
                    <p className="text-sm text-text-secondary mb-4">View your day-by-day nutritional breakdown for the selected time period.</p>
                    <button 
                        onClick={openNutritionModal}
                        className="bg-bg-elevated hover:bg-bg-hover text-text-primary px-6 py-3 rounded-sm font-medium border border-border w-full transition-colors"
                    >
                        View Nutrition Intake for Period
                    </button>
                </div>
            </div>

            {/* Nutrition Modal */}
            {isModalOpen && profile && (
                <div className="fixed top-16 bottom-0 left-0 right-0 z-[100] flex items-center justify-center p-4 bg-stone-900/40 backdrop-blur-sm" onClick={() => setIsModalOpen(false)}>
                    <div 
                        className="bg-bg-card border border-border rounded-sm w-full max-w-2xl max-h-[80vh] overflow-hidden flex flex-col shadow-2xl"
                        onClick={e => e.stopPropagation()}
                    >
                        <div className="p-5 border-b border-border flex justify-between items-center">
                            <h2 className="text-lg font-bold">Nutrition Summary</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-text-secondary hover:text-text-primary text-xl">&times;</button>
                        </div>
                        <div className="p-5 overflow-y-auto">
                            {nutritionData.length === 0 ? (
                                <p className="text-center text-text-secondary py-8">No data found for this period.</p>
                            ) : (
                                <div className="space-y-4">
                                    {nutritionData.map((day) => {
                                        // Traffic Light Logic
                                        const pCal = (day.calories / profile.dailyCaloriesTarget) * 100;
                                        const pPro = (day.proteinG / profile.dailyProteinG) * 100;
                                        const pCarb = (day.carbsG / profile.dailyCarbsG) * 100;
                                        const pFat = (day.fatG / profile.dailyFatG) * 100;

                                        let minGreen = 90;
                                        let maxGreen = 105;
                                        if (profile.weeklyWeightGoal < 0) { // Lose 
                                            minGreen = 85; maxGreen = 100;
                                        } else if (profile.weeklyWeightGoal > 0) { // Gain
                                            minGreen = 95; maxGreen = 110;
                                        }

                                        const getColor = (p: number) => {
                                            if (p === 0) return "text-text-secondary";
                                            if (p > maxGreen) return "text-danger";  // Red (over limit)
                                            if (p >= minGreen) return "text-success"; // Green (target hit)
                                            return "text-warning";               // Yellow (not there yet)
                                        };

                                        return (
                                            <div key={day.date} className="bg-bg-elevated rounded-sm p-4 flex flex-col sm:flex-row gap-4 items-center justify-between">
                                                <div className="font-medium text-text-primary w-24">
                                                    {new Date(day.date).toLocaleDateString("en-AU", { weekday: 'short', month: 'short', day: 'numeric' })}
                                                </div>
                                                <div className="flex gap-4 md:gap-8 flex-1 justify-end">
                                                    <div className="text-center">
                                                        <div className="text-xs text-text-secondary">Cal</div>
                                                        <div className={`font-bold ${getColor(pCal)}`}>{Math.round(day.calories)}</div>
                                                    </div>
                                                    <div className="text-center">
                                                        <div className="text-xs text-text-secondary">Pro</div>
                                                        <div className={`font-bold ${getColor(pPro)}`}>{Math.round(day.proteinG)}g</div>
                                                    </div>
                                                    <div className="text-center">
                                                        <div className="text-xs text-text-secondary">Carb</div>
                                                        <div className={`font-bold ${getColor(pCarb)}`}>{Math.round(day.carbsG)}g</div>
                                                    </div>
                                                    <div className="text-center">
                                                        <div className="text-xs text-text-secondary">Fat</div>
                                                        <div className={`font-bold ${getColor(pFat)}`}>{Math.round(day.fatG)}g</div>
                                                    </div>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
