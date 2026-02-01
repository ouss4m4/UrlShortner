# Frontend Architecture

## Technology Decisions

### Why Vite?

- ⚡ Lightning fast HMR (Hot Module Replacement)
- 🚀 Optimized production builds
- 📦 Smaller bundle size than Create React App
- 🛠️ Native ESM support
- ✅ Industry standard in 2026

### Why Tailwind CSS?

- 🎨 Utility-first = no CSS files to maintain
- 📦 PurgeCSS built-in = tiny production bundle
- 🔄 Consistent design system
- 🚀 Fast prototyping
- ✅ Works perfectly with shadcn/ui

### Why shadcn/ui?

- 📋 Copy-paste components (not an npm dependency)
- 🎨 Fully customizable (you own the code)
- ♿ Accessible by default (Radix UI primitives)
- 🎭 Beautiful out of the box
- ✅ No bloat, no lock-in

### Why NO React Router?

- 📄 Single page app initially
- 🚀 Simpler deployment
- 🔄 Can add later if needed
- ✅ Conditional rendering is enough

### Why NO Redux/Zustand?

- 🎯 Simple app, simple state
- ⚡ React hooks (useState, useContext) sufficient
- 📦 Smaller bundle size
- ✅ YAGNI (You Aren't Gonna Need It)

## Folder Structure

```
Client/
├── docs/                    # This documentation
│   ├── REQUIREMENTS.md
│   ├── ARCHITECTURE.md
│   ├── PROCESS.md
│   └── CURRENT_STATUS.md
├── public/                  # Static assets
│   └── favicon.ico
├── src/
│   ├── components/          # React components
│   │   ├── ui/             # shadcn components
│   │   ├── Landing.tsx     # Landing page
│   │   ├── Auth.tsx        # Login/Register
│   │   └── Dashboard.tsx   # Main app
│   ├── lib/
│   │   ├── utils.ts        # cn() helper
│   │   └── api.ts          # API client
│   ├── hooks/              # Custom hooks
│   │   └── useAuth.ts      # Auth state
│   ├── App.tsx             # Main app component
│   ├── main.tsx            # Entry point
│   └── index.css           # Tailwind + theme
├── .gitignore
├── package.json
├── tailwind.config.js
├── tsconfig.json
└── vite.config.ts
```

## Data Flow

```
User Action
    ↓
Component Event Handler
    ↓
API Call (lib/api.ts)
    ↓
.NET Backend (/api/*)
    ↓
Response
    ↓
Update React State
    ↓
Re-render Component
```

## State Management Strategy

### Auth State

- Stored in: `localStorage` (JWT token)
- Accessed via: `useAuth()` custom hook
- Shared via: Context API if needed

### URL List State

- Stored in: Component state (`useState`)
- Fetched on mount
- Optimistic updates on create/delete

### Form State

- Controlled components
- Local state only
- Validation on submit

## API Client Design

```typescript
// lib/api.ts
const API_BASE = "/api";

async function request(endpoint, options) {
  const token = localStorage.getItem("token");
  const headers = {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.json();
}

export const api = {
  auth: {
    register: (data) => request("/auth/register", { method: "POST", body: JSON.stringify(data) }),
    login: (data) => request("/auth/login", { method: "POST", body: JSON.stringify(data) }),
  },
  urls: {
    create: (data) => request("/url", { method: "POST", body: JSON.stringify(data) }),
    list: () => request("/url"),
    get: (id) => request(`/url/${id}`),
    delete: (id) => request(`/url/${id}`, { method: "DELETE" }),
  },
};
```

## Build & Deployment

### Development

```bash
npm run dev  # Vite dev server on :5173
```

### Production Build

```bash
npm run build  # Output to dist/
```

### Integration with .NET

1. Vite builds to `dist/`
2. Dockerfile copies `dist/` to `API/wwwroot/`
3. .NET serves static files from wwwroot
4. SPA fallback: all routes → `index.html`

## Performance Considerations

### Bundle Size

- Vite code-splitting by default
- Tailwind PurgeCSS removes unused styles
- Target: <200KB initial JS bundle

### Lazy Loading

- Not needed initially (small app)
- Can add `React.lazy()` later for routes

### Caching

- Vite generates hashed filenames
- .NET serves with cache headers
- Service Worker optional for offline support

## Security

### XSS Prevention

- React escapes by default
- Don't use `dangerouslySetInnerHTML`
- Sanitize any user-generated HTML

### JWT Handling

- Store in `localStorage` (acceptable for MVP)
- Consider `httpOnly` cookie later
- Auto-clear on 401 response

### CORS

- Not needed (same origin after deployment)
- .NET serves both API and frontend

## Accessibility

### ARIA Labels

- All interactive elements have labels
- Form fields have proper labels
- Error messages announced

### Keyboard Navigation

- Tab order is logical
- Enter key submits forms
- Esc key closes modals

### Color Contrast

- shadcn/ui passes WCAG AA
- Test with accessibility devtools
