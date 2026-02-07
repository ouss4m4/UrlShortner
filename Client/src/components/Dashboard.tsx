import { useState, useEffect } from "react";
import { api, type CreateUrlResponse } from "../lib/api";
import { cn } from "../lib/utils";

interface DashboardProps {
  userId: number;
}

export function Dashboard({ userId }: DashboardProps) {
  const [urls, setUrls] = useState<CreateUrlResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [copiedId, setCopiedId] = useState<number | null>(null);

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
            </div>
          );
        })}
      </div>
    </div>
  );
}
