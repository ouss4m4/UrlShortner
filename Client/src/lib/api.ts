// API client for backend communication
const API_BASE = "/api";

interface ApiError {
  error: string;
  message: string;
}

async function request<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem("token");
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers: {
      ...headers,
      ...options?.headers,
    },
  });

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
