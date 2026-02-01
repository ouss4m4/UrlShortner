# Frontend Requirements

## Overview

Simple, lightweight React frontend for URL shortener SaaS. Served from .NET wwwroot (no separate deployment).

## Tech Stack

- **React 18** + **TypeScript**
- **Vite** (fast dev server, optimized builds)
- **Tailwind CSS** v3.4 (utility-first styling)
- **shadcn/ui** (copy-paste component library)
- **NO** routing library initially (single page)
- **NO** state management library (React hooks only)

## Core Features

### 1. Landing Page (Public)

- Hero section with value proposition
- Feature highlights (Fast, Analytics, Custom Aliases)
- CTA buttons (Get Started, Learn More)
- Clean, modern design

### 2. Authentication

- Register form (username, email, password)
- Login form (email, password)
- JWT token storage (localStorage)
- Auto-logout on token expiration

### 3. Dashboard (Authenticated)

- Create short URL form
  - Original URL input
  - Custom alias (optional)
  - Category/tags (optional)
  - Expiration date (optional)
- List of user's URLs
  - Short URL with copy button
  - Original URL
  - Click count
  - Created date
  - Edit/Delete actions
- Simple analytics display
  - Total clicks
  - Recent activity

### 4. URL Management

- Copy short URL to clipboard
- View individual URL analytics
- Delete URLs
- Edit URL (change alias, expiration)

## Non-Functional Requirements

- **Responsive**: Mobile-first design
- **Fast**: <2s initial load, <100ms interactions
- **Accessible**: Keyboard navigation, ARIA labels
- **SEO**: Meta tags for landing page
- **Security**: XSS protection, secure token handling

## Out of Scope (MVP)

- ❌ Bulk URL operations
- ❌ QR code generation
- ❌ Advanced analytics charts
- ❌ Team/collaboration features
- ❌ Custom domains
- ❌ API key management
- ❌ Password reset flow (manual for now)

## API Integration

All API calls to `/api/*` endpoints:

- `/api/auth/register` - POST
- `/api/auth/login` - POST
- `/api/url` - POST, GET
- `/api/url/{id}` - GET, PUT, DELETE
- `/api/analytics/{urlId}` - GET

## Success Metrics

- ✅ Works on mobile and desktop
- ✅ No console errors
- ✅ All API calls succeed
- ✅ Intuitive UX (no documentation needed)
- ✅ Builds successfully in Docker
