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
  expiresAt?: string;
}

export interface CreateUrlResponse {
  id: number;
  originalUrl: string;
  shortCode: string;
  customAlias?: string;
  clickCount: number;
  createdAt: string;
  expiresAt?: string;
}

export const api = {
  urls: {
    create: (data: CreateUrlRequest) =>
      request<CreateUrlResponse>("/Url", {
        method: "POST",
        body: JSON.stringify(data),
      }),
  },
};
