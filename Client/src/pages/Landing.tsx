import { useState } from "react";
import { api, type CreateUrlResponse } from "../lib/api";
import { cn } from "../lib/utils";
import { validateUrl } from "../lib/url";

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
      const validation = validateUrl(url);
      if (!validation.valid || !validation.normalized) {
        onError(validation.message ?? "Invalid URL format");
        return;
      }

      const data = await api.urls.create({ originalUrl: validation.normalized });
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
    <div className="relative min-h-screen overflow-hidden">
      <div className="pointer-events-none absolute -top-40 left-[-10%] h-[28rem] w-[28rem] rounded-full bg-[radial-gradient(circle_at_center,rgba(59,130,246,0.35),transparent_70%)] blur-3xl float-slow" />
      <div className="pointer-events-none absolute top-24 right-[-8%] h-[22rem] w-[22rem] rounded-full bg-[radial-gradient(circle_at_center,rgba(250,204,21,0.35),transparent_70%)] blur-3xl float-fast" />

      <ToastContainer />

      {/* Header */}
      <header className="sticky top-0 z-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="glass rounded-2xl px-5 py-3 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="h-9 w-9 rounded-xl bg-primary/10 text-primary flex items-center justify-center font-semibold">US</div>
              <div>
                <div className="text-lg font-semibold tracking-tight">UrlShort</div>
                <div className="text-xs text-muted-foreground">Modern link control</div>
              </div>
            </div>
            <button
              onClick={onAuthClick}
              className="px-4 py-2 rounded-full bg-primary text-primary-foreground text-sm font-semibold shadow-sm hover:opacity-90 transition-opacity"
            >
              Sign In
            </button>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative z-10 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-24 pt-8">
        <div className="grid lg:grid-cols-[1.1fr_0.9fr] gap-10 items-center">
          <div className="space-y-6">
            <div className="inline-flex items-center gap-2 rounded-full bg-white/70 border border-white/60 px-4 py-2 text-xs font-semibold tracking-wide text-muted-foreground">
              2026-ready link operations
              <span className="h-1.5 w-1.5 rounded-full bg-primary" />
              instant analytics
            </div>
            <h1 className="font-display text-5xl sm:text-6xl leading-tight">
              Links that look sharp,
              <span className="text-primary"> move fast</span>, and stay on-brand.
            </h1>
            <p className="text-lg text-muted-foreground max-w-xl">
              Shorten, organize, and monitor every campaign in one crisp dashboard. Built for speed, tuned for clarity.
            </p>

            <form onSubmit={handleShorten} className="glass-strong rounded-2xl p-4 sm:p-5 neo-outline">
              <div className="flex flex-col sm:flex-row gap-3">
                <input
                  type="text"
                  value={url}
                  onChange={(e) => setUrl(e.target.value)}
                  placeholder="Paste a long URL (we add https://)"
                  inputMode="url"
                  required
                  className="flex-1 px-5 py-3 text-base border border-white/60 rounded-xl bg-white/70 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                />
                <button
                  type="submit"
                  disabled={loading}
                  className={cn(
                    "px-6 py-3 rounded-xl bg-primary text-primary-foreground font-semibold text-base shadow-sm hover:opacity-90 transition-opacity",
                    loading && "opacity-50 cursor-not-allowed",
                  )}
                >
                  {loading ? "Shortening..." : "Shorten"}
                </button>
              </div>
              <div className="mt-3 text-xs text-muted-foreground">We validate domains and block unsafe links.</div>
            </form>

            {result && (
              <div className="glass-strong rounded-2xl p-6 space-y-4">
                <p className="text-xs uppercase tracking-wide text-muted-foreground">Your shortened URL</p>
                <a
                  href={`${window.location.origin}/${result.shortCode}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-2xl sm:text-3xl font-semibold text-primary hover:underline break-all"
                >
                  {window.location.host}/{result.shortCode}
                </a>
                <div className="flex flex-wrap gap-3">
                  <button
                    onClick={handleCopy}
                    className={cn(
                      "px-5 py-2 rounded-full border border-primary/40 text-primary font-medium hover:bg-primary hover:text-primary-foreground transition-colors",
                      copied && "bg-green-50 border-green-400 text-green-700",
                    )}
                  >
                    {copied ? "Copied" : "Copy Link"}
                  </button>
                  <button
                    onClick={onAuthClick}
                    className="px-5 py-2 rounded-full bg-primary text-primary-foreground font-medium hover:opacity-90 transition-opacity"
                  >
                    Sign Up for Analytics
                  </button>
                </div>
              </div>
            )}
          </div>

          <div className="glass rounded-3xl p-8 space-y-6">
            <div>
              <p className="text-xs uppercase tracking-wide text-muted-foreground">Live snapshot</p>
              <h2 className="font-display text-3xl mt-2">Realtime clarity for every link</h2>
            </div>
            <div className="space-y-4">
              <div className="rounded-2xl bg-white/70 border border-white/60 p-4">
                <p className="text-sm text-muted-foreground">Total clicks (24h)</p>
                <p className="text-3xl font-semibold">8,245</p>
              </div>
              <div className="rounded-2xl bg-white/70 border border-white/60 p-4">
                <p className="text-sm text-muted-foreground">Top campaign</p>
                <p className="text-lg font-semibold">Launch-2026</p>
                <p className="text-sm text-muted-foreground">CTR 12.4%</p>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="rounded-2xl bg-white/70 border border-white/60 p-3 text-sm">
                  <p className="text-muted-foreground">Speed</p>
                  <p className="font-semibold">&lt; 200ms</p>
                </div>
                <div className="rounded-2xl bg-white/70 border border-white/60 p-3 text-sm">
                  <p className="text-muted-foreground">Uptime</p>
                  <p className="font-semibold">99.99%</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="grid md:grid-cols-3 gap-6 mt-16">
          {[
            { icon: "⚡", title: "Lightning fast", desc: "Instant redirects with global edge caching." },
            { icon: "📊", title: "Signal-rich analytics", desc: "Clicks, referrers, and geo breakdowns." },
            { icon: "🎯", title: "Custom aliases", desc: "Branded slugs that read clean and sharp." },
          ].map((feature) => (
            <div key={feature.title} className="glass rounded-2xl p-5 text-left">
              <div className="text-3xl">{feature.icon}</div>
              <h3 className="mt-3 text-lg font-semibold">{feature.title}</h3>
              <p className="text-sm text-muted-foreground mt-1">{feature.desc}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
