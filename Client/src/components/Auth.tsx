import { useState } from "react";
import { api } from "../lib/api";
import { cn } from "../lib/utils";

interface AuthProps {
  onSuccess: (authResponse: any) => void;
  onCancel: () => void;
}

export function Auth({ onSuccess, onCancel }: AuthProps) {
  const [isLogin, setIsLogin] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [username, setUsername] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      if (isLogin) {
        const response = await api.auth.login({ email, password });
        onSuccess(response);
      } else {
        const response = await api.auth.register({ username, email, password });
        onSuccess(response);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Authentication failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-md flex items-center justify-center p-4 z-50">
      <div className="glass-strong rounded-3xl max-w-md w-full p-6 sm:p-7 space-y-6 shadow-xl">
        {/* Header */}
        <div className="space-y-2">
          <h2 className="text-2xl font-bold font-display">{isLogin ? "Welcome Back" : "Create Account"}</h2>
          <p className="text-sm text-muted-foreground">
            {isLogin ? "Sign in to manage your links and view analytics" : "Sign up to track your links and access advanced features"}
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {!isLogin && (
            <div className="space-y-2">
              <label htmlFor="username" className="text-sm font-medium">
                Username
              </label>
              <input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                minLength={3}
                className="w-full px-4 py-2 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
                placeholder="johndoe"
              />
            </div>
          )}

          <div className="space-y-2">
            <label htmlFor="email" className="text-sm font-medium">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
              placeholder="you@example.com"
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="password" className="text-sm font-medium">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={6}
              className="w-full px-4 py-2 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
              placeholder="••••••••"
            />
          </div>

          {error && <div className="p-3 bg-destructive/10 border border-destructive/20 rounded-2xl text-sm text-destructive">{error}</div>}

          <button
            type="submit"
            disabled={loading}
            className={cn(
              "w-full px-4 py-2 rounded-xl bg-primary text-primary-foreground font-medium",
              loading && "opacity-50 cursor-not-allowed",
            )}
          >
            {loading ? "Please wait..." : isLogin ? "Sign In" : "Create Account"}
          </button>
        </form>

        {/* Toggle */}
        <div className="text-center space-y-2">
          <button
            type="button"
            onClick={() => {
              setIsLogin(!isLogin);
              setError("");
            }}
            className="text-sm text-muted-foreground hover:text-foreground"
          >
            {isLogin ? "Don't have an account? " : "Already have an account? "}
            <span className="text-primary font-medium">{isLogin ? "Sign up" : "Sign in"}</span>
          </button>

          <button type="button" onClick={onCancel} className="block w-full text-sm text-muted-foreground hover:text-foreground">
            Continue as guest
          </button>
        </div>
      </div>
    </div>
  );
}
