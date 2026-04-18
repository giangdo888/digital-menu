"use client";

import { useState, useEffect } from "react";
import { AFCDItem } from "@/types";
import { dishService } from "@/services/dishService";

interface Props {
    onSelect: (item: AFCDItem) => void;
}

export default function AFCDSearch({ onSelect }: Props) {
    const [query, setQuery] = useState("");
    const [results, setResults] = useState<AFCDItem[]>([]);
    const [isSearching, setIsSearching] = useState(false);

    // Debounced search — waits 300ms after user stops typing
    useEffect(() => {
        if (query.length < 2) {
            setResults([]);
            return;
        }

        const timer = setTimeout(async () => {
            setIsSearching(true);
            try {
                const res = await dishService.searchAFCD(query);
                setResults(res.data);
            } catch {
                setResults([]);
            } finally {
                setIsSearching(false);
            }
        }, 300);

        // Cleanup: if user types again before 300ms, cancel the old timer
        return () => clearTimeout(timer);
    }, [query]);

    return (
        <div className="relative">
            <input
                type="text"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Search ingredients (e.g., chicken breast)..."
                className="w-full bg-bg-primary border border-border rounded-sm px-4 py-3 text-text-primary focus:outline-none focus:border-accent"
            />

            {/* Dropdown Results */}
            {(results.length > 0 || isSearching) && (
                <div className="absolute z-10 top-full left-0 right-0 mt-1 bg-bg-card border border-border rounded-sm max-h-60 overflow-y-auto shadow-lg">
                    {isSearching && <p className="p-3 text-sm text-text-secondary">Searching...</p>}
                    {results.map((item) => (
                        <button
                            key={item.id}
                            onClick={() => { onSelect(item); setQuery(""); setResults([]); }}
                            className="w-full text-left px-4 py-2 hover:bg-bg-elevated text-sm border-b border-border last:border-0"
                        >
                            <span className="font-medium">{item.name}</span>
                            {item.variant && <span className="text-text-secondary ml-1">({item.variant})</span>}
                            <span className="text-xs text-text-secondary ml-2">
                                {Math.round(item.calories)} cal per 100g
                            </span>
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
