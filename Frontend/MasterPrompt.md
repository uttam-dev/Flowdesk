You are working on a production-grade React application built with Vite, TailwindCSS, Redux Toolkit, and Axios.

========================
🎨 UI & DESIGN SYSTEM RULES
========================

1. Colors
- Use ONLY predefined Tailwind theme colors
- Primary: indigo (primary actions)
- Secondary: sky (secondary actions)
- Success: green
- Danger: red
- Warning: amber
- Neutral: gray
- Do NOT use random hex colors

2. Typography
- Font: Inter (or system default if not configured)
- Headings: font-semibold / font-bold
- Body: font-normal
- Maintain consistent text sizes:
  - h1: text-2xl / text-3xl
  - h2: text-xl
  - body: text-sm / text-base

3. Spacing & Layout
- Use Tailwind spacing scale only (p-4, m-2, gap-4, etc.)
- Prefer flex/grid layouts
- Maintain consistent padding across pages (p-4 or p-6)

4. Components
- ALWAYS reuse components from:
  - /components/ui (Button, Input, Modal, etc.)
  - /components/layout (Navbar, Sidebar, Header)
- Do NOT duplicate UI components
- Keep components small and reusable

5. Responsiveness (VERY IMPORTANT)
- Mobile-first approach
- Use breakpoints:
  - sm, md, lg, xl
- Ensure:
  - No horizontal scroll
  - Proper stacking on mobile
  - Buttons & inputs are touch-friendly

6. UX Rules
- Show loading states (spinners/skeletons)
- Show error messages clearly
- Disable buttons during API calls
- Use consistent form validation UI

========================
🏗️ ARCHITECTURE RULES
========================

1. Follow feature-based structure:
- /features/{feature}/
  - api.ts
  - slice.ts
  - pages/
  - components/
  - types.ts

2. Do NOT mix features
3. Keep logic inside feature folders
4. Shared logic goes to:
  - /components
  - /utils
  - /services

========================
⚙️ CODE QUALITY RULES
========================

1. Use functional components only
2. Use hooks (no class components)
3. Use Redux Toolkit (no custom state patterns)
4. Use axios instance (apiClient) for API calls
5. No hardcoded data
6. Proper naming:
   - camelCase for variables
   - PascalCase for components
7. Keep files small and readable
8. Extract reusable logic into hooks

========================
🔒 SAFETY RULES (DO NOT BREAK)
========================

1. Do NOT modify existing business logic
2. Do NOT change API contracts
3. Do NOT rename existing functions/files
4. Do NOT break Redux state shape
5. Ensure backward compatibility

========================
📤 OUTPUT RULES
========================

- Provide only necessary code changes
- Do NOT rewrite full files unless required
- Follow existing patterns in the codebase

========================
🎯 GOAL
========================

Build scalable, consistent, responsive UI while maintaining clean architecture and not breaking existing functionality.