export function formatApiValidationErrors(error: any): string {
    const data = error?.response?.data;
    if (!data) return error?.message || "An error occurred";

    // If ASP.NET Core validation format
    if (data.errors && typeof data.errors === "object") {
        const parts: string[] = [];
        for (const key of Object.keys(data.errors)) {
            const vals = data.errors[key];
            if (Array.isArray(vals)) {
                parts.push(`${key}: ${vals.join(" ")}`);
            } else {
                parts.push(`${key}: ${String(vals)}`);
            }
        }
        return parts.join("\n");
    }

    // Fallback to title/message
    return data.title || data.message || JSON.stringify(data);
}

export function parseApiValidationErrors(error: any): Record<string, string[]> {
    const data = error?.response?.data;
    if (!data) return { _general: [error?.message || "An error occurred"] };

    if (data.errors && typeof data.errors === "object") {
        // Ensure keys map to string[]
        const out: Record<string, string[]> = {};
        for (const key of Object.keys(data.errors)) {
            const vals = data.errors[key];
            out[key] = Array.isArray(vals) ? vals.map(String) : [String(vals)];
        }
        return out;
    }

    // fallback
    return { _general: [data.title || data.message || JSON.stringify(data)] };
}

export default { formatApiValidationErrors, parseApiValidationErrors };
