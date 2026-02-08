const knownTlds = new Set([
  "com",
  "net",
  "org",
  "io",
  "co",
  "dev",
  "app",
  "ai",
  "me",
  "xyz",
  "info",
  "biz",
  "edu",
  "gov",
  "mil",
  "us",
  "uk",
  "ca",
  "de",
  "fr",
  "es",
  "it",
  "nl",
  "se",
  "no",
  "fi",
  "dk",
  "ch",
  "at",
  "ie",
  "be",
  "pl",
  "cz",
  "sk",
  "hu",
  "ro",
  "pt",
  "gr",
  "tr",
  "il",
  "sa",
  "ae",
  "in",
  "pk",
  "bd",
  "lk",
  "np",
  "cn",
  "jp",
  "kr",
  "sg",
  "hk",
  "tw",
  "vn",
  "th",
  "my",
  "id",
  "ph",
  "au",
  "nz",
  "br",
  "mx",
  "ar",
  "cl",
  "co",
  "pe",
  "uy",
  "za",
  "ng",
  "ke",
  "gg",
  "to",
  "tv",
  "site",
  "online",
  "store",
  "tech",
  "cloud",
  "studio",
]);

function isPrivateIp(host: string): boolean {
  const ipv4Match = host.match(/^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/);
  if (!ipv4Match) return false;

  const [, a, b] = ipv4Match.map(Number);
  if (a === 10) return true;
  if (a === 172 && b >= 16 && b <= 31) return true;
  if (a === 192 && b === 168) return true;
  return false;
}

function isValidHost(host: string): boolean {
  if (!host) return false;
  const lower = host.toLowerCase();
  if (lower === "localhost" || lower === "127.0.0.1" || lower === "0.0.0.0" || lower === "::1") {
    return false;
  }

  if (isPrivateIp(host)) return false;

  if (host.includes(".") === false) return false;
  const labels = host.split(".").filter(Boolean);
  if (labels.length < 2) return false;

  for (const label of labels) {
    if (!/^[a-z0-9-]{1,63}$/i.test(label)) return false;
    if (label.startsWith("-") || label.endsWith("-")) return false;
  }

  const tld = labels[labels.length - 1];
  if (!/^[a-z]{2,63}$/i.test(tld)) return false;
  return knownTlds.has(tld.toLowerCase());
}

export function normalizeUrl(input: string): string {
  const trimmed = input.trim();
  if (!trimmed) return trimmed;
  if (!trimmed.includes("://")) {
    return `https://${trimmed}`;
  }
  return trimmed;
}

export function validateUrl(input: string): { valid: boolean; message?: string; normalized?: string } {
  if (!input || !input.trim()) {
    return { valid: false, message: "URL cannot be empty" };
  }

  const normalized = normalizeUrl(input);
  if (normalized.length > 2048) {
    return { valid: false, message: "URL exceeds maximum length of 2048 characters" };
  }

  if (/\p{C}/u.test(normalized)) {
    return { valid: false, message: "Invalid URL format: contains control characters" };
  }

  let url: URL;
  try {
    url = new URL(normalized);
  } catch {
    return { valid: false, message: "Invalid URL format" };
  }

  if (url.protocol !== "http:" && url.protocol !== "https:") {
    return { valid: false, message: "URL must use HTTP or HTTPS protocol" };
  }

  if (!isValidHost(url.hostname)) {
    return { valid: false, message: "Invalid URL format: invalid or unsupported domain" };
  }

  return { valid: true, normalized };
}
