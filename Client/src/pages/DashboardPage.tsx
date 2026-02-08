import { useState } from "react";
import { api } from "../lib/api";
import { cn } from "../lib/utils";
import { Dashboard } from "../components/Dashboard";

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
      await api.urls.create({
        originalUrl: url,
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
    <div className="min-h-screen bg-background">
      <ToastContainer />

      {/* Header */}
      <header className="border-b border-border bg-card sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="text-xl font-bold">🔗 UrlShort</div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-muted-foreground">Hi, {user.username}</span>
            <button onClick={onLogout} className="px-4 py-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
        {/* Compact Create Form */}
        <div className="bg-card border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold mb-4">Create Short Link</h2>
          <form onSubmit={handleCreate} className="space-y-3">
            <div className="flex gap-3">
              <input
                type="url"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                placeholder="Paste long URL"
                required
                className="flex-1 px-4 py-2 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
              />
              {showAdvanced && (
                <input
                  type="text"
                  value={customAlias}
                  onChange={(e) => setCustomAlias(e.target.value)}
                  placeholder="Custom alias (optional)"
                  pattern="[a-zA-Z0-9]{4,12}"
                  className="w-48 px-4 py-2 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                />
              )}
              <button
                type="submit"
                disabled={loading}
                className={cn(
                  "px-6 py-2 bg-primary text-primary-foreground rounded-lg font-medium hover:opacity-90 transition-opacity",
                  loading && "opacity-50 cursor-not-allowed",
                )}
              >
                {loading ? "Creating..." : "Create"}
              </button>
            </div>

            {showAdvanced && (
              <div className="flex gap-3">
                <input
                  type="text"
                  value={category}
                  onChange={(e) => setCategory(e.target.value)}
                  placeholder="Category"
                  className="flex-1 px-4 py-2 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                />
                <input
                  type="text"
                  value={tags}
                  onChange={(e) => setTags(e.target.value)}
                  placeholder="Tags (comma-separated)"
                  className="flex-1 px-4 py-2 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                />
                <input
                  type="datetime-local"
                  value={expiry}
                  onChange={(e) => setExpiry(e.target.value)}
                  className="px-4 py-2 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                />
              </div>
            )}

            <button type="button" onClick={() => setShowAdvanced(!showAdvanced)} className="text-sm text-primary hover:underline">
              {showAdvanced ? "Hide" : "Show"} advanced options
            </button>
          </form>
        </div>

        {/* Dashboard */}
        <Dashboard key={dashboardKey} userId={user.id} onSuccess={onSuccess} onError={onError} />
      </div>
    </div>
  );
}
