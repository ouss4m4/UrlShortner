export function UrlSkeleton() {
  return (
    <div className="p-4 border border-border rounded-lg bg-card animate-pulse">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div className="flex-1 space-y-2">
          <div className="h-6 bg-muted rounded w-3/4"></div>
          <div className="h-4 bg-muted rounded w-full"></div>
          <div className="h-3 bg-muted rounded w-24"></div>
        </div>
        <div className="flex gap-2">
          <div className="h-10 w-28 bg-muted rounded-lg"></div>
          <div className="h-10 w-20 bg-muted rounded-lg"></div>
          <div className="h-10 w-20 bg-muted rounded-lg"></div>
        </div>
      </div>
    </div>
  );
}

export function UrlSkeletonList({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <UrlSkeleton key={i} />
      ))}
    </div>
  );
}
