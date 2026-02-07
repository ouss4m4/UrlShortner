import { useState } from "react";
import "./App.css";
import { api, type CreateUrlResponse } from "./lib/api";
import { cn } from "./lib/utils";
import { useAuth } from "./hooks/useAuth";
import { Auth } from "./components/Auth";
import { Dashboard } from "./components/Dashboard";

function App() {
  const [url, setUrl] = useState("");
  const [customAlias, setCustomAlias] = useState("");
  const [category, setCategory] = useState("");
  const [tags, setTags] = useState("");
  const [expiry, setExpiry] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState<CreateUrlResponse | null>(null);
  const [copied, setCopied] = useState(false);
  const [showAuth, setShowAuth] = useState(false);
  const [dashboardKey, setDashboardKey] = useState(0);

  const { user, isAuthenticated, login, logout } = useAuth();

  const handleShorten = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const data = await api.urls.create({
        originalUrl: url,
        ...(customAlias && { shortCode: customAlias }),
        ...(category && { category }),
        ...(tags && { tags }),
        ...(expiry && { expiry: new Date(expiry).toISOString() }),
      });
      setResult(data);
      setUrl("");
      setCustomAlias("");
      setCategory("");
      setTags("");
      setExpiry("");
      // Refresh dashboard if user is authenticated
      if (isAuthenticated) {
        setDashboardKey((prev) => prev + 1);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to shorten URL");
    } finally {
      setLoading(false);
    }
  };

  const shortUrl = result ? `${import.meta.env.DEV ? "http://localhost:5011" : window.location.origin}/${result.shortCode}` : "";

  const handleCopy = async () => {
    if (shortUrl) {
      await navigator.clipboard.writeText(shortUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      {/* Auth Modal */}
      {showAuth && (
        <Auth
          onSuccess={(authResponse) => {
            login(authResponse);
            setShowAuth(false);
          }}
          onCancel={() => setShowAuth(false)}
        />
      )}

      {/* Header */}
      <header className="border-b border-border">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <h1 className="text-xl font-bold">LinkShort</h1>
          <div>
            {isAuthenticated ? (
              <div className="flex items-center gap-4">
                <span className="text-sm text-muted-foreground">{user?.username}</span>
                <button onClick={logout} className="text-sm text-muted-foreground hover:text-foreground">
                  Logout
                </button>
              </div>
            ) : (
              <button onClick={() => setShowAuth(true)} className="px-4 py-2 text-sm border border-border rounded-lg hover:bg-accent">
                Sign In
              </button>
            )}
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-12 md:py-20">
        <div className="max-w-3xl mx-auto">
          {/* Header */}
          <div className="text-center space-y-4 mb-12">
            <h1 className="text-4xl md:text-6xl font-bold tracking-tight">Shorten Your Links</h1>
            <p className="text-lg md:text-xl text-muted-foreground">Create short, memorable links in seconds. No sign-up required.</p>
          </div>

          {/* URL Shortener Form */}
          <div className="space-y-4">
            <form onSubmit={handleShorten} className="space-y-4">
              <div className="flex flex-col gap-3">
                <input
                  type="url"
                  value={url}
                  onChange={(e) => setUrl(e.target.value)}
                  placeholder="Enter your long URL here..."
                  required
                  className="w-full px-4 py-4 text-lg border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                />

                <div className="flex flex-col sm:flex-row gap-3">
                  <input
                    type="text"
                    value={customAlias}
                    onChange={(e) => setCustomAlias(e.target.value)}
                    placeholder="Custom alias (optional)"
                    pattern="[a-zA-Z0-9]{4,12}"
                    title="4-12 alphanumeric characters"
                    className="flex-1 px-4 py-3 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  />
                  <input
                    type="text"
                    value={category}
                    onChange={(e) => setCategory(e.target.value)}
                    placeholder="Category (optional)"
                    className="flex-1 px-4 py-3 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  />
                </div>

                <div className="flex flex-col sm:flex-row gap-3">
                  <input
                    type="text"
                    value={tags}
                    onChange={(e) => setTags(e.target.value)}
                    placeholder="Tags (comma-separated)"
                    className="flex-1 px-4 py-3 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  />
                  <input
                    type="datetime-local"
                    value={expiry}
                    onChange={(e) => setExpiry(e.target.value)}
                    className="flex-1 px-4 py-3 border border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  />
                  <button
                    type="submit"
                    disabled={loading}
                    className={cn(
                      "px-8 py-3 bg-primary text-primary-foreground rounded-lg font-medium transition-opacity",
                      loading && "opacity-50 cursor-not-allowed",
                    )}
                  >
                    {loading ? "Shortening..." : "Shorten"}
                  </button>
                </div>
              </div>
            </form>

            {/* Error Message */}
            {error && <div className="p-4 bg-destructive/10 border border-destructive/20 rounded-lg text-destructive">{error}</div>}

            {/* Result */}
            {result && (
              <div className="p-6 border border-border rounded-lg bg-card space-y-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
                <div className="flex items-center justify-between">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm text-muted-foreground mb-1">Your shortened URL</p>
                    <a
                      href={shortUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-2xl font-semibold text-primary hover:underline break-all"
                    >
                      {shortUrl}
                    </a>
                  </div>
                </div>

                <div className="flex gap-2">
                  <button
                    onClick={handleCopy}
                    className="flex-1 px-4 py-3 bg-primary text-primary-foreground rounded-lg font-medium hover:opacity-90 transition-opacity"
                  >
                    {copied ? "✓ Copied!" : "Copy Link"}
                  </button>
                  <a
                    href={shortUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="px-4 py-3 border border-border rounded-lg font-medium hover:bg-accent transition-colors"
                  >
                    Visit →
                  </a>
                </div>

                <div className="pt-2 text-sm text-muted-foreground">
                  <p>
                    Original: <span className="break-all">{result.originalUrl}</span>
                  </p>
                  {result.category && <p className="mt-1">Category: {result.category}</p>}
                  {result.tags && <p className="mt-1">Tags: {result.tags}</p>}
                  {result.expiry && <p className="mt-1">Expires: {new Date(result.expiry).toLocaleString()}</p>}
                </div>
              </div>
            )}
          </div>

          {/* Features */}
          <div className="grid sm:grid-cols-3 gap-4 mt-16">
            <div className="text-center space-y-2">
              <div className="text-3xl">⚡</div>
              <h3 className="font-semibold">Lightning Fast</h3>
              <p className="text-sm text-muted-foreground">Instant redirects</p>
            </div>
            <div className="text-center space-y-2">
              <div className="text-3xl">📊</div>
              <h3 className="font-semibold">Track Clicks</h3>
              <p className="text-sm text-muted-foreground">See your stats</p>
            </div>
            <div className="text-center space-y-2">
              <div className="text-3xl">🎯</div>
              <h3 className="font-semibold">Custom Aliases</h3>
              <p className="text-sm text-muted-foreground">Branded links</p>
            </div>
          </div>

          {/* Dashboard - Show only for authenticated users */}
          {isAuthenticated && user && (
            <div className="mt-16 pt-16 border-t border-border">
              <Dashboard key={dashboardKey} userId={user.id} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
