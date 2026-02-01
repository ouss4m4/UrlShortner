# Frontend Development Process

## Phase 1: Foundation ✅ COMPLETE

**Goal**: Basic project setup with Tailwind + shadcn

### Tasks Completed

- [x] Create Vite + React + TypeScript project
- [x] Install Tailwind CSS v3.4
- [x] Configure Tailwind (tailwind.config.js, postcss.config.js)
- [x] Set up shadcn/ui (components.json, utils.ts)
- [x] Create index.css with theme variables
- [x] Configure path aliases (@/\* imports)
- [x] Create simple landing page
- [x] Test dev server (npm run dev)

### Deliverables

- ✅ Client/ folder with Vite project
- ✅ Tailwind CSS working
- ✅ Landing page displays at localhost:5173
- ✅ Documentation structure (this file!)

---

## Phase 2: Authentication UI

**Goal**: Login and Register forms

### Tasks

- [ ] Create Auth component
  - [ ] Register form (username, email, password)
  - [ ] Login form (email, password)
  - [ ] Toggle between register/login
  - [ ] Form validation
  - [ ] Error display
- [ ] Create API client (lib/api.ts)
  - [ ] request() helper function
  - [ ] auth.register()
  - [ ] auth.login()
- [ ] Create useAuth hook
  - [ ] Store JWT in localStorage
  - [ ] Provide isAuthenticated state
  - [ ] Auto-logout on token expiration
- [ ] Update App.tsx
  - [ ] Show Landing for unauthenticated
  - [ ] Show Auth forms on "Get Started"
  - [ ] Show Dashboard when authenticated

### Testing

```bash
# Test registration
curl -X POST http://localhost:5173/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Test123!"}'

# Test login
curl -X POST http://localhost:5173/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'
```

### Deliverables

- Auth component with forms
- API client integrated
- Token storage working
- Can login/register successfully

---

## Phase 3: Dashboard UI

**Goal**: Create and manage URLs

### Tasks

- [ ] Create Dashboard component
  - [ ] Header with user info and logout
  - [ ] Create URL form
    - [ ] Original URL input
    - [ ] Custom alias input (optional)
    - [ ] Submit button
  - [ ] URL list display
    - [ ] Show short URL with copy button
    - [ ] Show original URL
    - [ ] Show click count
    - [ ] Show created date
    - [ ] Delete button
- [ ] Add API methods
  - [ ] urls.create()
  - [ ] urls.list()
  - [ ] urls.delete()
- [ ] Implement copy to clipboard
- [ ] Add loading states
- [ ] Add error handling

### Testing

- Create a URL
- Copy short link
- Delete a URL
- Verify API calls work

### Deliverables

- Working dashboard
- Can create/list/delete URLs
- Copy button works
- Responsive on mobile

---

## Phase 4: .NET Integration

**Goal**: Serve frontend from .NET backend

### Tasks

- [ ] Update .NET Program.cs
  - [ ] Add `app.UseStaticFiles()`
  - [ ] Add `app.MapFallbackToFile("index.html")`
- [ ] Update Dockerfile
  - [ ] Add Node stage to build React app
  - [ ] Copy dist/ to API/wwwroot/
- [ ] Update .dockerignore
  - [ ] Exclude Client/node_modules
  - [ ] Exclude Client/dist
- [ ] Test locally
  - [ ] Build Client: `npm run build`
  - [ ] Copy to wwwroot: `cp -r Client/dist/* API/wwwroot/`
  - [ ] Run .NET: `dotnet run --project API`
  - [ ] Access at http://localhost:5000

### Testing

```bash
# Local test
cd Client && npm run build
cp -r dist/* ../API/wwwroot/
cd ../API && dotnet run

# Visit http://localhost:5000
# Should see React app served by .NET
```

### Deliverables

- Frontend served by .NET locally
- Dockerfile builds both
- Single deployment unit

---

## Phase 5: Railway Deployment

**Goal**: Deploy integrated app to Railway

### Tasks

- [ ] Commit all changes
- [ ] Push to GitHub
- [ ] Railway auto-deploys
- [ ] Test production URL
- [ ] Verify API calls work
- [ ] Test on mobile device

### Testing

```bash
# Test production
curl https://urlshortner-production-ae23.up.railway.app/
# Should return React app HTML

curl https://urlshortner-production-ae23.up.railway.app/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"prod","email":"prod@test.com","password":"Test123!"}'
```

### Deliverables

- ✅ Full app deployed on Railway
- ✅ Frontend + Backend working together
- ✅ Public URL accessible
- ✅ MVP COMPLETE! 🎉

---

## Phase 6: Polish (Optional)

**Goal**: Improve UX and add nice-to-haves

### Tasks

- [ ] Add shadcn Button, Input, Card components
- [ ] Add loading spinners
- [ ] Add success/error toasts
- [ ] Add URL validation
- [ ] Add analytics page
- [ ] Add dark mode toggle
- [ ] Add favicon
- [ ] Add meta tags for SEO

---

## Development Workflow

### Daily Routine

1. Pull latest from GitHub
2. `cd Client && npm run dev`
3. Open http://localhost:5173
4. Make changes (hot reload)
5. Test in browser
6. Commit when feature works
7. Push to GitHub

### Before Deploying

1. `npm run build` - verify no errors
2. Test production build locally
3. Commit to GitHub
4. Railway auto-deploys
5. Test production URL
6. Monitor Railway logs

### Debugging

- Use React DevTools
- Check browser console
- Check Network tab for API calls
- Check Railway logs for backend errors

---

## Best Practices

### Component Design

- Keep components small (<200 lines)
- One component per file
- Use TypeScript interfaces
- Destructure props

### Styling

- Use Tailwind classes only
- Group related classes
- Use responsive prefixes (md:, lg:)
- Extract repeated patterns to components

### State Management

- Lift state up when needed
- Use useEffect for side effects
- Clean up subscriptions
- Avoid prop drilling (use Context if needed)

### API Calls

- Always handle errors
- Show loading states
- Use try/catch
- Display user-friendly messages

### Performance

- Don't premature optimize
- Use React.memo() sparingly
- Avoid inline functions in render
- Measure before optimizing
