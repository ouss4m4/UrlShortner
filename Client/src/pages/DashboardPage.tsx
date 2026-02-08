import { useState } from "react";
import { api } from "../lib/api";
import { cn } from "../lib/utils";
import { Dashboard } from "../components/Dashboard";
import { validateUrl } from "../lib/url";

interface DashboardPageProps {
  user: { id: number; username: string; email: string };
  onLogout: () => void;
  ToastContainer: () => React.ReactElement;
  onSuccess: (message: string) => void;
  onError: (message: string) => void;
}

export function DashboardPage({ user, onLogout, ToastContainer, onSuccess, onError }: DashboardPageProps) {
  const [url, setUrl] = useState("");
  const [customAlias, setCustomAlias] = useState("");
  const [loading, setLoading] = useState(false);
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [category, setCategory] = useState("");
  const [tags, setTags] = useState("");
  const [expiry, setExpiry] = useState("");
  const [dashboardKey, setDashboardKey] = useState(0);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const validation = validateUrl(url);
      if (!validation.valid || !validation.normalized) {
        onError(validation.message ?? "Invalid URL format");
        return;
      }

      await api.urls.create({
        originalUrl: validation.normalized,
        ...(customAlias && { shortCode: customAlias }),
        ...(category && { category }),
        ...(tags && { tags }),
        ...(expiry && { expiry: new Date(expiry).toISOString() }),
      });

      setUrl("");
      setCustomAlias("");
      setCategory("");
      setTags("");
      setExpiry("");
      setShowAdvanced(false);
      onSuccess("URL created successfully!");
      setDashboardKey((prev) => prev + 1);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Failed to create URL";
      onError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen">
      <ToastContainer />

      {/* Header */}
      <header className="sticky top-0 z-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="glass rounded-2xl px-5 py-3 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="h-9 w-9 rounded-xl bg-primary/10 text-primary flex items-center justify-center font-semibold">US</div>
              <div>
                <div className="text-lg font-semibold tracking-tight">UrlShort</div>
                <div className="text-xs text-muted-foreground">Dashboard</div>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-sm text-muted-foreground">Hi, {user.username}</span>
              <button
                onClick={onLogout}
                className="px-4 py-2 text-sm rounded-full border border-white/60 hover:bg-white/70 transition-colors"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
        {/* Create Form */}
        <div className="glass-strong rounded-3xl p-6 sm:p-7">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
            <div>
              <h2 className="text-lg font-semibold">Create Short Link</h2>
              <p className="text-sm text-muted-foreground">Launch a new link with tracking baked in.</p>
            </div>
            <button
              type="button"
              onClick={() => setShowAdvanced(!showAdvanced)}
              className="text-xs uppercase tracking-wide px-3 py-2 rounded-full border border-white/60 hover:bg-white/70 transition-colors"
            >
              {showAdvanced ? "Hide" : "Show"} advanced options
            </button>
          </div>

          <form onSubmit={handleCreate} className="space-y-4">
            <div className="flex flex-col lg:flex-row gap-3">
              <input
                type="text"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                placeholder="Paste long URL"
                inputMode="url"
                required
                className="flex-1 px-4 py-3 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
              />
              {showAdvanced && (
                <input
                  type="text"
                  value={customAlias}
                  onChange={(e) => setCustomAlias(e.target.value)}
                  placeholder="Custom alias"
                  pattern="[a-zA-Z0-9]{4,12}"
                  className="lg:w-48 px-4 py-3 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
                />
              )}
              <button
                type="submit"
                disabled={loading}
                className={cn(
                  "px-6 py-3 rounded-xl bg-primary text-primary-foreground font-semibold hover:opacity-90 transition-opacity",
                  loading && "opacity-50 cursor-not-allowed",
                )}
              >
                {loading ? "Creating..." : "Create"}
              </button>
            </div>

            {showAdvanced && (
              <div className="grid md:grid-cols-3 gap-3">
                <input
                  type="text"
                  value={category}
                  onChange={(e) => setCategory(e.target.value)}
                  placeholder="Category"
                  className="px-4 py-3 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
                />
                <input
                  type="text"
                  value={tags}
                  onChange={(e) => setTags(e.target.value)}
                  placeholder="Tags (comma-separated)"
                  className="px-4 py-3 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
                />
                <input
                  type="datetime-local"
                  value={expiry}
                  onChange={(e) => setExpiry(e.target.value)}
                  className="px-4 py-3 border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary"
                />
              </div>
            )}
          </form>
        </div>

        {/* Dashboard */}
        <Dashboard key={dashboardKey} userId={user.id} onSuccess={onSuccess} onError={onError} />
      </div>
    </div>
  );
}
