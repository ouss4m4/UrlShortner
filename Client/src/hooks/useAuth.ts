import { useState, useEffect } from "react";
import type { AuthResponse } from "../lib/api";

interface User {
  id: number;
  username: string;
  email: string;
}

export function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check for existing token on mount
    const storedToken = localStorage.getItem("token");
    const storedRefreshToken = localStorage.getItem("refreshToken");
    const storedUser = localStorage.getItem("user");

    if (storedToken && storedRefreshToken && storedUser && storedUser !== "undefined") {
      try {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      } catch (error) {
        // Clear invalid data
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("user");
      }
    }
    setLoading(false);
  }, []);

  const login = (authResponse: AuthResponse) => {
    const user = {
      id: authResponse.userId,
      username: authResponse.username,
      email: authResponse.email,
    };
    localStorage.setItem("token", authResponse.accessToken);
    localStorage.setItem("refreshToken", authResponse.refreshToken);
    localStorage.setItem("user", JSON.stringify(user));
    setToken(authResponse.accessToken);
    setUser(user);
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
  };

  return {
    user,
    token,
    isAuthenticated: !!token,
    loading,
    login,
    logout,
  };
}
