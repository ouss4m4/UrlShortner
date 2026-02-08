import { useState } from "react";
import { api, type CreateUrlResponse } from "../lib/api";
import { cn } from "../lib/utils";

interface LandingProps {
  onAuthClick: () => void;
  ToastContainer: () => React.ReactElement;
  onSuccess: (message: string) => void;
  onError: (message: string) => void;
}

export function Landing({ onAuthClick, ToastContainer, onSuccess, onError }: LandingProps) {
  const [url, setUrl] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CreateUrlResponse | null>(null);
  const [copied, setCopied] = useState(false);

  const handleShorten = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const data = await api.urls.create({ originalUrl: url });
      setResult(data);
      setUrl("");
      onSuccess("URL shortened successfully!");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Failed to shorten URL";
      onError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const handleCopy = async () => {
    if (result) {
      const shortUrl = `${window.location.origin}/${result.shortCode}`;
      await navigator.clipboard.writeText(shortUrl);
      setCopied(true);
      onSuccess("Copied to clipboard!");
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <ToastContainer />

      {/* Header */}
      <header className="border-b border-border bg-card">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="text-2xl font-bold">🔗 UrlShort</div>
          <button
            onClick={onAuthClick}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:opacity-90 transition-opacity"
          >
            Sign In
          </button>
        </div>
      </header>

      {/* Hero Section */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
        <div className="text-center space-y-6">
          <h1 className="text-5xl sm:text-6xl font-bold tracking-tight">
            Shorten Your Links.
            <br />
            <span className="text-primary">Amplify Your Reach.</span>
          </h1>
          <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
            Create powerful short links with analytics. Track clicks, manage campaigns, and optimize your marketing.
          </p>

          {/* URL Shortener Form */}
          <form onSubmit={handleShorten} className="mt-12 max-w-3xl mx-auto">
            <div className="flex flex-col sm:flex-row gap-3">
              <input
                type="url"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                placeholder="Enter your long URL here..."
                required
                className="flex-1 px-6 py-4 text-lg border-2 border-border rounded-lg bg-background focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
              />
              <button
                type="submit"
                disabled={loading}
                className={cn(
                  "px-8 py-4 bg-primary text-primary-foreground rounded-lg font-semibold text-lg hover:opacity-90 transition-opacity",
                  loading && "opacity-50 cursor-not-allowed",
                )}
              >
                {loading ? "Shortening..." : "Shorten"}
              </button>
            </div>
          </form>

          {/* Result */}
          {result && (
            <div className="mt-8 p-6 border-2 border-primary/20 rounded-lg bg-primary/5 space-y-4">
              <p className="text-sm text-muted-foreground">Your shortened URL</p>
              <a
                href={`${window.location.origin}/${result.shortCode}`}
                target="_blank"
                rel="noopener noreferrer"
                className="text-3xl font-bold text-primary hover:underline"
              >
                {window.location.host}/{result.shortCode}
              </a>
              <div className="flex justify-center gap-3">
                <button
                  onClick={handleCopy}
                  className={cn(
                    "px-6 py-2 border-2 border-primary rounded-lg font-medium hover:bg-primary hover:text-primary-foreground transition-colors",
                    copied && "bg-green-50 border-green-500 text-green-700",
                  )}
                >
                  {copied ? "✓ Copied!" : "Copy Link"}
                </button>
                <button
                  onClick={onAuthClick}
                  className="px-6 py-2 bg-primary text-primary-foreground rounded-lg font-medium hover:opacity-90 transition-opacity"
                >
                  Sign Up to Track Clicks
                </button>
              </div>
            </div>
          )}

          {/* Features */}
          <div className="grid sm:grid-cols-3 gap-8 mt-20 pt-20 border-t border-border">
            <div className="space-y-3">
              <div className="text-4xl">⚡</div>
              <h3 className="text-xl font-semibold">Lightning Fast</h3>
              <p className="text-muted-foreground">Instant redirects with global CDN</p>
            </div>
            <div className="space-y-3">
              <div className="text-4xl">📊</div>
              <h3 className="text-xl font-semibold">Detailed Analytics</h3>
              <p className="text-muted-foreground">Track every click with location data</p>
            </div>
            <div className="space-y-3">
              <div className="text-4xl">🎯</div>
              <h3 className="text-xl font-semibold">Custom Aliases</h3>
              <p className="text-muted-foreground">Branded short links for your business</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
