import { useState, useEffect } from "react";
import { api, type CreateUrlResponse, type UrlAnalytics } from "../lib/api";
import { cn } from "../lib/utils";

interface DashboardProps {
  userId: number;
}

export function Dashboard({ userId }: DashboardProps) {
  const [urls, setUrls] = useState<CreateUrlResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [copiedId, setCopiedId] = useState<number | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [analyticsById, setAnalyticsById] = useState<Record<number, UrlAnalytics | null>>({});
  const [analyticsErrorById, setAnalyticsErrorById] = useState<Record<number, string>>({});
  const [analyticsLoadingId, setAnalyticsLoadingId] = useState<number | null>(null);

  useEffect(() => {
    const loadUrls = async () => {
      try {
        setLoading(true);
        const data = await api.urls.list(userId);
        setUrls(data);
        setError("");
      } catch (err) {
        console.error("Error loading URLs:", err);
        setError(err instanceof Error ? err.message : "Failed to load URLs");
      } finally {
        setLoading(false);
      }
    };

    loadUrls();
  }, [userId]);

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this URL?")) return;

    try {
      await api.urls.delete(id);
      setUrls(urls.filter((url) => url.id !== id));
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to delete");
    }
  };

  const handleCopy = async (shortCode: string, id: number) => {
    const shortUrl = `${import.meta.env.DEV ? "http://localhost:5011" : window.location.origin}/${shortCode}`;
    await navigator.clipboard.writeText(shortUrl);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const toggleAnalytics = async (urlId: number) => {
    // Toggle off
    if (expandedId === urlId) {
      setExpandedId(null);
      return;
    }

    // Expand and load analytics if not cached
    setExpandedId(urlId);

    if (analyticsById[urlId]) {
      // Already cached
      return;
    }

    // Fetch analytics
    setAnalyticsLoadingId(urlId);
    try {
      const data = await api.analytics.url(urlId);
      setAnalyticsById((prev) => ({ ...prev, [urlId]: data }));
      setAnalyticsErrorById((prev) => ({ ...prev, [urlId]: "" }));
    } catch (err) {
      setAnalyticsErrorById((prev) => ({ ...prev, [urlId]: err instanceof Error ? err.message : "Failed to load analytics" }));
    } finally {
      setAnalyticsLoadingId(null);
    }
  };

  const handleToggleAnalytics = async (urlId: number) => {
    if (expandedId === urlId) {
      setExpandedId(null);
      return;
    }

    setExpandedId(urlId);

    if (analyticsById[urlId]) {
      return;
    }

    try {
      setAnalyticsLoadingId(urlId);
      setAnalyticsErrorById((prev) => ({ ...prev, [urlId]: "" }));
      const data = await api.analytics.url(urlId);
      setAnalyticsById((prev) => ({ ...prev, [urlId]: data }));
    } catch (err) {
      setAnalyticsErrorById((prev) => ({
        ...prev,
        [urlId]: err instanceof Error ? err.message : "Failed to load analytics",
      }));
    } finally {
      setAnalyticsLoadingId(null);
    }
  };

  if (loading) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Loading your links...</p>
      </div>
    );
  }

  if (error) {
    return <div className="p-4 bg-destructive/10 border border-destructive/20 rounded-lg text-destructive">{error}</div>;
  }

  if (urls.length === 0) {
    return (
      <div className="text-center py-12 space-y-4">
        <div className="text-4xl">📎</div>
        <h3 className="text-xl font-semibold">No links yet</h3>
        <p className="text-muted-foreground">Create your first short link above to get started</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">Your Links ({urls.length})</h2>

      <div className="space-y-3">
        {urls.map((url) => {
          const shortUrl = `${import.meta.env.DEV ? "http://localhost:5011" : window.location.origin}/${url.shortCode}`;

          return (
            <div key={url.id} className="p-4 border border-border rounded-lg bg-card hover:bg-accent/50 transition-colors">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                {/* URL Info */}
                <div className="flex-1 min-w-0 space-y-1">
                  <a
                    href={shortUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-lg font-semibold text-primary hover:underline break-all"
                  >
                    {shortUrl}
                  </a>
                  <p className="text-sm text-muted-foreground truncate">→ {url.originalUrl}</p>
                  <div className="flex gap-4 text-xs text-muted-foreground">
                    <span> {new Date(url.createdAt).toLocaleDateString()}</span>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex gap-2">
                  <button
                    onClick={() => toggleAnalytics(url.id)}
                    className="px-4 py-2 text-sm border border-border rounded-lg hover:bg-accent transition-colors"
                  >
                    {expandedId === url.id ? "Hide Analytics" : "Show Analytics"}
                  </button>
                  <button
                    onClick={() => handleCopy(url.shortCode, url.id)}
                    className={cn(
                      "px-4 py-2 text-sm border border-border rounded-lg hover:bg-accent transition-colors",
                      copiedId === url.id && "bg-green-50 border-green-300 text-green-700",
                    )}
                  >
                    {copiedId === url.id ? "✓ Copied" : "Copy"}
                  </button>
                  <button
                    onClick={() => handleDelete(url.id)}
                    className="px-4 py-2 text-sm border border-destructive/30 text-destructive rounded-lg hover:bg-destructive/10 transition-colors"
                  >
                    Delete
                  </button>
                </div>
              </div>

              {/* Analytics Section */}
              {expandedId === url.id && (
                <div className="mt-4 pt-4 border-t border-border">
                  {analyticsLoadingId === url.id ? (
                    <p className="text-muted-foreground text-sm">Loading analytics...</p>
                  ) : analyticsErrorById[url.id] ? (
                    <p className="text-destructive text-sm">{analyticsErrorById[url.id]}</p>
                  ) : analyticsById[url.id] ? (
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                      <div className="bg-blue-50 dark:bg-blue-950 rounded-lg p-4">
                        <p className="text-sm font-medium text-muted-foreground mb-1">Total Visits</p>
                        <p className="text-3xl font-bold text-blue-600 dark:text-blue-400">{analyticsById[url.id].totalVisits}</p>
                      </div>
                      <div className="bg-green-50 dark:bg-green-950 rounded-lg p-4">
                        <p className="text-sm font-medium text-muted-foreground mb-1">First Visit</p>
                        <p className="text-sm">
                          {analyticsById[url.id].firstVisit
                            ? new Date(analyticsById[url.id].firstVisit!).toLocaleString()
                            : "No visits yet"}
                        </p>
                      </div>
                      <div className="bg-purple-50 dark:bg-purple-950 rounded-lg p-4">
                        <p className="text-sm font-medium text-muted-foreground mb-1">Last Visit</p>
                        <p className="text-sm">
                          {analyticsById[url.id].lastVisit ? new Date(analyticsById[url.id].lastVisit!).toLocaleString() : "No visits yet"}
                        </p>
                      </div>
                    </div>
                  ) : null}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
