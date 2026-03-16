import axios from "axios";

const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

// ──────────────────────────────────────────────
// REQUEST INTERCEPTOR — runs before every request
// ──────────────────────────────────────────────
api.interceptors.request.use(
    (config) => {
        //grab JWT token from localStorage
        const token = localStorage.getItem("token");

        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// ──────────────────────────────────────────────
// RESPONSE INTERCEPTOR — runs after every response
// ──────────────────────────────────────────────
api.interceptors.response.use(
    (response) => response, //if 2xx, just pass through

    async (error) => {
        const originalRequest = error.config;

        //if 401 then check if we refreshed
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                const refreshToken = localStorage.getItem("refreshToken");
                if (!refreshToken) throw new Error("No refresh token");

                //call refresh token endpoint to get new JWT token
                const response = await axios.post(
                    `${process.env.NEXT_PUBLIC_API_URL}/auth/refresh-token`,
                    { refreshToken }
                );

                const { token, refreshToken: newRefreshToken } = response.data;

                //Save new token in locaalstorage
                localStorage.setItem("token", token);
                localStorage.setItem("refreshToken", newRefreshToken);

                //retry original request with new token
                originalRequest.headers.Authorization = `Bearer ${token}`;
                return api(originalRequest);
            } catch {
                //refresh failed - force logout
                localStorage.removeItem("token");
                localStorage.removeItem("refreshToken");
                window.location.href = "/login";
                return Promise.reject(error)
            }
        }

        return Promise.reject(error);
    }
);


export default api;