// API client for backend communication
const API_BASE = "/api";

interface ApiError {
  error: string;
  message: string;
}

let isRefreshing = false;
let refreshQueue: Array<() => void> = [];

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) {
    return null;
  }

  try {
    const response = await fetch(`${API_BASE}/Auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      // Refresh failed, clear tokens
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("user");
      window.location.href = "/";
      return null;
    }

    const data = await response.json();
    localStorage.setItem("token", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    return data.accessToken;
  } catch (error) {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    window.location.href = "/";
    return null;
  }
}

async function request<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem("token");
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };

  let response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers: {
      ...headers,
      ...options?.headers,
    },
  });

  // Handle 401 - try to refresh token and retry
  if (response.status === 401 && !endpoint.includes("/Auth/")) {
    if (!isRefreshing) {
      isRefreshing = true;
      const newToken = await refreshAccessToken();
      isRefreshing = false;

      // Resolve all queued requests
      refreshQueue.forEach((callback) => callback());
      refreshQueue = [];

      if (newToken) {
        // Retry original request with new token
        response = await fetch(`${API_BASE}${endpoint}`, {
          ...options,
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${newToken}`,
            ...options?.headers,
          },
        });
      }
    } else {
      // Wait for refresh to complete
      await new Promise<void>((resolve) => {
        refreshQueue.push(resolve);
      });

      // Retry with refreshed token
      const newToken = localStorage.getItem("token");
      if (newToken) {
        response = await fetch(`${API_BASE}${endpoint}`, {
          ...options,
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${newToken}`,
            ...options?.headers,
          },
        });
      }
    }
  }

  if (!response.ok) {
    let errorMessage = "Request failed";
    try {
      const error = (await response.json()) as ApiError;
      errorMessage = error.message || errorMessage;
    } catch {
      errorMessage = `Request failed with status ${response.status}`;
    }
    throw new Error(errorMessage);
  }

  // Handle empty responses
  const text = await response.text();
  if (!text) {
    return {} as T;
  }

  return JSON.parse(text) as T;
}

export interface CreateUrlRequest {
  originalUrl: string;
  shortCode?: string; // Backend expects 'shortCode', not 'customAlias'
  expiry?: string | null;
  category?: string | null;
  tags?: string | null;
}

export interface CreateUrlResponse {
  id: number;
  userId?: number | null;
  originalUrl: string;
  shortCode: string;
  createdAt: string;
  expiry?: string | null;
  category?: string | null;
  tags?: string | null;
}

export interface UrlAnalytics {
  urlId: number;
  shortCode: string;
  originalUrl: string;
  totalVisits: number;
  lastVisit?: string | null;
  firstVisit?: string | null;
}

export interface DateAnalytics {
  date: string;
  totalVisits: number;
  uniqueIps: number;
}

export interface CountryAnalytics {
  country: string;
  totalVisits: number;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  userId: number;
  username: string;
  email: string;
}

export const api = {
  auth: {
    register: (data: RegisterRequest) =>
      request<AuthResponse>("/Auth/register", {
        method: "POST",
        body: JSON.stringify(data),
      }),
    login: (data: LoginRequest) =>
      request<AuthResponse>("/Auth/login", {
        method: "POST",
        body: JSON.stringify(data),
      }),
  },
  urls: {
    create: (data: CreateUrlRequest) =>
      request<CreateUrlResponse>("/Url", {
        method: "POST",
        body: JSON.stringify(data),
      }),
    list: (userId: number) => request<CreateUrlResponse[]>(`/Url/user/${userId}`),
    delete: (id: number) =>
      request<void>(`/Url/${id}`, {
        method: "DELETE",
      }),
  },
  analytics: {
    url: (urlId: number) => request<UrlAnalytics>(`/Analytics/url/${urlId}`),
    byDate: (start: string, end: string) =>
      request<DateAnalytics[]>(`/Analytics/date?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`),
    byCountry: () => request<CountryAnalytics[]>("/Analytics/country"),
  },
};
